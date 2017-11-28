using System;
using System.Collections.Generic;

namespace XamlCSS
{
    public abstract class StyleServiceBase<TStyle, TDependencyObject, TDependencyProperty> : INativeStyleService<TStyle, TDependencyObject, TDependencyProperty>
        where TDependencyObject : class
        where TDependencyProperty : class
        where TStyle : class
    {
        protected const string StyleSheetStyleKey = "StyleSheetStyle";

        public TStyle CreateFrom(IDictionary<TDependencyProperty, object> dict, IEnumerable<TDependencyObject> triggers, Type forType)
        {
            TStyle style = CreateStyle(forType);

            foreach (var i in dict)
            {
                AddSetter(style, i.Key, i.Value);
            }

            foreach (var trigger in triggers)
            {
                AddTrigger(style, trigger);
            }

            return style;
        }
        
        public abstract TDependencyObject CreateTrigger(StyleSheet styleSheet, ITrigger trigger, Type targetType, TDependencyObject styleResourceReferenceHolder);

        public abstract IDictionary<TDependencyProperty, object> GetStyleAsDictionary(TStyle style);

        public abstract void SetStyle(TDependencyObject visualElement, TStyle style);

        public string GetStyleResourceKey(string styleSheetId, Type type, string selector)
        {
            return $"{StyleSheetStyleKey}_{styleSheetId}_${type.FullName}{{{selector}";
        }

        public abstract IEnumerable<TDependencyObject> GetTriggersAsList(TStyle style);

        public string BaseStyleResourceKey { get { return StyleSheetStyleKey; } }

        protected abstract TStyle CreateStyle(Type forType);

        protected abstract void AddSetter(TStyle style, TDependencyProperty property, object value);

        protected abstract void AddTrigger(TStyle style, TDependencyObject trigger);

    }
}