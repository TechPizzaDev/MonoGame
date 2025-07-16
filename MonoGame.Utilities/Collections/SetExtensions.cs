using System.Collections.ObjectModel;

namespace System.Collections.Generic;

public static class SetExtensions
{
#if !NET10_0_OR_GREATER
    public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set)
    {
        return new ReadOnlySet<T>(set);
    }
#endif
}
