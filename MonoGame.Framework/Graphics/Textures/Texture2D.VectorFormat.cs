// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using MonoGame.Framework.Memory;
using MonoGame.Framework.Vectors;

namespace MonoGame.Framework.Graphics
{
    public partial class Texture2D
    {
        private static Dictionary<SurfaceFormat, HashSet<VectorFormat>> VectorFormatBySurface { get; } = new();

        private static Dictionary<SurfaceFormat, ReadOnlySet<VectorFormat>> VectorFormatBySurfaceRO { get; } = new();

        private static Dictionary<Type, HashSet<VectorFormat>> VectorFormatsByType { get; } = new();

        private static Dictionary<Type, ReadOnlySet<VectorFormat>> VectorFormatsByTypeRO { get; } = new();

        private static void InitializeVectorFormats()
        {
            // TODO: implement SRgb

            AddVectorFormat(SurfaceFormat.Alpha8, typeof(Alpha8));
            AddVectorFormat(SurfaceFormat.Single, typeof(RedF));
            AddVectorFormat(SurfaceFormat.Rgb24, typeof(Rgb24));
            //AddVectorFormat(SurfaceFormat.Rgba32SRgb, typeof(Rgba32SRgb));
            AddVectorFormat(SurfaceFormat.Rgba32, typeof(Color), typeof(Byte4));
            AddVectorFormat(SurfaceFormat.Rg32, typeof(Rg32));
            AddVectorFormat(SurfaceFormat.Rgba64, typeof(Rgba64));
            AddVectorFormat(SurfaceFormat.Rgba1010102, typeof(Rgba1010102));
            AddVectorFormat(SurfaceFormat.Bgr565, typeof(Bgr565));
            AddVectorFormat(SurfaceFormat.Bgra5551, typeof(Bgra5551));
            AddVectorFormat(SurfaceFormat.Bgra4444, typeof(Bgra4444));

            //AddVectorFormat(SurfaceFormat.Bgr32SRgb, typeof(Bgr32SRgb));
            //AddVectorFormat(SurfaceFormat.Bgra32SRgb, typeof(Bgra32SRgb));
            AddVectorFormat(SurfaceFormat.Bgr32, typeof(Bgr32));
            AddVectorFormat(SurfaceFormat.Bgra32, typeof(Bgra32));

            AddVectorFormat(SurfaceFormat.HalfSingle, typeof(HalfSingle));
            AddVectorFormat(SurfaceFormat.HalfVector2, typeof(HalfVector2));
            AddVectorFormat(SurfaceFormat.HalfVector4, typeof(HalfVector4));
            AddVectorFormat(SurfaceFormat.Vector2, typeof(RgVector));
            AddVectorFormat(SurfaceFormat.Vector4, typeof(RgbaVector));
            AddVectorFormat(SurfaceFormat.HdrBlendable, typeof(HalfVector4));

            AddVectorFormat(SurfaceFormat.NormalizedByte2, typeof(NormalizedByte2));
            AddVectorFormat(SurfaceFormat.NormalizedByte4, typeof(NormalizedByte4));
        }

        private static VectorFormat AddVectorFormat(SurfaceFormat surfaceFormat, params Type[] vectorTypes)
        {
            var vectorInfos = vectorTypes.Select(x => VectorType.Get(x));
            var vectorFormat = new VectorFormat(surfaceFormat, vectorInfos);

            if (!VectorFormatBySurface.TryGetValue(surfaceFormat, out var bySurfaceSet))
            {
                bySurfaceSet = new HashSet<VectorFormat>();
                VectorFormatBySurface.TryAdd(surfaceFormat, bySurfaceSet);
                VectorFormatBySurfaceRO.TryAdd(surfaceFormat, bySurfaceSet.AsReadOnly());
            }
            bySurfaceSet.Add(vectorFormat);

            foreach (var vectorType in vectorTypes)
            {
                if (!VectorFormatsByType.TryGetValue(vectorType, out var byTypeSet))
                {
                    byTypeSet = new HashSet<VectorFormat>();
                    VectorFormatsByType.TryAdd(vectorType, byTypeSet);
                    VectorFormatsByTypeRO.TryAdd(vectorType, byTypeSet.AsReadOnly());
                }
                byTypeSet.Add(vectorFormat);
            }

            return vectorFormat;
        }

