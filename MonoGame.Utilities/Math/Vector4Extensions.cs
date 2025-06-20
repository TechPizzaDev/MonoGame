using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace MonoGame.Framework
{
    public static class Vector4Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 AddLowerToUpper(this Vector4 rect)
        {
            Vector128<float> xywh = rect.AsVector128();
            xywh += Vector128.Shuffle(xywh, Vector128.Create(-1, -1, 0, 1));
            return xywh.AsVector4();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetUpper(this Vector4 vector)
        {
            return Vector128.Shuffle(vector.AsVector128(), Vector128.Create(2, 3, 0, 1)).AsVector2();
        }
    }
}
