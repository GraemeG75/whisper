# Real-Time MP3 Streaming Transcription

Your Whisper transcriber now supports real-time or near real-time MP3 streaming transcription.

## Prerequisites

For streaming support, install the required packages:

```bash
pip install requests pydub
```

## Usage Modes

### 1. Stream from URL

Transcribe an MP3 stream from a URL (e.g., internet radio, live stream):

```bash
python transcribe_whisper.py --stream-url "http://example.com/stream.mp3" --language en --model base
```

**Options:**
- `--stream-url URL` - URL to the MP3 stream
- `--stream-chunk-ms MILLISECONDS` - Buffer size before transcription (default: 10000ms)
- `--stream-output FILE` - Save results to file (default: stdout)
- `--stream-auth-cookies FILE` - Path to cookie file for authentication (optional)

**Example with output file:**
```bash
python transcribe_whisper.py \
  --stream-url "http://example.com/radio.mp3" \
  --language en \
  --model base \
  --stream-output transcripts/live_stream.txt
```

**Example with authentication:**
```bash
python transcribe_whisper.py \
  --stream-url "http://example.com/protected.mp3" \
  --language en \
  --model base \
  --stream-auth-cookies cookies.json
```

### Stream from an M3U8/HLS playlist

M3U and M3U8 playlist URLs are resolved through FFmpeg before audio is sent
to Whisper. Authentication cookies are applied to the playlist and segment
requests:

```bash
python transcribe_whisper.py \
  --stream-url "https://example.com/live/playlist.m3u8" \
  --language en \
  --model base \
  --stream-auth-cookies cookies.txt \
  --stream-output transcripts/hls.txt
```

### 2. Stream from stdin

Pipe MP3 data from another source:

```bash
ffmpeg -i input.mp3 -f mp3 - | python transcribe_whisper.py --stream-stdin --language en --model base
```

Or with a network source:
```bash
curl http://example.com/stream.mp3 | python transcribe_whisper.py --stream-stdin --language en --model base
```

**Options:**
- `--stream-stdin` - Enable stdin streaming mode
- `--stream-chunk-ms MILLISECONDS` - Buffer size (default: 10000ms)
- `--stream-output FILE` - Save results to file (default: stdout)

## Configuration

### Buffer Size (--stream-chunk-ms)

Controls how much audio to accumulate before transcribing:
- **Smaller (5000ms)**: Faster responses, but more frequent processing
- **Larger (20000ms)**: Better transcription quality, higher latency
- **Recommended**: 10000ms (10 seconds) for balanced performance

Example:
```bash
python transcribe_whisper.py --stream-url "..." --stream-chunk-ms 5000 --language en
```

### Output

**Real-time to console:**
```bash
python transcribe_whisper.py --stream-url "..." --language en
```

**Save to file:**
```bash
python transcribe_whisper.py \
  --stream-url "..." \
  --language en \
  --stream-output transcripts/stream_output.txt
```

## Authentication with Cookies

If the streaming URL requires authentication, you can provide cookies using the `--stream-auth-cookies` flag.

### Supported Cookie Formats

#### 1. JSON Format (Recommended)

Create a `cookies.json` file:

```json
{
    "session_id": "your_session_value",
    "auth_token": "your_auth_token",
    "custom_cookie": "custom_value"
}
```

Then use it:

```bash
python transcribe_whisper.py \
  --stream-url "http://example.com/protected.mp3" \
  --language en \
  --model base \
  --stream-auth-cookies cookies.json
```

#### 2. Netscape Cookie Jar Format

Export cookies from your browser in Netscape format (common browser export):

```
# Netscape HTTP Cookie File
# domain flag path secure expiration name value
.example.com	TRUE	/	TRUE	1735689600	session_id	abc123def456
.example.com	TRUE	/	TRUE	1735689600	auth_token	xyz789
```

Use it the same way:

```bash
python transcribe_whisper.py \
  --stream-url "http://example.com/protected.mp3" \
  --language en \
  --model base \
  --stream-auth-cookies cookies.txt
```

### How to Export Cookies from Your Browser

