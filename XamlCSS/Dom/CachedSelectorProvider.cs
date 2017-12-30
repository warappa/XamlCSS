using System;
using System.Collections.Generic;
using XamlCSS.CssParsing;

namespace XamlCSS.Dom
{
    public class CachedSelectorProvider
    {
        private static CachedSelectorProvider instance = new CachedSelectorProvider();

        public static CachedSelectorProvider Instance  => instance;

        private Dictionary<string, ISelector> selectors = new Dictionary<string, ISelector>();

        public ISelector GetOrAdd(string selectorString, CssNode selectorAst = null)
        {
            if (selectors.ContainsKey(selectorString))
            {
                return selectors[selectorString];
            }

            var selector = new Selector(selectorString, selectorAst);

            selectors[selectorString] = selector;

            return selector;
        }
    }
}
