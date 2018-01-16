using System;
using XamlCSS.Dom;

namespace XamlCSS
{
    public interface IDependencyPropertyService<TDependencyObject, TStyle, TDependencyProperty>
        where TDependencyObject : class
        where TStyle : class
        where TDependencyProperty : class
    {
        bool IsLoaded(TDependencyObject obj);

        void RegisterLoadedOnce(TDependencyObject frameworkElement, Action<object> func);

        string[] GetMatchingStyles(TDependencyObject obj);
        void SetMatchingStyles(TDependencyObject obj, string[] value);

        string[] GetAppliedMatchingStyles(TDependencyObject obj);
        void SetAppliedMatchingStyles(TDependencyObject obj, string[] value);

        bool GetHandledCss(TDependencyObject obj);
        void SetHandledCss(TDependencyObject obj, bool value);

        StyleSheet GetStyledByStyleSheet(TDependencyObject obj);
        void SetStyledByStyleSheet(TDependencyObject obj, StyleSheet value);

        string GetName(TDependencyObject obj);
        void SetName(TDependencyObject obj, string value);

        TStyle GetInitialStyle(TDependencyObject obj);
        void SetInitialStyle(TDependencyObject obj, TStyle value);

        bool? GetHadStyle(TDependencyObject obj);
        void SetHadStyle(TDependencyObject obj, bool? value);

        StyleDeclarationBlock GetStyle(TDependencyObject obj);
        void SetStyle(TDependencyObject obj, StyleDeclarationBlock value);

        StyleSheet GetStyleSheet(TDependencyObject obj);
        void SetStyleSheet(TDependencyObject obj, StyleSheet value);

        string GetClass(TDependencyObject obj);
        void SetClass(TDependencyObject obj, string value);

        object GetDependencyPropertyValue(Type type, string propertyName, TDependencyProperty property, object value);

        TDependencyProperty GetDependencyProperty(TDependencyObject frameworkElement, string propertyName);
        TDependencyProperty GetDependencyProperty(Type uiElementType, string propertyName);

        IDomElement<TDependencyObject> GetDomElement(TDependencyObject obj, SelectorType selectorType);
        void SetDomElement(TDependencyObject obj, IDomElement<TDependencyObject> value, SelectorType selectorType);

        object GetValue(TDependencyObject obj, string propertyName);
        void SetValue(TDependencyObject obj, string propertyName, object value);
    }
}
