using System;
using System.Collections.Generic;
using XamlCSS.Utils;

namespace XamlCSS.Dom
{
    public class LazyDependencyPropertyDictionary<TDependencyProperty> : Dictionary<string, TDependencyProperty>, IDictionary<string, TDependencyProperty>
        where TDependencyProperty : class
    {
        private Type type;

        public LazyDependencyPropertyDictionary(Type type)
        {
            this.type = type;
        }

        TDependencyProperty IDictionary<string, TDependencyProperty>.this[string key]
        {
            get
            {
                return this[key];
            }
            set
            {
                this[key] = value;
            }
        }

        public new TDependencyProperty this[string key]
        {
            get
            {
                if (!TryGetValue(key, out TDependencyProperty prop))
                {
                    prop = TypeHelpers.GetDependencyPropertyInfo<TDependencyProperty>(type, key)?.Property;
                    Add(key, prop);
                }

                return prop;
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
