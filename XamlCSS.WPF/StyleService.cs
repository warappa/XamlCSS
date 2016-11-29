using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace XamlCSS.WPF
{
	public class StyleService : INativeStyleService<Style, DependencyObject, DependencyProperty>
	{
		protected const string StyleSheetStyleKey = "StyleSheetStyle";

		public Style CreateFrom(IDictionary<DependencyProperty, object> dict, Type forType)
		{
			Style style;

			if (forType != null)
			{
				style = new Style(forType);
			}
			else
			{
				style = new Style();
			}
            
			foreach (var i in dict)
			{
				style.Setters.Add(new Setter(i.Key, i.Value));
			}

			return style;
		}

		public IDictionary<DependencyProperty, object> GetStyleAsDictionary(Style style)
		{
            if (style == null)
            {
                return null;
            }

            return style.Setters.OfType<Setter>().ToDictionary(x => x.Property, x => x.Value);
		}

		public void SetStyle(DependencyObject visualElement, Style s)
		{
			if (visualElement is FrameworkElement)
			{
				(visualElement as FrameworkElement).Style = s;
			}
			else if (visualElement is FrameworkContentElement)
			{
				(visualElement as FrameworkContentElement).Style = s;
			}
		}

		public string GetStyleResourceKey(Type type, string selector)
		{
			return $"{StyleSheetStyleKey}_${type.FullName}_{selector}";
		}

		public string BaseStyleResourceKey {  get { return StyleSheetStyleKey; } }
	}
}
