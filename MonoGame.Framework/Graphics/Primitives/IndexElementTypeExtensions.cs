using System;

namespace MonoGame.Framework.Graphics
{
    public static partial class IndexElementTypeExtensions
    {
        /// <summary>
        /// Gets the size of the element type in bytes.
        /// </summary>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static int TypeSize(this IndexElementType elementType)
        {
            switch (elementType)
            {
                case IndexElementType.Int16:
                    return 2;

                case IndexElementType.Int32:
                    return 4;

                default:
                    ThrowHelper.ArgumentOutOfRange(null, nameof(elementType));
                    return 0;
            }
        }
    }
}
