using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MonoGame.Framework.Audio;
using MonoGame.Framework.Utilities;
using MonoGame.OpenAL;

namespace MonoGame.Framework.Media
{
    // TODO: split up into a streamer base that supports multiple formats

    internal class OggStreamer : IDisposable
    {
        public const float DefaultUpdateRate = 16;
        public const int DefaultBufferSize = 8192;
        public const int MaxQueuedBuffers = 4;

        internal object IterationMutex { get; } = new object();
        private object FillMutex { get; } = new object();

        private float[] _readBuffer;
        private short[]? _castBuffer;
        internal readonly HashSet<OggStream> _streams = new();
        private TimeSpan[] _updateTiming;
        private float _updateRate;

        private readonly Thread _thread;
        private bool _pendingFinish;
        private volatile bool _cancelled;

        public ALController Controller { get; }
        public int BufferSize { get; }
        public TimeSpan UpdateDelay { get; private set; }

        public ReadOnlyMemory<TimeSpan> UpdateTiming => _updateTiming.AsMemory();

        public float UpdateRate
        {
            get => _updateRate;
            set
            {
                _updateRate = value;
                UpdateDelay = TimeSpan.FromSeconds(1 / ((value <= 0) ? 1.0 : value));
            }
        }

        public OggStreamer(int bufferSize = DefaultBufferSize, float updateRate = DefaultUpdateRate)
        {
            Controller = ALController.Get();

            BufferSize = bufferSize;
            UpdateRate = updateRate;

            _updateTiming = new TimeSpan[(int)Math.Max(1, UpdateRate)];

            _readBuffer = new float[BufferSize];
            if (!Controller.SupportsFloat32)
                _castBuffer = new short[BufferSize];

            _thread = new Thread(SongStreamingThread)
            {
                Name = "Song Streaming Thread",
                Priority = ThreadPriority.BelowNormal
            };
            _thread.Start();
        }

        internal bool AddStream(OggStream stream)
        {
            lock (IterationMutex)
                return _streams.Add(stream);
        }

        internal bool RemoveStream(OggStream stream)
        {
            lock (IterationMutex)
                return _streams.Remove(stream);
        }

        public bool TryFillBuffer(OggStream stream, [MaybeNullWhen(false)] out ALBuffer buffer)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            lock (FillMutex)
            {
                var reader = stream.Reader;
                if (reader == null)
                    throw new ArgumentException(
                        "The OggStream is missing a VorbisReader.", nameof(stream));

                int readSamples = reader.ReadSamples(_readBuffer);
                if (readSamples > 0)
                {
                    var channels = (AudioChannels)reader.Channels;
                    bool useFloat = Controller.SupportsFloat32;
                    ALFormat format = ALHelper.GetALFormat(channels, useFloat);

                    Span<float> dataSpan = _readBuffer.AsSpan(0, readSamples);
                    buffer = ALBufferPool.Rent();
                    if (useFloat)
                    {
                        buffer.BufferData(dataSpan, format, reader.SampleRate);
                    }
                    else
                    {
                        var castSpan = _castBuffer.AsSpan(0, readSamples);
                        AudioLoader.ConvertSingleToInt16(dataSpan, castSpan);
                        buffer.BufferData(castSpan, format, reader.SampleRate);
                    }
                    return true;
                }
            }
            buffer = null;
            return false;
        }

        private void SongStreamingThread()
        {
            var watch = new Stopwatch();
            var localStreams = new List<OggStream>();

            while (!_cancelled)
            {
                long sleepTicks = UpdateDelay.Ticks - watch.ElapsedTicks;
                if (sleepTicks > TimeSpan.TicksPerMillisecond)
                {
                    int sleepMillis = (int)(sleepTicks / TimeSpan.TicksPerMillisecond);
                    ThreadHelper.Instance.Sleep(watch, sleepMillis);
                }

                if (_cancelled)
                    break;

                watch.Restart();

                if (_streams.Count > 0)
                {
                    lock (IterationMutex)
                    {
                        foreach (var stream in _streams)
                            localStreams.Add(stream);
                    }

                    foreach (OggStream stream in localStreams)
                    {
                        lock (stream.ReadMutex)
                        {
                            lock (IterationMutex)
                                if (!_streams.Contains(stream))
                                    continue;

                            if (!FillStream(stream))
                                continue;
                        }

                        lock (stream.StopMutex)
                        {
                            if (stream.IsPreparing)
                                continue;

                            lock (IterationMutex)
                                if (!_streams.Contains(stream))
                                    continue;

                            var state = stream.GetState();
                            if (state == ALSourceState.Stopped)
                            {
                                AL.SourcePlay(stream.SourceId);
                                ALHelper.CheckError("Failed to play after fill.");
                            }
                        }
                    }
                    localStreams.Clear();

                    // Shift all elements forward, clipping off the last value...
                    Array.Copy(_updateTiming, 0, _updateTiming, destinationIndex: 1, _updateTiming.Length - 1);
                    // ... and set the first (now empty) element.
                    _updateTiming[0] = watch.Elapsed;
                }
                else
                {
                    Array.Clear(_updateTiming, 0, _updateTiming.Length);
                }

                watch.Stop();
            }
        }

        private bool FillStream(OggStream stream)
        {
            AL.GetSource(stream.SourceId, ALGetSourcei.BuffersProcessed, out int processed);
            ALHelper.CheckError("Failed to fetch processed buffers.");

            if (processed > 0)
            {
                AL.SourceUnqueueBuffers(stream.SourceId, processed);
                ALHelper.CheckError("Failed to unqueue buffers.");

                for (int i = 0; i < processed; i++)
                    stream.DequeueAndReturnBuffer();
            }

            int requested = Math.Max(MaxQueuedBuffers - stream.QueuedBufferCount, 0);
            if (requested == 0)
                return false;

            AL.GetSource(stream.SourceId, ALGetSourcei.BuffersQueued, out int queued);
            ALHelper.CheckError("Failed to fetch queued buffers.");

            int buffersFilled = 0;
            for (int i = 0; i < requested; i++)
            {
                if (TryFillBuffer(stream, out var buffer))
                {
                    stream.QueueBuffer(buffer);
                    buffersFilled++;
                }
                else
                {
                    if (stream.IsLooped)
                    {
                        lock (stream.ReadMutex)
                        {
                            if (stream.Reader != null)
                                stream.Reader.SamplePosition = 0;
                        }
                    }
                    else
                    {
                        _pendingFinish = true;
                        break;
                    }
                }
            }

            if (_pendingFinish && queued == 0)
            {
                _pendingFinish = false;
                lock (IterationMutex)
                    _streams.Remove(stream);

                stream.OnFinished?.Invoke();
            }
            else if (buffersFilled > 0)
            {
                return true;
            }
            else if (!stream.IsLooped)
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            _cancelled = true;

            lock (IterationMutex)
                _streams.Clear();

            _thread.Join(1000);
        }
    }
}