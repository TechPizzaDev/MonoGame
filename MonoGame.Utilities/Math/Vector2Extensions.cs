using System.Numerics;
using System.Runtime.CompilerServices;

namespace MonoGame.Framework
{
    public static class Vector2Extensions
    {
        public static PointF ToPointF(this Vector2 vector)
        {
            return Unsafe.BitCast<Vector2, PointF>(vector);
        }

        public static SizeF ToSizeF(this Vector2 vector)
        {
            return Unsafe.BitCast<Vector2, SizeF>(vector);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point ToPoint(this Vector2 vector)
        {
            vector += new Vector2(0.5f);
            return new Point((int)vector.X, (int)vector.Y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Size ToSize(this Vector2 vector)
        {
            vector += new Vector2(0.5f);
            return new Size((int)vector.X, (int)vector.Y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToVector4(this Vector2 vector) => new(vector, 0, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToVector4(this Vector2 vector, Vector2 upper) => new(vector, upper.X, upper.Y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToRect(this Vector2 position, Vector2 size)
        {
            return position.ToVector4(size);
        }
    }
}
