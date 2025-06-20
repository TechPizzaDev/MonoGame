using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System;
using System.Numerics;

namespace MonoGame.Framework.Graphics
{
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColor : IVertexType, IEquatable<VertexPositionColor>
    {
        [DataMember]
        public Vector3 Position;

        [DataMember]
        public Color Color;

        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration(new[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        });

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPositionColor(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public readonly bool Equals(VertexPositionColor other)
        {
            return this == other;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is VertexPositionColor other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, Color);
        }

        public override readonly string ToString()
        {
            return "{Position:" + Position + " Color:" + Color + "}";
        }

        public static bool operator ==(in VertexPositionColor a, in VertexPositionColor b)
        {
            return (a.Color == b.Color)
                && (a.Position == b.Position);
        }

        public static bool operator !=(in VertexPositionColor a, in VertexPositionColor b)
        {
            return !(a == b);
        }
    }
}
