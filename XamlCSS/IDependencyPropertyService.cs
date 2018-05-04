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

        string GetName(TDependencyObject obj);
        void SetName(TDependencyObject obj, string value);

        TStyle GetInitialStyle(TDependencyObject obj);
        void SetInitialStyle(TDependencyObject obj, TStyle value);

        StyleDeclarationBlock GetStyle(TDependencyObject obj);
        void SetStyle(TDependencyObject obj, StyleDeclarationBlock value);

        StyleSheet GetStyleSheet(TDependencyObject obj);
        void SetStyleSheet(TDependencyObject obj, StyleSheet value);

        string GetClass(TDependencyObject obj);
        void SetClass(TDependencyObject obj, string value);

        object GetDependencyPropertyValue(Type type, string propertyName, TDependencyProperty property, object value);

        TDependencyProperty GetDependencyProperty(TDependencyObject frameworkElement, string propertyName);
        TDependencyProperty GetDependencyProperty(Type uiElementType, string propertyName);

        IDomElement<TDependencyObject, TDependencyProperty> GetDomElement(TDependencyObject obj);
        void SetDomElement(TDependencyObject obj, IDomElement<TDependencyObject, TDependencyProperty> value);

        object GetValue(TDependencyObject obj, string propertyName);
        void SetValue(TDependencyObject obj, string propertyName, object value);
    }
}
