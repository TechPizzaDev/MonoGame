using System;
using System.Text;

namespace MonoGame.Framework.Vectors
{
    /// <summary>
    /// Describes a vector and the data it stores.
    /// </summary>
    public readonly struct VectorComponentInfo : IEquatable<VectorComponentInfo>
    {
        /// <summary>
        /// Gets the vector component definitions in data order.
        /// </summary>
        public ReadOnlyMemory<VectorComponent> Components { get; }

        /// <summary>
        /// Gets the sum of the all the component sizes in bits.
        /// </summary>
        public int BitDepth
        {
            get
            {
                int sum = 0;
                foreach (ref readonly VectorComponent v in Components.Span)
                {
                    sum += v.Bits;
                }
                return sum;
            }
        }

        /// <summary>
        /// Gets the bit size of the component with the smallest size.
        /// </summary>
        public int MinBitDepth
        {
            get
            {
                int min = int.MaxValue;
                foreach (ref readonly VectorComponent v in Components.Span)
                {
                    min = Math.Min(min, v.Bits);
                }
                return min;
            }
        }

        /// <summary>
        /// Gets the bit size of the component with the largest size.
        /// </summary>
        public int MaxBitDepth
        {
            get
            {
                int max = int.MinValue;
                foreach (ref readonly VectorComponent v in Components.Span)
                {
                    max = Math.Max(max, v.Bits);
                }
                return max;
            }
        }

        /// <summary>
        /// Constructs the <see cref="VectorComponentInfo"/> with the specified component definitions.
        /// </summary>
        /// <param name="components">The vector components in data order.</param>
        /// <exception cref="ArgumentNullException"><paramref name="components"/> is null.</exception>
        /// <exception cref="ArgumentEmptyException"><paramref name="components"/> is empty.</exception>
        public VectorComponentInfo(params VectorComponent[] components)
        {
            if (components == null) throw new ArgumentNullException(nameof(components));
            if (components.Length == 0) throw new ArgumentEmptyException(nameof(components));

            foreach (var component in components)
                AssertValidComponent(component);

            Components = (VectorComponent[]) components.Clone();
        }

        /// <summary>
        /// Constructs the <see cref="VectorComponentInfo"/> with one component definition.
        /// </summary>
        /// <param name="component">The vector component.</param>
        public VectorComponentInfo(VectorComponent component)
        {
            AssertValidComponent(component);

            Components = new[] { component };
        }

        private static void AssertValidComponent(VectorComponent component)
        {
            if (!component.IsValid)
                throw new ArgumentException("The component is not valid.", nameof(component));
        }

        public bool HasComponentType(VectorComponentChannel componentType)
        {
            foreach (var component in Components.Span)
                if (component.Channel == componentType)
                    return true;
            return false;
        }

        public bool Equals(VectorComponentInfo other)
        {
            return this == other;
        }

        public override bool Equals(object? obj)
        {
            return obj is VectorComponentInfo info && Equals(info);
        }

        public override int GetHashCode()
        {
            var code = new HashCode();
            foreach (var comp in Components.Span)
                code.Add(comp);
            return code.ToHashCode();
        }

        public override string ToString()
        {
            var components = Components.Span;
            int bitDepth = 0;

            var builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                bitDepth += component.Bits;

                builder.Append(component.Channel).Append(':');

                if (component.Type == VectorComponentType.BitField)
                    builder.Append(component.Bits).Append('u');
                else if (component.Type == VectorComponentType.SignedBitField)
                    builder.Append(component.Bits).Append('i');
                else
                    builder.Append(component.Type.ToShortString());

                if (i < components.Length - 1)
                    builder.Append(", ");
            }
            builder.Append("]: ");
            builder.Append(bitDepth).Append("bit");
            return builder.ToString();
        }

        public static bool operator ==(in VectorComponentInfo a, in VectorComponentInfo b)
        {
            return a.Components.Span.SequenceEqual(b.Components.Span);
        }

        public static bool operator !=(in VectorComponentInfo a, in VectorComponentInfo b)
        {
            return !(a == b);
        }
    }
}
