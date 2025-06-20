using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MonoGame.OpenAL;

namespace MonoGame.Framework.Audio
{
    internal static class ALHelper
    {
        internal static void CheckError(string message)
        {
            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                ThrowError(message, error);
            }
        }

        internal static void CheckError<K, V>(K key, V value, [CallerMemberName] string? prefix = null)
        {
            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                ThrowError($"{prefix}: Failed to set {key} to {value}", error);
            }
        }

        private static void ThrowError(string message, ALError error)
        {
            ThrowHelper.InvalidOperation($"{message} (Reason: {error})");
        }

        public static bool IsStereoFormat(ALFormat format)
        {
            return format == ALFormat.Stereo8
                || format == ALFormat.Stereo16
                || format == ALFormat.StereoFloat32
                || format == ALFormat.StereoIma4
                || format == ALFormat.StereoMSAdpcm;
        }

        public static ALFormat GetALFormat(AudioChannels channels, AudioDepth depth)
        {
            switch (channels)
            {
                case AudioChannels.Mono:
                    switch (depth)
                    {
                        case AudioDepth.Short: return ALFormat.Mono16;
                        case AudioDepth.Float: return ALFormat.MonoFloat32;
                    }
                    break;

                case AudioChannels.Stereo:
                    switch (depth)
                    {
                        case AudioDepth.Short: return ALFormat.Stereo16;
                        case AudioDepth.Float: return ALFormat.StereoFloat32;
                    }
                    break;

                default:
                    ThrowHelper.ArgumentOutOfRange("Only mono and stereo channels are supported.", nameof(channels));
                    return default;
            }
            ThrowHelper.ArgumentOutOfRange("Audio format is not supported.", nameof(depth));
            return default;
        }

        public static ALFormat GetALFormat(AudioChannels channels, bool isFloat)
        {
            return GetALFormat(channels, isFloat ? AudioDepth.Float : AudioDepth.Short);
        }
    }
}
