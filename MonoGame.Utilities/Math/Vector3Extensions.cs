using System.Numerics;
using System.Runtime.CompilerServices;

namespace MonoGame.Framework
{
    public static class Vector3Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToVector4(this Vector3 vector) => new(vector, 1);
    }
}
