using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MonoGame.Framework
{
    public static class Matrix4x4Extensions
    {
        public static void CopyTo(in this Matrix4x4 value, Span<float> destination)
        {
            MemoryMarshal.AsBytes(new ReadOnlySpan<Matrix4x4>(in value)).CopyTo(MemoryMarshal.AsBytes(destination));
        }

        /// <summary>
        /// The backward vector formed from the third row M31, M32, M33 elements.
        /// </summary>
        public static Vector3 Backward(in this Matrix4x4 value)
        {
            return new Vector3(value.M31, value.M32, value.M33);
        }

        /// <summary>
        /// The down vector formed from the second row -M21, -M22, -M23 elements.
        /// </summary>
        public static Vector3 Down(in this Matrix4x4 value)
        {
            return new Vector3(-value.M21, -value.M22, -value.M23);
        }

        /// <summary>
        /// The forward vector formed from the third row -M31, -M32, -M33 elements.
        /// </summary>
        public static Vector3 Forward(in this Matrix4x4 value)
        {
            return new Vector3(-value.M31, -value.M32, -value.M33);
        }

        /// <summary>
        /// The left vector formed from the first row -M11, -M12, -M13 elements.
        /// </summary>
        public static Vector3 Left(in this Matrix4x4 value)
        {
            return new Vector3(-value.M11, -value.M12, -value.M13);
        }

        /// <summary>
        /// The right vector formed from the first row M11, M12, M13 elements.
        /// </summary>
        public static Vector3 Right(in this Matrix4x4 value)
        {
            return new Vector3(value.M11, value.M12, value.M13);
        }

        /// <summary>
        /// The upper vector formed from the second row M21, M22, M23 elements.
        /// </summary>
        public static Vector3 Up(in this Matrix4x4 value)
        {
            return new Vector3(value.M21, value.M22, value.M23);
        }
    }
}
