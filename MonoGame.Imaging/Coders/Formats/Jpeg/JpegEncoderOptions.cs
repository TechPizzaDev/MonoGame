using System;

namespace MonoGame.Imaging.Coders.Formats.Jpeg
{
    [Serializable]
    public class JpegEncoderOptions : EncoderOptions, ICoderOptions<JpegEncoderOptions>
    {
        public static new JpegEncoderOptions Default { get; } = new();

        public int Quality { get; }
        public JpegSubsampling Subsampling { get; }
        
        public JpegEncoderOptions(int quality, JpegSubsampling subsampling = JpegSubsampling.Default)
        {
            switch (subsampling)
            {
                case JpegSubsampling.Allow:
                case JpegSubsampling.Disallow:
                case JpegSubsampling.Force:
                    Subsampling = subsampling;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(subsampling));
            }

            Quality = quality;
        }

        public JpegEncoderOptions() : this(90)
        {
        }
    }
}