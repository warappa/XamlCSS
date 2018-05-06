using System.Collections.Generic;

namespace XamlCSS.Utils
{
    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        public static LinkedHashSet<T> ToLinkedHashSet<T>(this IEnumerable<T> source)
        {
            return new LinkedHashSet<T>(source);
        }
    }
}
