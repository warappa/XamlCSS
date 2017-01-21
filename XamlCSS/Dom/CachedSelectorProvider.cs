using AngleSharp.Dom.Css;
using System;
using System.Collections.Generic;

namespace XamlCSS.Dom
{
    public class CachedSelectorProvider
    {
        private Dictionary<string, ISelector> selectors = new Dictionary<string, ISelector>();

        public ISelector GetOrAdd(string selectorString, Func<string, ISelector> selectorFactory)
        {
            if (selectors.ContainsKey(selectorString))
            {
                return selectors[selectorString];
            }

            lock (selectors)
            {
                var selector = selectorFactory(selectorString);

                selectors[selectorString] = selector;

                return selector;
            }
        }
    }
}