        private static Exception GetUnsupportedSurfaceFormatException(
            SurfaceFormat surfaceFormat, string paramName)
        {
            var innerException = surfaceFormat.IsCompressedFormat()
                ? new ArgumentException("Compressed texture formats are currently not supported.")
                : null;

            return new ArgumentException(
                $"The format {surfaceFormat} is not supported.", paramName, innerException);
        }

        private static Exception GetUnsupportedVectorTypeException(
            Type vectorType, string paramName)
        {
            return new ArgumentException($"The vector type {vectorType} is not supported.", paramName);
        }

        /// <summary>
        /// Tries to map a surface format to a vector format.
        /// </summary>
        /// <exception cref="ArgumentException">Compressed texture formats are not supported.</exception>
        /// <returns>A set of vector formats the represent the surface format.</returns>
        public static ReadOnlySet<VectorFormat> GetVectorFormats(SurfaceFormat surfaceFormat)
        {
            if (!VectorFormatBySurfaceRO.TryGetValue(surfaceFormat, out var formatSet))
                throw GetUnsupportedSurfaceFormatException(surfaceFormat, nameof(surfaceFormat));
            return formatSet;
        }

        /// <summary>
        /// Tries to map a vector type to a vector format.
        /// </summary>
        /// <returns>A set of vector formats the represent the vector type.</returns>
        public static ReadOnlySet<VectorFormat> GetVectorFormats(Type vectorType)
        {
            if (!VectorFormatsByTypeRO.TryGetValue(vectorType, out var formatSet))
                throw GetUnsupportedVectorTypeException(vectorType, nameof(vectorType));
            return formatSet;
        }

        /// <summary>
        /// Tries to map a surface format to a vector format.
        /// </summary>
        /// <exception cref="ArgumentException">Compressed texture formats are not supported.</exception>
        public static VectorFormat GetVectorFormat(SurfaceFormat surfaceFormat)
        {
            return GetVectorFormats(surfaceFormat).FirstOrDefault() ??
                throw GetUnsupportedSurfaceFormatException(surfaceFormat, nameof(surfaceFormat));
        }

        /// <summary>
        /// Tries to map a vector type to a vector format.
        /// </summary>
        public static VectorFormat GetVectorFormat(Type vectorType)
        {
            return GetVectorFormats(vectorType).FirstOrDefault() ??
                throw GetUnsupportedVectorTypeException(vectorType, nameof(vectorType));
        }

        /// <summary>
        /// Represents a mapping between a surface format and vector formats.
        /// </summary>
        public class VectorFormat
        {
            private static MethodInfo _getDataMethod;

            /// <summary>
            /// Represents a delegate that can retrieve data from a texture.
            /// </summary>
            public delegate IMemory GetDataDelegate(
                Texture2D texture, Rectangle? rectangle, int level, int arraySlice);

            /// <summary>
            /// Gets the surface format that represents this vector format.
            /// </summary>
            public SurfaceFormat SurfaceFormat { get; }

            /// <summary>
            /// Gets structurally equal vector types that represent this vector format.
            /// </summary>
            public ReadOnlyMemory<VectorType> VectorTypes { get; }

            /// <summary>
            /// Gets a delegate that can retrieve texture data of this vector format.
            /// </summary>
            public GetDataDelegate GetData { get; }

            static VectorFormat()
            {
                var methodParams = typeof(GetDataDelegate).GetDelegateParameters().Skip(1);
                _getDataMethod = typeof(Texture2D).GetMethod(
                    "GetData", methodParams.Select(x => x.ParameterType).ToArray()) ??
                    throw new TypeLoadException("Failed to find method required for reflection.");
            }

            /// <summary>
            /// Constructs the vector format.
            /// </summary>
            /// <param name="surfaceFormat">The surface format to map to vector types.</param>
            /// <param name="vectorTypes">The vector types to map to a surface format.</param>
            public VectorFormat(SurfaceFormat surfaceFormat, IEnumerable<VectorType> vectorTypes)
            {
                if (vectorTypes == null)
                    throw new ArgumentNullException(nameof(vectorTypes));

                VectorTypes = vectorTypes.ToArray();
                if (VectorTypes.IsEmpty)
                    throw new ArgumentEmptyException(nameof(vectorTypes));

                SurfaceFormat = surfaceFormat;
                GetData = GetGetDataDelegate(VectorTypes.Span[0].Type);
            }

            private static GetDataDelegate GetGetDataDelegate(Type type)
            {
                var genericMethod = _getDataMethod.MakeGenericMethod(type);
                return genericMethod.CreateDelegate<GetDataDelegate>();
            }
        }
    }
}
