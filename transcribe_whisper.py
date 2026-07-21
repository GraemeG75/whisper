import whisper
import warnings
import os
import shutil
import sys
from pathlib import Path
from voicefixer import VoiceFixer
import torch
import subprocess

print("CUDA available:", torch.cuda.is_available())
print("Using device:", torch.cuda.get_device_name(0) if torch.cuda.is_available() else "CPU")

lang= "en"  # Default language for transcription

warnings.filterwarnings("ignore", message="FP16 is not supported on CPU*")

# Load a more accurate Whisper model
model = whisper.load_model("large").to("cuda")

os.makedirs("processed", exist_ok=True)

# Folder containing your MP3 files
input_folder = "audio_files"  # Change this to your folder path

# Make sure output folder exists (optional)
output_folder = "transcripts"
os.makedirs(output_folder, exist_ok=True)

# Process all .mp3 files in the input folder
for file_path in Path(input_folder).glob("*.mp3"):    

    input_file = str(file_path)
    enhanced_audio = input_file
    enhanced_audio_fixed = input_file
    '''
    enhanced_audio = Path(input_folder) / (file_path.stem + ".wav")
    enhanced_audio_fixed = Path(input_folder) / (file_path.stem + "_fixed.wav")

    # Step 1: Extract and enhance audio using FFmpeg
    ffmpeg_command = [
        "ffmpeg", "-y", "-i", input_file,
        "-af", "highpass=f=100, lowpass=f=3500, dynaudnorm",
        enhanced_audio
    ]
    subprocess.run(ffmpeg_command, stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT)

    fixer = VoiceFixer()
    fixer.restore(input=enhanced_audio, output=enhanced_audio_fixed, cuda=True, mode=2
    '''
    print("✅ Audio enhanced and extracted")

    #enhanced_audio_fixed = enhanced_audio

    print(f"Transcribing: {enhanced_audio_fixed}...")

    # Transcribe
    result = model.transcribe(str(enhanced_audio_fixed), verbose=False, word_timestamps=False, logprob_threshold=-1.0, no_speech_threshold=0.6, language=lang)

    # Estimate total duration from segments
    segments = result["segments"]
    total = len(segments)
    transcript = ""
    last_end = 0.0

    print("\nTranscribing...\n")
    for i, seg in enumerate(segments):
        gap = seg['start'] - last_end
        if gap > 1.0:
            transcript += "\n"  # Add a line break if there's a long pause

        # Add segment text
        transcript += seg['text'].strip() + " "
        
        last_end = seg['end']

        # Display progress
        percent = (i + 1) / total * 100
        sys.stdout.write(f"\rProgress: {percent:.1f}%")
        sys.stdout.flush()

    # Build output filename
    output_filename = Path(output_folder) / (file_path.stem + "_" + lang + ".txt")

    # Save the transcription
    with open(output_filename, "w", encoding="utf-8") as f:
        f.write(transcript.strip())

    print(f"\nSaved to: {output_filename}\n")

    # Move the original audio file to the 'processed' folder
    #filepath = os.path.join("processed", str(file_path.name))
    #print(f"Moving: {enhanced_audio_fixed}... to {filepath}")
    #shutil.move(enhanced_audio_fixed, filepath)

print("\n✅ All files transcribed!")
