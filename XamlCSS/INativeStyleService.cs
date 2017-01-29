using System;
using System.Collections.Generic;

namespace XamlCSS
{
    public interface INativeStyleService<TStyle, TDependencyObject, TDependencyProperty>
        where TDependencyObject : class
        where TDependencyProperty : class
        where TStyle : class
    {
        TStyle CreateFrom(IDictionary<TDependencyProperty, object> dict, IEnumerable<TDependencyObject> triggers, Type forType);
        void SetStyle(TDependencyObject visualElement, TStyle s);
        IDictionary<TDependencyProperty, object> GetStyleAsDictionary(TStyle style);
        string GetStyleResourceKey(string styleSheetId, Type type, string selector);
        string BaseStyleResourceKey { get; }
        IEnumerable<TDependencyObject> GetTriggersAsList(TStyle style);
        TDependencyObject CreateTrigger(ITrigger trigger, Type targetType);
    }
}
