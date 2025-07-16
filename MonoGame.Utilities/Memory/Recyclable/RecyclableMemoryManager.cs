// ---------------------------------------------------------------------
// Copyright (c) 2015-2016 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// ---------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MonoGame.Framework.Memory
{
    /// <summary>
    /// Manages pools of small block arrays and large buffer arrays.
    /// </summary>
    /// <remarks>
    /// There are two pools managed in here. The small block pool contains same-sized
    /// buffers that are handed to streams as they write more data.
    /// 
    /// For scenarios that need to call <see cref="GetBuffer"/>, 
    /// the large buffer pool contains buffers of various sizes,
    /// all multiples/exponentials of <see cref="BufferMultiple"/> (<see cref="DefaultBufferMultiple"/> MB by default). 
    /// They are split by size to avoid overly-wasteful buffer usage. 
    /// There should be far fewer 8 MB buffers than 1 MB buffers, for example.
    /// </remarks>
    public partial class RecyclableMemoryManager
    {
        public enum MemoryDiscardReason
        {
            TooLarge,
            EnoughFree
        }

        #region Delegate Definitions

        /// <summary>
        /// Delegate for handling buffer discard reports.
        /// </summary>
        /// <param name="tag">The tag associated with the event.</param>
        /// <param name="reason">Reason the buffer was discarded.</param>
        public delegate void BufferDiscardedEventHandler(string? tag, MemoryDiscardReason reason);

        /// <summary>
        /// Delegate for handling reports of stream size when streams are allocated
        /// </summary>
        /// <param name="bytes">Bytes allocated.</param>
        public delegate void StreamLengthReportHandler(long bytes);

        /// <summary>
        /// Delegate for handling actions that will return a callstack 
        /// whenever <see cref="GenerateCallStacks"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="tag">The tag associated with the event.</param>
        /// <param name="callStack">The call stack of the allocation.</param>
        public delegate void ImportantActionHandler(string? tag, string? callStack);

        /// <summary>
        /// Delegate for handling actions that have an attached tag.
        /// </summary>
        /// <param name="tag">The tag associated with the event.</param>
        public delegate void TaggedActionHandler(string? tag);

        /// <summary>
        /// Delegate for handling periodic reporting of memory use statistics.
        /// </summary>
        /// <param name="blockPoolInUseBytes">Bytes currently in use in the block pool.</param>
        /// <param name="blockPoolFreeBytes">Bytes currently free in the block pool.</param>
        /// <param name="bufferPoolInUseBytes">Bytes currently in use in the buffer pool.</param>
        /// <param name="bufferPoolFreeBytes">Bytes currently free in the buffer pool.</param>
        public delegate void UsageReportEventHandler(
            long blockPoolInUseBytes, long blockPoolFreeBytes, long bufferPoolInUseBytes, long bufferPoolFreeBytes);

        #endregion

        public const int DefaultBlockSize = 64 * 1024;
        public const int DefaultBufferMultiple = 1024;
        public const int DefaultMaximumBufferSize = 16 * 1024 * 1024;
        public const bool DefaultUseExponentialBuffer = true;

        private static object DefaultInitMutex { get; } = new object();
        private static RecyclableMemoryManager? _default;

        public static RecyclableMemoryManager Default
        {
            get
            {
                if (_default == null)
                {
                    lock (DefaultInitMutex)
                    {
                        if (_default != null)
                            return _default;
                        _default = new RecyclableMemoryManager();
                    }
                }
                return _default;
            }
        }

        #region Instance Fields

        private readonly long[] _bufferFreeSize;
        private readonly long[] _bufferInUseSize;

        /// <summary>
        /// pools[0] = 1x BufferMultiple buffers
        /// pools[1] = 2x BufferMultiple buffers
        /// pools[2] = 3x(multiple)/4x(exponential) BufferMultiple buffers
        /// pools[3] = 4x(multiple)/8x(exponential) BufferMultiple buffers
        /// etc., up to maximumBufferSize
        /// </summary>
        private readonly ConcurrentStack<byte[]>[] _bufferPools;
        private readonly ConcurrentStack<byte[]> _blockPool;

        private long _blockPoolFreeSize;
        private long _blockPoolInUseSize;

        #endregion

        #region Public Properties

        /// <summary>
        /// The size of each block. It must be set at creation and cannot be changed.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// All buffers are multiples/exponentials of this number. It must be set at creation and cannot be changed.
        /// </summary>
        public int BufferMultiple { get; }

        /// <summary>
        /// Use exponential allocation strategy for buffers. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseExponentialBuffer { get; }

        /// <summary>
        /// Use multiple allocation strategy for buffers. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseMultipleBuffer => !UseExponentialBuffer;

        /// <summary>
        /// Gets the maximum buffer size of buffers.
        /// </summary>
        /// <remarks>
        /// Any buffer that is returned to the pool that is larger than this will be ignored.
        /// </remarks>
        public int MaximumBufferSize { get; }

        /// <summary>
        /// Number of bytes in block pool not currently in use
        /// </summary>
        public long BlockPoolFreeSize => _blockPoolFreeSize;

        /// <summary>
        /// Number of bytes currently in use by stream from the block pool.
        /// </summary>
        public long BlockPoolInUseSize => _blockPoolInUseSize;

        /// <summary>
        /// Number of bytes in buffer pool not currently in use.
        /// </summary>
        public long BufferPoolFreeSize
        {
            get
            {
                long sum = 0;
                foreach (var item in _bufferFreeSize)
                    sum += item;
                return sum;
            }
        }

        /// <summary>
        /// Number of bytes currently in use by streams from the buffer pool.
        /// </summary>
        public long BufferPoolInUseSize
        {
            get
            {
                long sum = 0;
                foreach (var item in _bufferInUseSize)
                    sum += item;
                return sum;
            }
        }

        /// <summary>
        /// How many blocks are in the block pool.
        /// </summary>
        public long BlocksFree => _blockPool.Count;

        /// <summary>
        /// How many buffers are in the buffer pool.
        /// </summary>
        public long BuffersFree
        {
            get
            {
                long free = 0;
                foreach (var pool in _bufferPools)
                    free += pool.Count;
                return free;
            }
        }

        /// <summary>
        /// How many bytes of free blocks to allow before we start dropping those returned to us.
        /// </summary>
        public long MaximumFreeBlockPoolBytes { get; set; }

        /// <summary>
        /// How many bytes of free buffers to allow before we start dropping those returned to us.
        /// </summary>
        public long MaximumFreeBufferPoolBytes { get; set; }

        /// <summary>
        /// Maximum stream capacity in bytes. Attempts to set a larger capacity will result in an exception.
        /// </summary>
        /// <remarks>A value of 0 indicates no limit.</remarks>
        public long MaximumStreamCapacity { get; set; }

        /// <summary>
        /// Whether to save callstacks for stream allocations. This can help in debugging.
        /// It should NEVER be turned on generally in production.
        /// </summary>
        public bool GenerateCallStacks { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool when <see cref="GetBuffer"/> is called on
        /// a stream and creates a single large buffer, if this setting is enabled, the other blocks will be
        /// returned to the buffer pool immediately.
        /// </summary>
        public bool AggresiveBlockReturn { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool. E.g. when <see cref="GetBuffer"/> is called on
        /// a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
        /// to the buffer pool immediately.
        /// Note when enabling this setting that the user is responsible for ensuring that any buffer previously
        /// retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
        /// be valid).
        /// </summary>
        public bool AggressiveBufferReturn { get; set; }

        #endregion

        #region Public Events

        /// <summary>
        /// Triggered when a new block is created.
        /// </summary>
        public event Action? BlockCreated;

        /// <summary>
        /// Triggered when a new block is created.
        /// </summary>
        public event TaggedActionHandler? BlockDiscarded;

        /// <summary>
        /// Triggered when a new buffer is created.
        /// </summary>
        public event ImportantActionHandler? BufferCreated;

        /// <summary>
        /// Triggered when a new stream is created.
        /// </summary>
        public event Action? StreamCreated;

        /// <summary>
        /// Triggered when a stream is disposed.
        /// </summary>
        public event Action? StreamDisposed;

        /// <summary>
        /// Triggered when a stream is finalized.
        /// </summary>
        public event Action? StreamFinalized;

        /// <summary>
        /// Triggered when a stream is converted to an array.
        /// </summary>
        public event ImportantActionHandler? StreamConvertedToArray;

        /// <summary>
        /// Triggered when a stream is finalized.
        /// </summary>
        public event StreamLengthReportHandler? StreamLength;

        /// <summary>
        /// Triggered when a buffer is discarded, along with the reason for the discard.
        /// </summary>
        public event BufferDiscardedEventHandler? BufferDiscarded;

        /// <summary>
        /// Periodically triggered to report usage statistics.
        /// </summary>
        public event UsageReportEventHandler? UsageReport;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications.
        /// </summary>
        public RecyclableMemoryManager() : this(
            DefaultBlockSize, DefaultBufferMultiple, DefaultMaximumBufferSize, DefaultUseExponentialBuffer)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block size.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="bufferMultiple">Each buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// blockSize is not a positive number, or bufferMultiple is not a positive number,
        /// or maximumBufferSize is less than blockSize.
        /// </exception>
        /// <exception cref="ArgumentException">maximumBufferSize is not a multiple of bufferMultiple</exception>
        public RecyclableMemoryManager(int blockSize, int bufferMultiple, int maximumBufferSize)
            : this(blockSize, bufferMultiple, maximumBufferSize, DefaultUseExponentialBuffer)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block size.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="bufferMultiple">Each buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="useExponentialBuffer">Switch to exponential buffer allocation strategy</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// blockSize is not a positive number, or bufferMultiple is not a positive number,
        /// or maximumBufferSize is less than blockSize.
        /// </exception>
        /// <exception cref="ArgumentException">maximumBufferSize is not a multiple/exponential of bufferMultiple</exception>
        public RecyclableMemoryManager(int blockSize, int bufferMultiple, int maximumBufferSize, bool useExponentialBuffer)
        {
            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(blockSize), blockSize, "Must be a positive number");

            if (bufferMultiple <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(bufferMultiple), bufferMultiple, "Must be a positive number");

            if (maximumBufferSize < blockSize)
                throw new ArgumentOutOfRangeException(
                    nameof(maximumBufferSize), maximumBufferSize, "Must be at least blockSize.");

            BlockSize = blockSize;
            BufferMultiple = bufferMultiple;
            MaximumBufferSize = maximumBufferSize;
            UseExponentialBuffer = useExponentialBuffer;
            AggresiveBlockReturn = true;

            if (!IsBufferSize(maximumBufferSize))
                throw new ArgumentException(
                    string.Format("maximumBufferSize is not {0} of bufferMultiple",
                    UseExponentialBuffer ? "an exponential" : "a multiple"), 
                    nameof(maximumBufferSize));

            _blockPool = new ConcurrentStack<byte[]>();
            int bufferPoolCount = useExponentialBuffer
                ? ((int)Math.Log(maximumBufferSize / bufferMultiple, 2) + 1)
                : (maximumBufferSize / bufferMultiple);

            // +1 to store size of bytes in use that are too large to be pooled
            _bufferInUseSize = new long[bufferPoolCount + 1];
            _bufferFreeSize = new long[bufferPoolCount];

            _bufferPools = new ConcurrentStack<byte[]>[bufferPoolCount];
            for (int i = 0; i < _bufferPools.Length; ++i)
                _bufferPools[i] = new ConcurrentStack<byte[]>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Removes and returns a single block from the pool.
        /// </summary>
        /// <returns>A byte[] array</returns>
        public byte[] GetBlock()
        {
            if (!_blockPool.TryPop(out var block))
            {
                // We'll add this back to the pool when the stream is disposed
                // (unless our free pool is too large)
                block = new byte[BlockSize];
                ReportBlockCreated();
            }
            else
            {
                Interlocked.Add(ref _blockPoolFreeSize, -BlockSize);
            }
            Interlocked.Add(ref _blockPoolInUseSize, BlockSize);
            return block;
        }

        /// <summary>
        /// Returns a buffer of arbitrary size from the buffer pool. This buffer
        /// will be at least the minimumSize and always be a multiple/exponential of <see cref="BufferMultiple"/>.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the buffer</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <returns>A buffer of at least the minimum size.</returns>
        public RecyclableBuffer GetBuffer(long minimumLength, string? tag = null)
        {
            int roundedLength = RoundToBufferSize(minimumLength);
            int poolIndex = GetPoolIndex(roundedLength);

            byte[]? buffer;
            if (poolIndex < _bufferPools.Length)
            {
                if (!_bufferPools[poolIndex].TryPop(out buffer))
                {
                    buffer = new byte[roundedLength];
                    ReportBufferCreated(tag);
                }
                else
                    Interlocked.Add(ref _bufferFreeSize[poolIndex], -buffer.Length);
            }
            else
            {
                // Buffer is too large to pool. They get a new buffer.

                // We still want to track the size, though, and we've reserved a slot
                // in the end of the inuse array for nonpooled bytes in use.
                poolIndex = _bufferInUseSize.Length - 1;

                // We still want to round up to reduce heap fragmentation.
                buffer = new byte[roundedLength];
                ReportBufferCreated(tag);
            }

            Interlocked.Add(ref _bufferInUseSize[poolIndex], buffer.Length);
            return new RecyclableBuffer(this, buffer, (int)minimumLength, tag);
        }

        /// <summary>
        /// Returns the buffer to the large buffer pool.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException">buffer is null</exception>
        /// <exception cref="ArgumentException">
        /// buffer.Length is not a multiple/exponential of <see cref="BufferMultiple"/> 
        /// (it did not originate from this pool).
        /// </exception>
        public void ReturnBuffer(byte[] buffer, string? tag = null)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (buffer.Length > MaximumBufferSize)
                return;

            if (!IsBufferSize(buffer.Length))
                throw new ArgumentException(string.Format("The size is not {0} of {1}.",
                    UseExponentialBuffer ? "an exponential" : "a multiple", BufferMultiple));

            int poolIndex = GetPoolIndex(buffer.Length);
            if (poolIndex < _bufferPools.Length)
            {
                if ((_bufferPools[poolIndex].Count + 1) * buffer.Length <= MaximumFreeBufferPoolBytes ||
                    MaximumFreeBufferPoolBytes == 0)
                {
                    _bufferPools[poolIndex].Push(buffer);
                    Interlocked.Add(ref _bufferFreeSize[poolIndex], buffer.Length);
                }
                ReportBufferDiscarded(tag, MemoryDiscardReason.EnoughFree);
            }
            else
            {
                // This is a non-poolable buffer, but we still want to track its size for inuse
                // analysis. We have space in the inuse array for this.
                poolIndex = _bufferInUseSize.Length - 1;
                ReportBufferDiscarded(tag, MemoryDiscardReason.TooLarge);
            }

            Interlocked.Add(ref _bufferInUseSize[poolIndex], -buffer.Length);
            ReportUsageReport(_blockPoolInUseSize, _blockPoolFreeSize, BufferPoolInUseSize, BufferPoolFreeSize);
        }

        /// <summary>
        /// Returns a collection of blocks to the pool.
        /// </summary>
        /// <param name="blocks">Collection of blocks to return to the pool.</param>
        /// <param name="tag">The tag of the object returning these blocks, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException">blocks is null.</exception>
        /// <exception cref="ArgumentException">blocks contains buffers that are the wrong size (or null) for this memory manager.</exception>
        public void ReturnBlocks(ICollection<byte[]> blocks, string? tag = null)
        {
            if (blocks == null)
                throw new ArgumentNullException(nameof(blocks));

            void CheckBlock(byte[] block)
            {
                if (block == null || block.Length != BlockSize)
                    throw new ArgumentException("blocks contains buffers that are not BlockSize in length");
            }

            bool PushBlock(byte[] block)
            {
                if (MaximumFreeBlockPoolBytes == 0 || BlockPoolFreeSize < MaximumFreeBlockPoolBytes)
                {
                    Interlocked.Add(ref _blockPoolFreeSize, BlockSize);
                    _blockPool.Push(block);
                    return true;
                }
                ReportBlockDiscarded(tag);
                return false;
            }

            int bytesToReturn = blocks.Count * BlockSize;
            Interlocked.Add(ref _blockPoolInUseSize, -bytesToReturn);

            // small thing to reduce 2 IEnumerable+IEnumerator allocations in most cases
            if (blocks is List<byte[]> blockList)
            {
                foreach (byte[] block in blockList)
                    CheckBlock(block);

                foreach (byte[] block in blockList)
                    if (!PushBlock(block))
                        break;
            }
            else
            {
                foreach (byte[] block in blocks)
                    CheckBlock(block);

                foreach (byte[] block in blocks)
                    if (!PushBlock(block))
                        break;
            }

            ReportUsageReport(_blockPoolInUseSize, _blockPoolFreeSize, BufferPoolInUseSize, BufferPoolFreeSize);
        }

        /// <summary>
        /// Returns a block to the pool.
        /// </summary>
        /// <param name="block">The block to return to the pool.</param>
        /// <exception cref="ArgumentException">block is the wrong size for this memory manager.</exception>
        public void ReturnBlock(byte[]? block, string? tag = null)
        {
            if (block == null)
                return;

            if (block.Length != BlockSize)
                throw new ArgumentException("The block is the wrong size for this memory manager.");

            Interlocked.Add(ref _blockPoolInUseSize, -BlockSize);

            if (MaximumFreeBlockPoolBytes == 0 || BlockPoolFreeSize < MaximumFreeBlockPoolBytes)
            {
                Interlocked.Add(ref _blockPoolFreeSize, BlockSize);
                _blockPool.Push(block);
            }
            else
            {
                ReportBlockDiscarded(tag);
            }

            if (UsageReport != null)
                ReportUsageReport(_blockPoolInUseSize, _blockPoolFreeSize, BufferPoolInUseSize, BufferPoolFreeSize);
        }

        #endregion

        #region Helpers

        [DebuggerHidden]
        private string? GetCallStack()
        {
            if (GenerateCallStacks)
                return Environment.StackTrace;
            return null;
        }

        private int RoundToBufferSize(long minimumSize)
        {
            if (UseExponentialBuffer)
            {
                int pow = 1;
                while (BufferMultiple * pow < minimumSize)
                    pow <<= 1;
                return BufferMultiple * pow;
            }
            else
            {
                return (int)((minimumSize + BufferMultiple - 1) / BufferMultiple * BufferMultiple);
            }
        }

        private bool IsBufferSize(int value)
        {
            return (value != 0) && (UseExponentialBuffer
                ? (value == RoundToBufferSize(value))
                : (value % BufferMultiple) == 0);
        }

        private int GetPoolIndex(long length)
        {
            if (UseExponentialBuffer)
            {
                int index = 0;
                while ((BufferMultiple << index) < length)
                    index++;
                return index;
            }
            else
            {
                return (int)(length / BufferMultiple - 1);
            }
        }

        #endregion

        #region Event Helpers

        internal void ReportBlockCreated() => BlockCreated?.Invoke();
        internal void ReportStreamCreated() => StreamCreated?.Invoke();
        internal void ReportStreamDisposed() => StreamDisposed?.Invoke();
        internal void ReportStreamFinalized() => StreamFinalized?.Invoke();

        internal void ReportBlockDiscarded(string? tag) => BlockDiscarded?.Invoke(tag);

        // also grab the stack, we want to know who requires such large buffers
        internal void ReportBufferCreated(string? tag) => BufferCreated?.Invoke(tag, GetCallStack());
        internal void ReportStreamToArray(string? tag) => StreamConvertedToArray?.Invoke(tag, GetCallStack());

        internal void ReportStreamLength(long bytes) => StreamLength?.Invoke(bytes);

        internal void ReportBufferDiscarded(string? tag, MemoryDiscardReason reason)
        {
            BufferDiscarded?.Invoke(tag, reason);
        }

        internal void ReportUsageReport(
            long blockPoolInUseBytes, long blockPoolFreeBytes,
            long bufferPoolInUseBytes, long bufferPoolFreeBytes)
        {
            UsageReport?.Invoke(
                blockPoolInUseBytes, blockPoolFreeBytes, bufferPoolInUseBytes, bufferPoolFreeBytes);
        }

        #endregion

        #region Stream Construction Helpers

        /// <summary>
        /// Constructs a new <see cref="RecyclableBufferedStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"></param>
        /// <returns>The <see cref="RecyclableBufferedStream"/>.</returns>
        public RecyclableBufferedStream GetBufferedStream(Stream stream, bool leaveOpen)
        {
            return new RecyclableBufferedStream(this, stream, leaveOpen);
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with no tag
        /// tag and a default initial capacity.
        /// </summary>
        /// <returns>The <see cref="RecyclableMemoryStream"/>.</returns>
        public RecyclableMemoryStream GetMemoryStream()
        {
            return new RecyclableMemoryStream(this);
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with the given 
        /// tag and a default initial capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>The <see cref="RecyclableMemoryStream"/>.</returns>
        public RecyclableMemoryStream GetMemoryStream(string? tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with at least the given capacity.
        /// </summary>
        /// <param name="minimumSize">The minimum desired capacity for the stream.</param>
        /// <returns>The <see cref="RecyclableMemoryStream"/>.</returns>
        public RecyclableMemoryStream GetMemoryStream(long minimumSize)
        {
            return new RecyclableMemoryStream(this, null, minimumSize);
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with the given 
        /// tag and at least the given capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="minimumSize">The minimum desired capacity for the stream.</param>
        /// <returns>The <see cref="RecyclableMemoryStream"/>.</returns>
        public RecyclableMemoryStream GetMemoryStream(string? tag, long minimumSize)
        {
            return new RecyclableMemoryStream(this, tag, minimumSize);
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with the given tag 
        /// and at least the given capacity, possibly using a single continugous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call GetBuffer
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="minimumSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A MemoryStream.</returns>
        public RecyclableMemoryStream GetMemoryStream(string? tag, long minimumSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || minimumSize <= BlockSize)
                return GetMemoryStream(tag, minimumSize);

            return new RecyclableMemoryStream(this, minimumSize, Guid.Empty, tag, GetBuffer(minimumSize, tag));
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with the given 
        /// tag and with contents copied from the provided span.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>The <see cref="RecyclableMemoryStream"/>.</returns>
        public RecyclableMemoryStream GetMemoryStream(ReadOnlySpan<byte> data, string? tag = null)
        {
            RecyclableMemoryStream? stream = null;
            try
            {
                stream = new RecyclableMemoryStream(this, tag, data.Length);
                stream.Write(data);
                stream.Position = 0;
                return stream;
            }
            catch
            {
                stream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with the given 
        /// tag and with contents copied from the provided buffer.
        /// The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>The <see cref="RecyclableMemoryStream"/>.</returns>
        public RecyclableMemoryStream GetMemoryStream(byte[] buffer, int offset, int count, string? tag = null)
        {
            return GetMemoryStream(buffer.AsSpan(offset, count), tag);
        }

        /// <summary>
        /// Constructs a new <see cref="RecyclableMemoryStream"/> with the given 
        /// tag and with contents copied from the provided <see cref="Stream"/>.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="stream">The stream to read the data from.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>The <see cref="RecyclableMemoryStream"/>.</returns>
        public RecyclableMemoryStream GetMemoryStream(Stream stream, string? tag = null)
        {
            RecyclableMemoryStream? result = null;
            try
            {
                result = GetMemoryStream(tag);
                stream.CopyTo(result);
                result.Position = 0;
                return result;
            }
            catch
            {
                result?.Dispose();
                throw;
            }
        }

        #endregion
    }
}