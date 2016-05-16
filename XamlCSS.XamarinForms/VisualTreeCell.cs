using System;
using Xamarin.Forms;
using XamlCSS.Windows.Media;

namespace XamlCSS.XamarinForms
{
	public static class VisualTreeCell
	{
		public static readonly BindableProperty IncludeProperty =
			BindableProperty.CreateAttached(
				"Include",
				typeof(bool),
				typeof(VisualTreeCell),
				false,
				propertyChanged: OnIncludeChanged);

		public static bool GetInclude(BindableObject view)
		{
			return (bool)view.GetValue(IncludeProperty);
		}

		public static void SetInclude(BindableObject view, bool value)
		{
			view.SetValue(IncludeProperty, value);
		}

		static void OnIncludeChanged(BindableObject view, object oldValue, object newValue)
		{
			var entry = view as Cell;
			if (entry == null)
			{
				return;
			}

			bool register = (bool)newValue;
			if (register)
			{
				entry.Appearing += Entry_Appearing;
				entry.Disappearing += Entry_Disappearing;
			}
			else
			{
				entry.Appearing -= Entry_Appearing;
				entry.Disappearing -= Entry_Disappearing;
			}
		}

		private static void Entry_Disappearing(object sender, EventArgs e)
		{
			VisualTreeHelper.Exclude(sender as Element);
		}

		private static void Entry_Appearing(object sender, EventArgs e)
		{
			VisualTreeHelper.Include(sender as Element);
		}
	}
}
