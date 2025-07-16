
using MonoGame.Framework.Vectors;

namespace MonoGame.Imaging
{
    public class ImageInfo
    {
        public int Width { get; }
        public int Height { get; }
        public VectorComponentInfo ComponentInfo { get; }
        public ImageFormat Format { get; }

        // TODO: add meta data (that can be read AND written)

        public ImageInfo(
            int width, int height, VectorComponentInfo componentInfo, ImageFormat format)
        {
            Width = width;
            Height = height;
            ComponentInfo = componentInfo;
            Format = format;
        }

        public override string ToString()
        {
            VectorComponentInfo info = ComponentInfo;
            string withAlpha = info.HasComponentType(VectorComponentChannel.Alpha) ? "with alpha" : "";
            return $"{Format.ShortName} {Width}x{Height} {info.BitDepth}bit {withAlpha}";
        }
    }
}
