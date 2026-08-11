namespace WhisperWinForms.Services
{
    /// <summary>Wraps raw PCM16 mono samples in a minimal RIFF/WAVE header for Whisper.net.</summary>
    public static class WavUtils
    {
        public static MemoryStream CreatePcm16WavStream(byte[] pcmData, int sampleRate = 16000, int channels = 1)
        {
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true))
            {
                int byteRate = sampleRate * channels * 2;
                int blockAlign = channels * 2;

                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + pcmData.Length);
                writer.Write("WAVE".ToCharArray());

                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)blockAlign);
                writer.Write((short)16);

                writer.Write("data".ToCharArray());
                writer.Write(pcmData.Length);
                writer.Write(pcmData);
            }

            stream.Position = 0;
            return stream;
        }
    }
}
