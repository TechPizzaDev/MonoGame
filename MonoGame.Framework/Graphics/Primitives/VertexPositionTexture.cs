using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MonoGame.Framework.Graphics
{
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionTexture : IVertexType, IEquatable<VertexPositionTexture>
    {
        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration(new[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        });

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        [DataMember]
        public Vector3 Position;

        [DataMember]
        public Vector2 TexCoord;

        public VertexPositionTexture(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }

        public readonly bool Equals(VertexPositionTexture other)
        {
            return this == other;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is VertexPositionTexture other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, TexCoord);
        }

        public override readonly string ToString()
        {
            return "{Position:" + Position + " TexCoord:" + TexCoord + "}";
        }

        public static bool operator ==(in VertexPositionTexture left, in VertexPositionTexture right)
        {
            return (left.Position == right.Position)
                && (left.TexCoord == right.TexCoord);
        }

        public static bool operator !=(in VertexPositionTexture left, in VertexPositionTexture right)
        {
            return !(left == right);
        }
    }
}
