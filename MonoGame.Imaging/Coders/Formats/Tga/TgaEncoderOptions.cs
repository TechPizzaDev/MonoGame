using System;

namespace MonoGame.Imaging.Coders.Formats.Tga
{
    [Serializable]
    public class TgaEncoderOptions : EncoderOptions, ICoderOptions<TgaEncoderOptions>
    {
        public static new TgaEncoderOptions Default { get; } = new();

        public bool UseRunLengthEncoding { get; }

        public TgaEncoderOptions(bool useRunLengthEncoding)
        {
            UseRunLengthEncoding = useRunLengthEncoding;
        }
        
        public TgaEncoderOptions() : this(useRunLengthEncoding: true)
        {
        }
    }
}