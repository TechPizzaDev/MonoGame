// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System;
using System.Numerics;

namespace MonoGame.Framework.Graphics
{
    [DataContract]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPosition : IVertexType, IEquatable<VertexPosition>
    {
        [DataMember]
        public Vector3 Position;

        public static VertexDeclaration VertexDeclaration { get; } = new VertexDeclaration(new[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
        });

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        public readonly bool Equals(VertexPosition other)
        {
            return this == other;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is VertexPosition other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Position);
        }

        public override readonly string ToString()
        {
            return "{Position:" + Position + "}";
        }

        public static bool operator ==(VertexPosition a, VertexPosition b)
        {
            return a.Position == b.Position;
        }

        public static bool operator !=(VertexPosition a, VertexPosition b)
        {
            return !(a == b);
        }
    }
}
