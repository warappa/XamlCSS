using System;
using Windows.UI.Xaml;

namespace XamlCSS.UWP
{
    public static class ClassExtensions
    {
        public static string ToggleClass(this DependencyObject obj, string @class)
        {
            if (obj == null)
            {
                return null;
            }

            var classes = @class.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var current = Css.GetClass(obj);

            foreach (var curClass in classes)
            {
                if (current == null ||
                    current.IndexOf(curClass) == -1)
                {
                    current = (current ?? "") + " " + curClass;
                }
                else
                {
                    current = current.Replace(curClass, "");
                }
            }

            current = current.Trim();

            Css.SetClass(obj, current);

            return current;
        }

        public static string AddClass(this DependencyObject obj, string @class)
        {
            if (obj == null)
            {
                return null;
            }

            var classes = @class.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var current = Css.GetClass(obj);
            foreach (var curClass in classes)
            {
                if (current == null ||
                    current.IndexOf(curClass) == -1)
                {
                    current = (current ?? "") + " " + curClass;
                }
            }

            current = current.Trim();

            Css.SetClass(obj, current);

            return current;
        }

        public static string RemoveClass(this DependencyObject obj, string @class)
        {
            if (obj == null)
            {
                return null;
            }

            var classes = @class.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var current = Css.GetClass(obj);
            foreach (var curClass in classes)
            {
                if (current != null &&
                    current.IndexOf(curClass) != -1)
                {
                    current = current.Replace(curClass, "");
                }
            }

            current = current.Trim();

            Css.SetClass(obj, current);

            return current;
        }
    }
}
