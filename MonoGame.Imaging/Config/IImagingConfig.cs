
using System.Diagnostics.CodeAnalysis;

namespace MonoGame.Imaging
{
    public interface IImagingConfig
    {
        bool TryGetModule<T>([MaybeNullWhen(false)] out T? module)
            where T : class;
    }
}
