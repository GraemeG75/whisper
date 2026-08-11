# Real-Time MP3 Streaming Transcription - Quick Start

## Installation

### 1. Install Python Dependencies

For streaming support, run:

```bash
pip install requests pydub
```

Or install all at once:

```bash
pip install -r requirements-streaming.txt
```

### 2. Install FFmpeg

FFmpeg is required for audio processing. Download from https://ffmpeg.org/download.html or use a package manager:

**Windows (if using Chocolatey):**
```bash
choco install ffmpeg
```

**Mac:**
```bash
brew install ffmpeg
```

**Linux:**
```bash
sudo apt-get install ffmpeg
```

## Quick Commands

### Stream from URL (Live Radio or Hosted MP3)

```bash
python transcribe_whisper.py --stream-url "http://example.com/stream.mp3" --language en --model base
```

### Stream from URL with Authentication

```bash
python transcribe_whisper.py --stream-url "http://example.com/stream.mp3" --language en --model base --stream-auth-cookies cookies.json
```

### Stream from stdin (Pipe from ffmpeg)

```bash
ffmpeg -i video.mp4 -f mp3 - | python transcribe_whisper.py --stream-stdin --language en --model base
```

### Save stream output to file

```bash
python transcribe_whisper.py --stream-url "..." --language en --model base --stream-output transcripts/output.txt
```

### Traditional batch processing (unchanged)

```bash
python transcribe_whisper.py --input-folder audio_files --output-folder transcripts --language en --model base
```

## Common Examples

### Example 1: Transcribe a live radio stream

```bash
python transcribe_whisper.py \
  --stream-url "http://radio.example.com/live.mp3" \
  --language en \
  --model base \
  --stream-output transcripts/radio.txt
```

### Example 2: Convert MP4 video to text in real-time

```bash
ffmpeg -i my_video.mp4 -f mp3 - | \
  python transcribe_whisper.py \
    --stream-stdin \
    --language en \
    --model base \
    --stream-output transcripts/video.txt
```

### Example 3: Stream protected audio with authentication

```bash
python transcribe_whisper.py \
  --stream-url "http://protected.example.com/stream.mp3" \
  --language en \
  --model small \
  --stream-auth-cookies cookies.json \
  --stream-output transcripts/protected.txt
```

### Example 4: Stream with custom dispatch prompt

```bash
python transcribe_whisper.py \
  --stream-url "http://dispatch.example.com/feed.mp3" \
  --language en \
  --model small \
  --prompt "Police dispatch, emergency radio" \
  --stream-chunk-ms 8000 \
  --stream-output transcripts/dispatch.txt
```

### Example 5: High-quality transcription (slower, but more accurate)

```bash
python transcribe_whisper.py \
  --stream-url "..." \
  --language en \
  --model large \
  --stream-chunk-ms 15000
```

### Example 6: Fast transcription (lower latency)

```bash
python transcribe_whisper.py \
  --stream-url "..." \
  --language en \
  --model base \
  --stream-chunk-ms 5000
```

## Key Options

| Option | Description | Example |
|--------|-------------|---------|
| `--stream-url URL` | Stream from URL | `--stream-url "http://...mp3"` |
| `--stream-stdin` | Read from stdin | `--stream-stdin` |
| `--stream-output FILE` | Save to file | `--stream-output transcripts/out.txt` |
| `--stream-auth-cookies FILE` | Cookie file for authentication (JSON or Netscape format) | `--stream-auth-cookies cookies.json` |
| `--stream-chunk-ms MS` | Buffer size (lower = faster, higher = better quality) | `--stream-chunk-ms 10000` |
| `--language` | Language code | `--language en` (English) |
| `--model` | Model size | `base`, `small`, `medium`, `large` |
| `--prompt` | Custom initial prompt | `--prompt "dispatch audio"` |

## Model Comparison

| Model | Speed | Accuracy | VRAM |
|-------|-------|----------|------|
| `base` | ⚡⚡⚡ Fast | Good | ~1GB |
| `small` | ⚡⚡ Medium | Very Good | ~2GB |
| `medium` | ⚡ Slow | Excellent | ~5GB |
| `large` | 🐢 Very Slow | Best | ~10GB |

For streaming, use `base` or `small` for real-time performance.

## Output Format

Each segment includes a timestamp:

```
[00:00:05.123] This is the first transcribed segment
[00:00:12.456] And here's the next one
[00:00:23.789] Real-time transcription works!
```

## Troubleshooting

### "No module named 'requests'" or "'pydub'"
Install: `pip install requests pydub`

### Connection timeout on stream
Use ffmpeg to handle it: `ffmpeg -i "http://url" -f mp3 - | python transcribe_whisper.py --stream-stdin ...`

### Slow transcription
Use a smaller model: `--model base` or `--model small`

### Too much latency
Reduce buffer size: `--stream-chunk-ms 5000`

### Audio quality issues
Increase buffer size: `--stream-chunk-ms 15000` or use larger model: `--model large`

## Next Steps

- See [STREAMING_USAGE.md](STREAMING_USAGE.md) for more detailed documentation
- Experiment with `--stream-chunk-ms` to find the right balance for your use case
- Use `--prompt` to improve accuracy for specialized domains
