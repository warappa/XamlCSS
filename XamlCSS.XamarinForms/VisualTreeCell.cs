using Xamarin.Forms;

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
            var entry = view as Cell;
            if (entry == null)
            {
                return;
            }

            bool register = (bool)newValue;
            if (register)
            {
                entry.PropertyChanged += Entry_PropertyChanged;
            }
            else
            {
                entry.PropertyChanged -= Entry_PropertyChanged;
            }
        }

        private static void Entry_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Parent")
            {
                var s = sender as Element;
                if (s.Parent != null)
                {
                    Css.GetOverriddenChildren(s.Parent).Add(s);

                    Css.instance?.UpdateElement(sender as Element);
                }
                else
                {
                    Css.GetOverriddenChildren(s.Parent).Remove(s);

                    Css.instance?.RemoveElement(sender as Element);
                }
            }
        }
    }
}
