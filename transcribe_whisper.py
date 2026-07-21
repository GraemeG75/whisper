import argparse
import json
import math
import re
import subprocess
import sys
import warnings
from pathlib import Path
from typing import List, Tuple

import torch
import whisper

warnings.filterwarnings("ignore", message="FP16 is not supported on CPU*")

DEFAULT_DISPATCH_PROMPT = (
    "Public safety and radio dispatch audio. Keep unit IDs, addresses, street names, cross streets, "
    "medical and fire call types, and number sequences accurate. Prefer literal transcription over paraphrase."
)

def normalize_segment_text(text: str) -> str:
    return " ".join(text.lower().strip().split())

def should_skip_segment(segment: dict) -> bool:
    text = segment.get("text", "").strip()
    avg_logprob = float(segment.get("avg_logprob", -99.0))
    no_speech_prob = float(segment.get("no_speech_prob", 0.0))
    if not text:
        return True
    # Drop likely hallucinated low-confidence fragments.
    # Tighter thresholds to catch "Thank you" and other common hallucinations.
    if avg_logprob < -0.95 or (avg_logprob < -0.6 and no_speech_prob > 0.25):
        return True
    if no_speech_prob > 0.5:
        return True
    return False

def is_likely_hallucination(text: str, avg_logprob: float, no_speech_prob: float) -> bool:
    """Check if a segment is likely a hallucinated common phrase."""
    norm = normalize_segment_text(text)
    
    # Very short common false positives - skip regardless of confidence
    very_common_hallucinations = {
        "thank you",
        "okay",
        "roger",
        "yes",
    }
    if norm in very_common_hallucinations and len(norm.split()) <= 2:
        return True
    
    # Longer hallucinations - only skip if low confidence
    prompt_hallucinations = {
        "thank you all",
        "thank you very much",
        "keep unit ids addresses street names cross streets medical and fire calls accurate",
        "keep unit ids addresses street names cross streets medical and fire call types and number sequences accurate prefer literal transcription over paraphrase",
        "public safety and radio dispatch audio keep unit ids addresses street names cross streets medical and fire call types and number sequences accurate prefer literal transcription over paraphrase",
    }
    if norm in prompt_hallucinations:
        if avg_logprob < -0.7 or no_speech_prob > 0.35:
            return True
    
    return False

def detect_repeating_loop(recent_texts: List[str], window_size: int = 10) -> bool:
    """Detect if the last N segments form a repeating or alternating loop.
    
    Examples:
    - ["a", "b", "a", "b", "a", "b"] -> True (alternating pattern)
    - ["x", "x", "x", "x"] -> True (same repeated)
    """
    if len(recent_texts) < window_size:
        return False
    
    recent = recent_texts[-window_size:]
    
    # Check for exact repetition (all same)
    if len(set(recent)) == 1:
        return True
    
    # Check for alternating two-phrase pattern (A, B, A, B, A, B, ...)
    if len(set(recent)) == 2:
        # If we have exactly 2 unique phrases alternating most of the time
        unique_phrases = list(set(recent))
        alternation_count = 0
        for i in range(1, len(recent)):
            if recent[i] != recent[i - 1]:
                alternation_count += 1
        # If it alternates for at least 70% of the window, it's a loop
        if alternation_count >= len(recent) * 0.7:
            return True
    
    return False

