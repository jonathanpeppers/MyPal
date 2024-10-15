using System.Collections.Concurrent;

namespace MyPal.ClassLibrary;

public class QueuedStream : Stream
{
    ConcurrentQueue<byte[]> _buffers = new();

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Flush() => throw new NotImplementedException();

    /// <summary>
    /// Main entry point for an IMicrophone to pass byte arrays to
    /// </summary>
    public void Enqueue(byte[] buffer) => _buffers.Enqueue(buffer);

    public override int Read(byte[] buffer, int offset, int count)
    {
        byte[]? bytes;
        while (!_buffers.TryDequeue(out bytes))
        {
            Thread.Sleep(100);
        }

        int length = Math.Min(bytes.Length, count);
        Array.Copy(bytes, 0, buffer, offset, length);
        return length;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
}