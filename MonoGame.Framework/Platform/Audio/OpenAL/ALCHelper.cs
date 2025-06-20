using System.Diagnostics;
using MonoGame.OpenAL;

namespace MonoGame.Framework.Audio
{
    internal static class ALCHelper
    {
        internal static void CheckError(string message)
        {
            ALCError error = ALC.GetError();
            if (error != ALCError.NoError)
            {
                ThrowError(message, error);
            }
        }

        private static void ThrowError(string message, ALCError error)
        {
            ThrowHelper.InvalidOperation($"{message} (Reason: {error})");
        }
    }
}
