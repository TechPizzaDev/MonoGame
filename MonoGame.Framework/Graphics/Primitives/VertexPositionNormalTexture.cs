using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MonoGame.Framework.Graphics
{
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionNormalTexture : IVertexType, IEquatable<VertexPositionNormalTexture>
    {
        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration(new[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(0x18, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        });

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        [DataMember]
        public Vector3 Position;

        [DataMember]
        public Vector3 Normal;

        [DataMember]
        public Vector2 TexCoord;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }

        public readonly bool Equals(VertexPositionNormalTexture other)
        {
            return this == other;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is VertexPositionNormalTexture other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position, Normal, TexCoord);
        }

        public override readonly string ToString()
        {
            return "{Position:" + Position + " Normal:" + Normal + " TexCoord:" + TexCoord + "}";
        }

        public static bool operator ==(in VertexPositionNormalTexture left, in VertexPositionNormalTexture right)
        {
            return (left.Position == right.Position)
                && (left.Normal == right.Normal)
                && (left.TexCoord == right.TexCoord);
        }

        public static bool operator !=(in VertexPositionNormalTexture left, in VertexPositionNormalTexture right)
        {
            return !(left == right);
        }
    }
}
