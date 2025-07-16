using System;

namespace MonoGame.Imaging.Coders
{
    /// <summary>
    /// Base class for coder options.
    /// </summary>
    [Serializable]
    public class CoderOptions : ICoderOptions<CoderOptions>
    {
        public static CoderOptions Default { get; } = new();
    }
}
