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
                entry.PropertyChanged += Entry_PropertyChanged;
                entry.BindingContextChanged += Entry_BindingContextChanged;
            }
            else
            {
                entry.Appearing -= Entry_Appearing;
                entry.PropertyChanged -= Entry_PropertyChanged;
                entry.BindingContextChanged -= Entry_BindingContextChanged;
            }
        }

        private static void Entry_BindingContextChanged(object sender, EventArgs e)
        {
            VisualTreeHelper.Exclude(sender as Element);
            VisualTreeHelper.Include(sender as Element);
        }


        private static void Entry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Parent")
            {
                var s = sender as Element;
                if (s.Parent != null)
                    VisualTreeHelper.Include(sender as Element);
                else
                    VisualTreeHelper.Exclude(sender as Element);
            }
        }

        private static void Entry_Appearing(object sender, EventArgs e)
        {
            VisualTreeHelper.Include(sender as Element);
        }
    }
}
