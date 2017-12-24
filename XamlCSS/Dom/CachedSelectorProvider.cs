using System;
using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public class CachedSelectorProvider
    {
        private static CachedSelectorProvider instance = new CachedSelectorProvider();

        public static CachedSelectorProvider Instance  => instance;

        private Dictionary<string, ISelector> selectors = new Dictionary<string, ISelector>();

        public ISelector GetOrAdd(string selectorString)
        {
            if (selectors.ContainsKey(selectorString))
            {
                return selectors[selectorString];
            }

            var selector = new Selector(selectorString);

            selectors[selectorString] = selector;

            return selector;
        }
    }
}
