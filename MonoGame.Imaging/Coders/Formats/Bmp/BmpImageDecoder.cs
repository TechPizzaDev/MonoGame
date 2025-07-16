using System.IO;
using MonoGame.Framework.Memory;
using MonoGame.Imaging.Coders.Decoding;

namespace MonoGame.Imaging.Coders.Formats.Bmp
{
    public class BmpImageDecoder(IImagingConfig config, Stream stream, DecoderOptions decoderOptions) :
        StbImageDecoderBase<DecoderOptions>(config, stream, decoderOptions)
    {
        public override ImageFormat Format => ImageFormat.Bmp;

        protected override void Read()
        {
            _ = StbSharp.ImageRead.Bmp.Load(Reader, ReadState, RecyclableArrayPool.Shared);
        }
    }
}
