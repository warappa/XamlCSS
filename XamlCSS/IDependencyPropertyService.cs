using System;

namespace XamlCSS
{
	public interface IDependencyPropertyService<TDependencyObject, TUIElement, TStyle, TDependencyProperty>
		where TUIElement : class, TDependencyObject
		where TDependencyObject : class
		where TStyle : class 
		where TDependencyProperty : class
	{
		bool IsLoaded(TUIElement obj);

		void RegisterLoadedOnce(TUIElement frameworkElement, Action<object> func);

		string[] GetMatchingStyles(TDependencyObject obj);
		void SetMatchingStyles(TDependencyObject obj, string[] value);

		string[] GetAppliedMatchingStyles(TDependencyObject obj);
		void SetAppliedMatchingStyles(TDependencyObject obj, string[] value);
		
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

		object GetBindablePropertyValue(Type type, TDependencyProperty property, object value);

		TDependencyProperty GetBindableProperty(TDependencyObject frameworkElement, string propertyName);
		TDependencyProperty GetBindableProperty(Type uiElementType, string propertyName);
	}
}
