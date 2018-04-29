using System.Collections.Generic;
using Xamarin.Forms;
using XamlCSS.XamarinForms.Dom;

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
                propertyChanged: OnIncludeChanged
                );

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
            var entry = view;
            if (entry == null)
            {
                return;
            }

            bool register = (bool)newValue;
            if (register)
            {
                entry.PropertyChanged += Entry_PropertyChanged;
                entry.PropertyChanging += Entry_PropertyChanging;
            }
            else
            {
                entry.PropertyChanged -= Entry_PropertyChanged;
                entry.PropertyChanging -= Entry_PropertyChanging;
            }
        }

        private static void Entry_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName == "Parent")
            {
                var s = sender as Element;
                if (s.Parent == null)
                {
                    Css.RemoveAdditionalChild(s.Parent, s);
                }
            }
        }

        private static void Entry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Parent")
            {
                var s = sender as Element;
                if (s.Parent != null)
                {
                    Css.AddAdditionalChild(s.Parent, s);
                }
            }
        }
    }
}