**Chrome/Edge:**
1. Go to the website you need cookies from
2. Press F12 to open Developer Tools
3. Go to Application → Cookies → Select the domain
4. Right-click and copy cookies, then manually create a JSON file

**Firefox:**
1. Install "Export Cookies" extension
2. Click the extension icon
3. Export as JSON format

**Manual JSON Creation:**
1. Go to the website in your browser
2. Open DevTools (F12) → Application → Cookies
3. Identify the cookies you need
4. Create a JSON file with the cookie names and values

### Example: Authenticated Radio Stream

```bash
# 1. Export cookies from browser to cookies.json
# 2. Run:
python transcribe_whisper.py \
  --stream-url "https://streaming.example.com/protected-radio.mp3" \
  --language en \
  --model small \
  --stream-chunk-ms 8000 \
  --stream-auth-cookies cookies.json \
  --stream-output transcripts/radio_protected.txt
```

### Troubleshooting Authentication Issues

**401 Unauthorized:**
- Check that your cookies are valid and not expired
- Try manually accessing the URL in a browser to verify it works
- Verify cookie names and values match exactly

**403 Forbidden:**
- The cookies may not have sufficient permissions
- You might need additional cookies (e.g., CSRF tokens)
- Check the browser's cookie requirements

**Connection works but authentication fails:**
- Some sites require additional headers or user-agent strings
- The cookie jar automatically includes common headers
- Try the URL directly in your browser to verify it works



1. **Use faster models for real-time**: `--model base` or `--model small` for lower latency
2. **Disable preprocessing**: Streaming already does basic filtering
3. **Adjust buffer size**: Experiment with `--stream-chunk-ms` based on your network and hardware
4. **Monitor resources**: Use GPU (`--model large` works best on CUDA)

## Output Format

Each transcribed segment includes a timestamp:

```
[00:00:05.123] This is the first transcribed segment
[00:00:12.456] And here's the next one
[00:00:23.789] Continuing in real-time...
```

## Examples

### Live Radio Stream
```bash
python transcribe_whisper.py \
  --stream-url "http://stream.example.com/radio" \
  --language en \
  --model small \
  --stream-chunk-ms 8000 \
  --stream-output transcripts/radio_live.txt
```

### HTTP Progressive Download
```bash
python transcribe_whisper.py \
  --stream-url "http://example.com/large-file.mp3" \
  --language en \
  --model base \
  --stream-chunk-ms 15000
```

### Pipe from ffmpeg
```bash
ffmpeg -i video.mp4 -f mp3 - | \
  python transcribe_whisper.py \
    --stream-stdin \
    --language en \
    --model base
```

### Combine with Other Tools

Stream transcription with real-time output formatting:
```bash
python transcribe_whisper.py \
  --stream-url "..." \
  --language en \
  --model base | \
  tee transcripts/stream.txt | \
  while read line; do echo "[$(date +'%H:%M:%S')] $line"; done
```

## Troubleshooting

### ImportError: No module named 'requests' or 'pydub'
Install missing packages:
```bash
pip install requests pydub
```

### Connection Timeout
Increase timeout or use `ffmpeg` to handle network streaming:
```bash
ffmpeg -i "http://stream.url" -f mp3 - | python transcribe_whisper.py --stream-stdin --language en
```

### Audio Quality Issues
- Lower the buffer size for more frequent processing
- Use a larger model: `--model medium` or `--model large`
- Check your network bandwidth

### High Latency
- Reduce buffer size: `--stream-chunk-ms 5000`
- Use a faster model: `--model small` or `--model base`
- Disable hallucination filtering if needed

## Batch Mode (Unchanged)

Original batch processing still works:

```bash
# Process files from audio_files folder
python transcribe_whisper.py --language en --model base

# With specific folder
python transcribe_whisper.py --input-folder my_audio --output-folder my_transcripts
```

## Combining Modes

You can use streaming arguments with some batch options:

```bash
python transcribe_whisper.py \
  --stream-url "..." \
  --language en \
  --model base \
  --prompt "Public safety dispatch audio" \
  --stream-chunk-ms 10000 \
  --stream-output transcripts/dispatch.txt
```
