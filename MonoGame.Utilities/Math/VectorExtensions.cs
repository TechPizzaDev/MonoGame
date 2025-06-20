using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace System.Numerics;

internal static class VectorExtensions
{
#if NET9_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 AsVector2(this Vector3 vector) => vector.AsVector128Unsafe().AsVector2();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AsVector3(this Vector2 vector) => vector.AsVector128().AsVector3();
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 AsVector2(this Vector3 vector) => vector.AsVector128().AsVector2();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AsVector3(this Vector2 vector) => vector.AsVector128().AsVector3();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 AsVector2(this Vector4 vector) => vector.AsVector128().AsVector2();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AsVector3(this Vector4 vector) => vector.AsVector128().AsVector3();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 AsVector4(this Vector2 vector) => vector.AsVector128().AsVector4();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 AsVector4(this Vector3 vector) => vector.AsVector128().AsVector4();
#endif

}