// This code originated from:
//
//    http://theinstructionlimit.com/ogg-streaming-using-opentk-and-nvorbis
//    https://github.com/renaudbedard/nvorbis/
//
// It was released to the public domain by the author (Renaud Bedard).
// No other license is intended or required. 

using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Framework.Audio;
using MonoGame.OpenAL;
using NVorbis;

namespace MonoGame.Framework.Media
{
    internal class OggStream : IDisposable
    {
        public object StopMutex { get; } = new object();
        public object ReadMutex { get; } = new object();

        private bool _leaveStreamOpen;
        private Stream _stream;
        private readonly Queue<ALBuffer> _queuedBuffers = new();

        private float _volume;
        private float _pitch;

        public OggStreamer Streamer { get; }
        public VorbisReader? Reader { get; private set; }
        public bool IsReady { get; private set; }
        public bool IsPreparing { get; private set; }
        public bool IsLooped { get; set; }

        public Action? OnFinished { get; private set; }
        public Action? OnLooped { get; private set; }

        public int QueuedBufferCount => _queuedBuffers.Count;
        public uint SourceId { get; private set; }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                UpdateVolume();
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                AL.Source(SourceId, ALSourcef.Pitch, _pitch = value);
                ALHelper.CheckError("Failed to set pitch.");
            }
        }

        public OggStream(
            Stream stream, bool leaveOpen, OggStreamer streamer,
            Action? onFinished = null, Action? onLooped = null)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            Streamer = streamer ?? throw new ArgumentNullException(nameof(streamer));
            _leaveStreamOpen = leaveOpen;
            OnFinished = onFinished;
            OnLooped = onLooped;

            SourceId = Streamer.Controller.ReserveSource();

            Volume = 1;
            Pitch = 1;
        }

        internal void UpdateVolume()
        {
            AL.Source(SourceId, ALSourcef.Gain, _volume * Song.MasterVolume);
            ALHelper.CheckError("Failed to set volume.");
        }

        public void Prepare(bool immediate)
        {
            lock (StopMutex)
            {
                if (IsPreparing)
                    return;

                var state = GetState();
                switch (state)
                {
                    case ALSourceState.Playing:
                    case ALSourceState.Paused:
                        return;

                    case ALSourceState.Stopped:
                        lock (ReadMutex)
                            Empty();
                        break;
                }

                if (!IsReady)
                {
                    lock (ReadMutex)
                    {
                        IsPreparing = true;
                        Open(precache: immediate);
                    }
                }
                else if (immediate)
                {
                    FillAndEnqueueBuffer();
                }

                IsPreparing = false;
            }
        }

        public void Play(bool immediate)
        {
            var state = GetState();
            switch (state)
            {
                case ALSourceState.Paused:
                    Resume();
                    return;

                case ALSourceState.Initial:
                case ALSourceState.Stopped:
                    Prepare(immediate);
                    Streamer.AddStream(this);

                    AL.SourcePlay(SourceId);
                    ALHelper.CheckError("Failed to play source.");
                    break;
            }
        }

        public void Pause()
        {
            var state = GetState();
            if (state != ALSourceState.Playing)
                return;

            Streamer.RemoveStream(this);
            AL.SourcePause(SourceId);
            ALHelper.CheckError("Failed to pause source.");
        }

        public void Resume()
        {
            var state = GetState();
            if (state != ALSourceState.Paused)
                return;

            Streamer.AddStream(this);
            AL.SourcePlay(SourceId);
            ALHelper.CheckError("Failed to play source.");
        }

        private void StopPlayback()
        {
            AL.SourceStop(SourceId);
            ALHelper.CheckError("Failed to stop source.");
        }

        public void Stop()
        {
            lock (StopMutex)
            {
                StopPlayback();

                lock (ReadMutex)
                {
                    Streamer.RemoveStream(this);
                    Empty();

                    if (Reader != null)
                        Reader.SamplePosition = 0;
                }

                AL.Source(SourceId, ALSourcei.Buffer, 0);
                ALHelper.CheckError("Failed to free source from buffers.");

                while (_queuedBuffers.Count > 0)
                    DequeueAndReturnBuffer();
            }
        }

        private void Empty(int attempts = 0)
        {
            AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out int processed);
            ALHelper.CheckError("Failed to fetch processed buffers.");

            // there are multiple attempts as some OpenAL implementations are faulty
            try
            {
                if (processed > 0)
                {
                    AL.SourceUnqueueBuffers(SourceId, processed);
                    ALHelper.CheckError("Failed to unqueue buffers (first attempt).");

                    for (int i = 0; i < processed; i++)
                        DequeueAndReturnBuffer();
                }
            }
            catch (InvalidOperationException)
            {
                if (processed > 0)
                {
                    AL.SourceUnqueueBuffers(SourceId, processed);
                    ALHelper.CheckError("Failed to unqueue buffers (second attempt).");
                }

                // Try turning it off again?
                AL.SourceStop(SourceId);
                ALHelper.CheckError("Failed to stop source.");

                if (attempts < 5)
                    Empty(attempts++);
            }
        }

        /// <summary>
        /// Seeking stops playback and empties buffers.
        /// </summary>
        public void SeekTo(TimeSpan pos)
        {
            if (Reader == null)
                return;

            lock (ReadMutex)
            {
                Stop();
                Reader.TimePosition = pos;
            }
        }

        public TimeSpan GetPosition()
        {
            if (Reader == null)
                return default;

            lock (ReadMutex)
                return Reader.TimePosition;
        }

        public TimeSpan? GetTotalTime()
        {
            if (Reader == null)
                return default;

            lock (ReadMutex)
                return Reader.TotalTime;
        }

        public ALSourceState GetState()
        {
            var state = AL.GetSourceState(SourceId);
            ALHelper.CheckError("Failed to get source state.");
            return state;
        }

        public void Open(bool precache = false)
        {
            if (Reader == null)
            {
                if (_stream == null)
                    throw new ObjectDisposedException(GetType().FullName);

                Reader = new VorbisReader(_stream, _leaveStreamOpen);
            }

            if (precache)
                FillAndEnqueueBuffer();

            IsReady = true;
        }

        private void FillAndEnqueueBuffer()
        {
            if (Streamer.TryFillBuffer(this, out var buffer))
                QueueBuffer(buffer);
        }

        public void QueueBuffer(ALBuffer buffer)
        {
            AL.SourceQueueBuffer(SourceId, buffer.BufferId);
            ALHelper.CheckError("Failed to queue buffer.");

            _queuedBuffers.Enqueue(buffer);
        }

        public void DequeueAndReturnBuffer()
        {
            var buffer = _queuedBuffers.Dequeue();
            ALBufferPool.Return(buffer);
        }

        public override int GetHashCode() => HashCode.Combine(SourceId);

        internal void Close()
        {
            lock (ReadMutex)
            {
                Reader?.Dispose();
                Reader = null!;

                IsReady = false;

                if (_stream != null && !_leaveStreamOpen)
                {
                    _stream.Dispose();
                    _stream = null!;
                }
            }
        }

        public void Dispose()
        {
            lock (StopMutex)
            {
                var state = GetState();
                if (state == ALSourceState.Playing || state == ALSourceState.Paused)
                    StopPlayback();

                lock (ReadMutex)
                {
                    Streamer.RemoveStream(this);
                    if (state != ALSourceState.Initial)
                        Empty();
                    Close();
                }

                AL.Source(SourceId, ALSourcei.Buffer, 0);
                ALHelper.CheckError("Failed to free source from buffers.");
            }

            while (_queuedBuffers.Count > 0)
                DequeueAndReturnBuffer();

            Streamer.Controller.RecycleSource(SourceId);
            SourceId = 0;
        }
    }
}