def normalize_and_amplify_audio(input_file: Path, temp_dir: Path) -> Path:
    """Apply basic normalization, noise reduction, and amplification to audio."""
    temp_dir.mkdir(parents=True, exist_ok=True)
    output_file = temp_dir / f"{input_file.stem}_normalized.wav"
    ffmpeg_command = [
        "ffmpeg",
        "-y",
        "-i",
        str(input_file),
        "-ac",
        "1",
        "-ar",
        "16000",
        "-af",
        "highpass=f=100,anlmdn=s=0.003:p=0.002,loudnorm=I=-20:TP=-3:LRA=4,volume=1.6",
        str(output_file),
    ]
    result = subprocess.run(ffmpeg_command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        print(f"    Warning: Audio normalization failed: {result.stderr[-500:]}")
        return input_file
    return output_file

def extract_audio_from_mp4(input_file: Path, temp_dir: Path) -> Path:
    """Extract audio from MP4 file and convert to mono WAV format.
    
    This handles MP4 files separately to avoid video stream issues.
    Includes noise reduction during extraction for better quality.
    """
    temp_dir.mkdir(parents=True, exist_ok=True)
    output_file = temp_dir / f"{input_file.stem}_extracted.wav"
    ffmpeg_command = [
        "ffmpeg",
        "-y",
        "-i",
        str(input_file),
        "-vn",  # No video
        "-acodec",
        "pcm_s16le",
        "-ar",
        "16000",
        "-ac",
        "1",
        "-af",
        "highpass=f=80,anlmdn=s=0.004:p=0.0015,volume=1.5",
        str(output_file),
    ]
    result = subprocess.run(ffmpeg_command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        print(f"    Warning: MP4 audio extraction failed: {result.stderr[-500:]}")
        return input_file
    return output_file

def preprocess_audio_ffmpeg(input_file: Path, temp_dir: Path) -> Path:
    """Apply heavy preprocessing: aggressive noise reduction + bandpass + amplification."""
    temp_dir.mkdir(parents=True, exist_ok=True)
    output_file = temp_dir / f"{input_file.stem}_clean.wav"
    ffmpeg_command = [
        "ffmpeg",
        "-y",
        "-i",
        str(input_file),
        "-ac",
        "1",
        "-ar",
        "16000",
        "-af",
        "highpass=f=120,lowpass=f=3500,anlmdn=s=0.005:p=0.001,afftdn,loudnorm=I=-20:TP=-3:LRA=4,volume=1.8",
        str(output_file),
    ]
    result = subprocess.run(ffmpeg_command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        print(f"    Warning: Heavy preprocessing failed: {result.stderr[-500:]}")
        return input_file
    return output_file

def detect_silences(audio_file: Path, silence_db: float, min_silence_sec: float) -> List[Tuple[float, float]]:
    command = [
        "ffmpeg",
        "-hide_banner",
        "-i",
        str(audio_file),
        "-af",
        f"silencedetect=noise={silence_db}dB:d={min_silence_sec}",
        "-f",
        "null",
        "-",
    ]
    result = subprocess.run(command, stdout=subprocess.DEVNULL, stderr=subprocess.PIPE, text=True)
    if result.returncode not in (0, 1):
        return []

    text = result.stderr or ""
    starts = [float(m.group(1)) for m in re.finditer(r"silence_start:\s*([0-9.]+)", text)]
    ends = [float(m.group(1)) for m in re.finditer(r"silence_end:\s*([0-9.]+)", text)]
    silences = []
    for i, start in enumerate(starts):
        if i < len(ends) and ends[i] >= start:
            silences.append((start, ends[i]))
    return silences

def get_audio_duration_seconds(audio_file: Path) -> float:
    command = [
        "ffprobe",
        "-v",
        "error",
        "-show_entries",
        "format=duration",
        "-of",
        "default=noprint_wrappers=1:nokey=1",
        str(audio_file),
    ]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.DEVNULL, text=True)
    if result.returncode != 0:
        return 0.0
    try:
        return float(result.stdout.strip())
    except ValueError:
        return 0.0

def build_chunk_ranges(
    duration: float,
    silences: List[Tuple[float, float]],
    target_chunk_sec: float,
    max_chunk_sec: float,
) -> List[Tuple[float, float]]:
    if duration <= 0:
        return [(0.0, 0.0)]

    if duration <= max_chunk_sec:
        return [(0.0, duration)]

    ranges: List[Tuple[float, float]] = []
    start = 0.0
    while start < duration:
        desired = start + target_chunk_sec
        hard_limit = min(start + max_chunk_sec, duration)
        cut = hard_limit

        for silence_start, silence_end in silences:
            if silence_start < desired:
                continue
            if silence_start > hard_limit:
                break
            # Cut at the start of a nearby silence to avoid mid-word splits.
            cut = silence_start
            break

        if cut <= start:
            cut = min(start + max_chunk_sec, duration)

        ranges.append((start, cut))
        start = cut

    return ranges

def export_audio_chunk(source_audio: Path, chunk_file: Path, start_sec: float, end_sec: float) -> bool:
    command = [
        "ffmpeg",
        "-y",
        "-i",
        str(source_audio),
        "-ss",
        f"{start_sec:.3f}",
        "-to",
        f"{end_sec:.3f}",
        "-ac",
        "1",
        "-ar",
        "16000",
        str(chunk_file),
    ]
    result = subprocess.run(command, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    return result.returncode == 0

def chunk_audio_by_silence(
    source_audio: Path,
    chunk_root: Path,
    silence_db: float,
    min_silence_sec: float,
    target_chunk_sec: float,
    max_chunk_sec: float,
) -> List[Tuple[Path, float]]:
    duration = get_audio_duration_seconds(source_audio)
    silences = detect_silences(source_audio, silence_db=silence_db, min_silence_sec=min_silence_sec)
    ranges = build_chunk_ranges(duration, silences, target_chunk_sec=target_chunk_sec, max_chunk_sec=max_chunk_sec)

    chunk_dir = chunk_root / source_audio.stem
    chunk_dir.mkdir(parents=True, exist_ok=True)

    chunk_paths: List[Tuple[Path, float]] = []
    for i, (start_sec, end_sec) in enumerate(ranges, start=1):
        if end_sec - start_sec < 0.3:
            continue
        chunk_file = chunk_dir / f"chunk_{i:04d}.wav"
        if export_audio_chunk(source_audio, chunk_file, start_sec, end_sec):
            chunk_paths.append((chunk_file, start_sec))

    if not chunk_paths:
        return [(source_audio, 0.0)]
    return chunk_paths

def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Batch transcribe audio files with Whisper.")
    parser.add_argument("--input-folder", default="audio_files", help="Folder containing audio files")
    parser.add_argument("--output-folder", default="transcripts", help="Folder for transcript files")
    parser.add_argument("--language", default="en", help="Whisper language code")
    parser.add_argument("--model", default="large", help="Whisper model name")
    parser.add_argument(
        "--extensions",
        nargs="+",
        default=[".mp3", ".wav", ".m4a", ".flac", ".mp4"],
        help="Audio file extensions to process",
    )
    parser.add_argument(
        "--preprocess",
        action="store_true",
        help="Pre-clean audio with ffmpeg (noise reduction + leveling)",
    )
    parser.add_argument(
        "--chunk-on-silence",
        action="store_true",
        help="Split long audio near silence before transcription",
    )
    parser.add_argument("--silence-db", type=float, default=-32.0, help="Silence threshold in dB for split detection")
    parser.add_argument("--min-silence", type=float, default=0.45, help="Minimum silence duration (seconds)")
    parser.add_argument("--target-chunk-sec", type=float, default=55.0, help="Preferred chunk duration in seconds")
    parser.add_argument("--max-chunk-sec", type=float, default=80.0, help="Hard maximum chunk duration in seconds")
    parser.add_argument(
        "--prompt",
        default=DEFAULT_DISPATCH_PROMPT,
        help="Initial prompt to bias transcript vocabulary",
    )
    parser.add_argument(
        "--detailed-output",
        action="store_true",
        help="Write a TSV and JSONL file with timestamps and confidence per segment",
    )
    parser.add_argument(
        "--fast",
        action="store_true",
        help="Enable fast mode: skip preprocessing, enable silence-based chunking, reduce beam search",
    )
    return parser

def transcribe_file(model, audio_path: Path, language: str, prompt: str, beam_size: int = 5, best_of: int = 5) -> dict:
    return model.transcribe(
        str(audio_path),
        language=language,
        verbose=False,
        word_timestamps=False,
        initial_prompt=prompt,
        temperature=(0.0, 0.2, 0.4),
        beam_size=beam_size,
        best_of=best_of,
        patience=1.2,
        condition_on_previous_text=False,
        compression_ratio_threshold=2.2,
        logprob_threshold=-0.8,
        no_speech_threshold=0.35,
    )

def cleanup_temp_files(file_path: Path, temp_dir: Path) -> None:
    """Delete temporary WAV files created during processing."""
    stem = file_path.stem
    files_to_remove = [
        temp_dir / f"{stem}_normalized.wav",
        temp_dir / f"{stem}_clean.wav",
        temp_dir / f"{stem}_extracted.wav",
    ]
    for temp_file in files_to_remove:
        if temp_file.exists():
            try:
                temp_file.unlink()
            except Exception as e:
                print(f"Warning: Could not delete {temp_file}: {e}")
    
    chunk_dir = temp_dir / "chunks" / stem
    if chunk_dir.exists():
        try:
            import shutil
            shutil.rmtree(chunk_dir)
        except Exception as e:
            print(f"Warning: Could not delete chunk directory {chunk_dir}: {e}")

def format_ts(seconds: float) -> str:
    total_ms = max(0, int(round(seconds * 1000.0)))
    hours = total_ms // 3600000
    minutes = (total_ms % 3600000) // 60000
    secs = (total_ms % 60000) // 1000
    millis = total_ms % 1000
    return f"{hours:02d}:{minutes:02d}:{secs:02d}.{millis:03d}"

def write_detailed_outputs(output_base: Path, kept_segments: List[dict]) -> None:
    tsv_file = output_base.with_suffix(".segments.tsv")
    jsonl_file = output_base.with_suffix(".segments.jsonl")

    with open(tsv_file, "w", encoding="utf-8") as f_tsv:
        f_tsv.write("start\tend\tavg_logprob\tno_speech_prob\tlow_conf\ttext\n")
        for seg in kept_segments:
            avg_logprob = float(seg.get("avg_logprob", -99.0))
            no_speech_prob = float(seg.get("no_speech_prob", 0.0))
            low_conf = avg_logprob < -0.8 or no_speech_prob > 0.35
            line = (
                f"{format_ts(float(seg.get('start', 0.0)))}\t"
                f"{format_ts(float(seg.get('end', 0.0)))}\t"
                f"{avg_logprob:.3f}\t{no_speech_prob:.3f}\t{int(low_conf)}\t"
                f"{seg.get('text', '').strip()}\n"
            )
            f_tsv.write(line)

    with open(jsonl_file, "w", encoding="utf-8") as f_jsonl:
        for seg in kept_segments:
            row = {
                "start": float(seg.get("start", 0.0)),
                "end": float(seg.get("end", 0.0)),
                "start_hms": format_ts(float(seg.get("start", 0.0))),
                "end_hms": format_ts(float(seg.get("end", 0.0))),
                "avg_logprob": float(seg.get("avg_logprob", -99.0)),
                "no_speech_prob": float(seg.get("no_speech_prob", 0.0)),
                "text": seg.get("text", "").strip(),
            }
            f_jsonl.write(json.dumps(row, ensure_ascii=True) + "\n")

def main() -> None:
    args = build_parser().parse_args()
    
    # Apply fast mode optimizations
    if args.fast:
        args.chunk_on_silence = True
        args.preprocess = False
        beam_size = 1
        best_of = 1
        print("Fast mode enabled: chunking on silence, reduced beam search")
    else:
        beam_size = 5
        best_of = 5
    
    input_folder = Path(args.input_folder)
    output_folder = Path(args.output_folder)
    output_folder.mkdir(parents=True, exist_ok=True)
    temp_dir = Path("processed")
    temp_dir.mkdir(parents=True, exist_ok=True)

    device = "cuda" if torch.cuda.is_available() else "cpu"
    print("CUDA available:", torch.cuda.is_available())
    print("Using device:", torch.cuda.get_device_name(0) if torch.cuda.is_available() else "CPU")

    model = whisper.load_model(args.model).to(device)
    exts = {ext.lower() if ext.startswith(".") else f".{ext.lower()}" for ext in args.extensions}
    files = [p for p in sorted(input_folder.iterdir()) if p.is_file() and p.suffix.lower() in exts]

    if not files:
        print(f"No input files found in '{input_folder}' for extensions: {sorted(exts)}")
        return

    for file_path in files:
        print(f"\nProcessing: {file_path.name}")
        
        # MP4 files need audio extraction first to avoid video stream issues
        source_audio = file_path
        if file_path.suffix.lower() == ".mp4":
            print("  Extracting audio from MP4...")
            source_audio = extract_audio_from_mp4(file_path, temp_dir)
        
        print("  Normalizing and amplifying audio...")
        source_audio = normalize_and_amplify_audio(source_audio, temp_dir)
        
        if args.preprocess:
            print("  Applying heavy preprocessing (noise reduction, bandpass filter)...")
            source_audio = preprocess_audio_ffmpeg(source_audio, temp_dir)

        chunks: List[Tuple[Path, float]] = [(source_audio, 0.0)]
        if args.chunk_on_silence:
            chunks = chunk_audio_by_silence(
                source_audio,
                chunk_root=temp_dir / "chunks",
                silence_db=args.silence_db,
                min_silence_sec=args.min_silence,
                target_chunk_sec=args.target_chunk_sec,
                max_chunk_sec=args.max_chunk_sec,
            )
            print(f"Detected {len(chunks)} chunk(s) for {file_path.name}")

        print(f"\nTranscribing: {source_audio}...")
        merged_segments: List[dict] = []
        total = len(chunks)
        for i, (chunk_file, offset_sec) in enumerate(chunks, start=1):
            result = transcribe_file(model, chunk_file, args.language, args.prompt, beam_size=beam_size, best_of=best_of)
            chunk_segments = result.get("segments", [])
            for seg in chunk_segments:
                adj = dict(seg)
                adj["start"] = float(seg.get("start", 0.0)) + offset_sec
                adj["end"] = float(seg.get("end", 0.0)) + offset_sec
                merged_segments.append(adj)

            percent = i / total * 100 if total > 0 else 100.0
            sys.stdout.write(f"\rChunk progress: {percent:.1f}%")
            sys.stdout.flush()

        segments = merged_segments
        last_end = 0.0
        transcript_parts = []
        kept_segments: List[dict] = []
        last_norm = ""
        repeat_count = 0
        recent_norms: List[str] = []

        for seg in segments:
            if should_skip_segment(seg):
                continue

            text = seg.get("text", "").strip()
            avg_logprob = float(seg.get("avg_logprob", -99.0))
            no_speech_prob = float(seg.get("no_speech_prob", 0.0))
            
            # Check for likely hallucinated common phrases.
            if is_likely_hallucination(text, avg_logprob, no_speech_prob):
                continue
            
            norm = normalize_segment_text(text)
            recent_norms.append(norm)
            if len(recent_norms) > 10:
                recent_norms.pop(0)
            
            # Detect repeating/alternating loops and skip if detected.
            if detect_repeating_loop(recent_norms, window_size=10):
                continue
            
            if norm == last_norm:
                repeat_count += 1
                # Skip repeated short segments after the first.
                if len(norm.split()) <= 6 and repeat_count >= 1:
                    continue
            else:
                last_norm = norm
                repeat_count = 0

            gap = float(seg.get("start", 0.0)) - last_end
            if gap > 1.0 and transcript_parts:
                transcript_parts.append("\n")

            start_ts = format_ts(float(seg.get("start", 0.0)))
            transcript_parts.append(f"[{start_ts}] ")
            transcript_parts.append(text)
            transcript_parts.append("\n")
            last_end = float(seg.get("end", last_end))
            kept_segments.append(seg)

        output_filename = output_folder / f"{file_path.stem}_{args.language}.txt"
        with open(output_filename, "w", encoding="utf-8") as f:
            f.write("".join(transcript_parts).strip())

        print(f"\nSaved to: {output_filename}")
        if args.detailed_output:
            write_detailed_outputs(output_filename, kept_segments)
            print(f"Saved segment diagnostics: {output_filename.with_suffix('.segments.tsv')}")
        
        # Clean up temporary files
        print("Cleaning up temporary files...")
        cleanup_temp_files(file_path, temp_dir)

    print("\nAll files transcribed.")


if __name__ == "__main__":
    main()
