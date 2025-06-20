// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace MonoGame.Framework.Audio
{
    /// <summary>
    /// Provides functionality for manipulating multiple sounds at a time.
    /// </summary>
    public class AudioCategory : IEquatable<AudioCategory>
    {
        private readonly AudioEngine _engine;
        private readonly List<XactSound> _sounds;

        internal bool isBackgroundMusic;
        internal bool isPublic;

        internal bool instanceLimit;
        internal int maxInstances;
        internal MaxInstanceBehavior InstanceBehavior;

        internal CrossfadeType fadeType;
        internal float fadeIn;
        internal float fadeOut;

        internal float Volume { get; private set; }

        /// <summary>
        /// Gets the category's friendly name.
        /// </summary>
        public string Name { get; }

        internal AudioCategory(AudioEngine audioEngine, string name, BinaryReader reader)
        {
            Debug.Assert(audioEngine != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            _sounds = new List<XactSound>();
            Name = name;
            _engine = audioEngine;

            maxInstances = reader.ReadByte();
            instanceLimit = maxInstances != 0xff;

            fadeIn = reader.ReadUInt16() / 1000f;
            fadeOut = reader.ReadUInt16() / 1000f;

            byte instanceFlags = reader.ReadByte();
            fadeType = (CrossfadeType)(instanceFlags & 0x7);
            InstanceBehavior = (MaxInstanceBehavior)(instanceFlags >> 3);

            reader.ReadUInt16(); //unkn

            Volume = XactHelpers.ParseVolumeFromDecibels(reader.ReadByte());

            byte visibilityFlags = reader.ReadByte();
            isBackgroundMusic = (visibilityFlags & 0x1) != 0;
            isPublic = (visibilityFlags & 0x2) != 0;
        }

        internal void AddSound(XactSound sound)
        {
            _sounds.Add(sound);
        }

        internal int GetPlayingInstanceCount()
        {
            int sum = 0;
            for (var i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i].Playing)
                    sum++;
            }
            return sum;
        }

        internal XactSound? GetOldestInstance()
        {
            for (var i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i].Playing)
                    return _sounds[i];
            }
            return null;
        }

        /// <summary>
        /// Pauses all associated sounds.
        /// </summary>
        public void Pause()
        {
            foreach (var sound in _sounds)
                sound.Pause();
        }

        /// <summary>
        /// Resumes all associated paused sounds.
        /// </summary>
        public void Resume()
        {
            foreach (var sound in _sounds)
                sound.Resume();
        }

        /// <summary>
        /// Stops all associated sounds.
        /// </summary>
        public void Stop(AudioStopOptions options)
        {
            foreach (var sound in _sounds)
                sound.Stop(options);
        }

        /// <summary>
        /// Set the volume for this <see cref="AudioCategory"/>.
        /// </summary>
        /// <param name="volume">The new volume of the category.</param>
        /// <exception cref="ArgumentException">If the volume is less than zero.</exception>
        public void SetVolume(float volume)
        {
            if (volume < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(volume), "The volume value must be positive.");

            // Updating all the sounds in a category can be
            // very expensive... so avoid it if we can.
            if (Volume == volume)
                return;
            Volume = volume;

            foreach (var sound in _sounds)
                sound.UpdateCategoryVolume(volume);
        }

        /// <summary>
        /// Determines whether two <see cref="AudioCategory"/> instances are equal.
        /// </summary>
        public static bool operator ==(AudioCategory a, AudioCategory b)
        {
            return a._engine == b._engine
                && a.Name.Equals(b.Name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether two <see cref="AudioCategory"/> instances are not equal.
        /// </summary>
        public static bool operator !=(AudioCategory a, AudioCategory b) => !(a == b);

        /// <summary>
        /// Determines whether two <see cref="AudioCategory"/> instances are equal.
        /// </summary>
        public bool Equals(AudioCategory? other) => this == other;

        /// <summary>
        /// Determines whether two <see cref="AudioCategory"/> instances are equal.
        /// </summary>
        /// <param name="obj">Object to compare with this instance.</param>
        /// <returns>true if the objects are equal or false if they aren't.</returns>
        public override bool Equals(object? obj) => obj is AudioCategory other && Equals(other);

        /// <summary>
        /// Returns the friendly name of this <see cref="AudioCategory"/>.
        /// </summary>
        public override string ToString() => Name;

        /// <summary>
        /// Gets the hash code for this <see cref="AudioCategory"/>.
        /// </summary>
        public override int GetHashCode() => HashCode.Combine(Name, _engine);
    }
}