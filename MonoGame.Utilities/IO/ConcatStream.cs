using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoGame.Framework.Collections;

namespace MonoGame.Framework.IO
{
    /// <summary>
    /// Concatenates multiple streams into one forward-only stream.
    /// </summary>
    public class ConcatStream : Stream
    {
        private Queue<Part> _parts;
        private long _position;

        #region Properties

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        /// <summary>
        /// Gets the total length of the stream. Returns -1 if any inner stream is not seekable.
        /// </summary>
        public override long Length
        {
            get
            {
                long total = 0;
                foreach (var slice in _parts)
                {
                    if (!slice.Stream.CanSeek)
                        return -1;
                    total += slice.Stream.Length;
                }
                return total;
            }
        }

        /// <summary>
        /// Gets whether any inner stream can time out.
        /// </summary>
        public override bool CanTimeout
        {
            get
            {
                foreach (var slice in _parts)
                    if (slice.Stream.CanTimeout)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Gets the read position within the stream.
        /// </summary>
        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the <see cref="ConcatStream"/> with two streams, 
        /// optionally leaving them open after disposal.
        /// </summary>
        public ConcatStream(Stream first, bool leaveFirstOpen, Stream second, bool leaveSecondOpen)
        {
            var firstPart = new Part(first, leaveFirstOpen);
            var secondPart = new Part(second, leaveSecondOpen);

            _parts = new Queue<Part>(2);
            _parts.Enqueue(firstPart);
            _parts.Enqueue(secondPart);
        }

        /// <summary>
        /// Constructs the <see cref="ConcatStream"/> with two streams, not leaving them open after disposal.
        /// </summary>
        public ConcatStream(Stream first, Stream second) : this(first, false, second, false)
        {
        }

        /// <summary>
        /// Constructs the <see cref="ConcatStream"/> from an enumerable of streams,
        /// optionally leaving them open after disposal.
        /// </summary>
        public ConcatStream(IEnumerable<Stream> streams, bool leaveOpen)
        {
            if (streams == null)
                throw new ArgumentNullException(nameof(streams));

            _parts = new Queue<Part>(streams.TryGetNonEnumeratedCount(out int count) ? count : 4);

            foreach (var stream in streams)
                _parts.Enqueue(new Part(stream, leaveOpen));
        }

        /// <summary>
        /// Constructs the <see cref="ConcatStream"/> from an enumerable of streams.
        /// Use this to individually control which streams are left open after disposal.
        /// </summary>
        public ConcatStream(IEnumerable<Part> parts)
        {
            _parts = new Queue<Part>(parts);
        }

        #endregion

        public override int Read(Span<byte> buffer)
        {
            TryRead:
            if (_parts.Count > 0)
            {
                int read = _parts.Peek().Stream.Read(buffer);
                if (read == 0)
                {
                    DequeueAndDisposePart();
                    goto TryRead;
                }
                else
                {
                    _position += read;
                    return read;
                }
            }
            return 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Flush() => throw new NotSupportedException();

        private void DequeueAndDisposePart()
        {
            var part = _parts.Dequeue();
            if (!part.LeaveOpen)
                part.Stream.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    while (_parts.Count > 0)
                        DequeueAndDisposePart();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Represents a <see cref="System.IO.Stream"/> that is optionally disposed after use.
        /// </summary>
        public readonly struct Part
        {
            public Stream Stream { get; }
            public bool LeaveOpen { get; }

            public Part(Stream stream, bool leaveOpen)
            {
                Stream = stream ?? throw new ArgumentNullException(nameof(stream));
                LeaveOpen = leaveOpen;
            }
        }
    }
}