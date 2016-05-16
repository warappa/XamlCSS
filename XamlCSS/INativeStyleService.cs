using System;
using System.Collections.Generic;

namespace XamlCSS
{
	public interface INativeStyleService<TStyle, TDependencyObject, TDependencyProperty>
		where TDependencyObject : class
		where TDependencyProperty : class
		where TStyle : class
	{
		TStyle CreateFrom(IDictionary<TDependencyProperty, object> dict, Type forType);
		void SetStyle(TDependencyObject visualElement, TStyle s);
		IDictionary<TDependencyProperty, object> GetStyleAsDictionary(TStyle style);
		string GetStyleResourceKey(Type type, string selector);
		string BaseStyleResourceKey { get; }
	}
}
