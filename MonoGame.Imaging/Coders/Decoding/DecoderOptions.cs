using System;
using MonoGame.Imaging.Coders;

namespace MonoGame.Imaging
{
    /// <summary>
    /// Base class for decoder options.
    /// </summary>
    [Serializable]
    public class DecoderOptions : CoderOptions, ICoderOptions<DecoderOptions>
    {
        public static new DecoderOptions Default { get; } = new();

        /// <summary>
        /// Gets or sets whether image memory should be 
        /// cleared on allocation as it may contain garbage otherwise. 
        /// Useful when the image will be used before it is fully decoded.
        /// <para>
        /// Defaults to <see langword="true"/>.
        /// </para>
        /// </summary>
        public bool ClearImageMemory { get; set; }

        public DecoderOptions()
        {
            ClearImageMemory = true;
        }
    }
}
