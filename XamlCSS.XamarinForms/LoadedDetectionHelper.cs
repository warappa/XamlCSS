using System;
using System.Diagnostics;
using Xamarin.Forms;
using XamlCSS.XamarinForms;
using XamlCSS.XamarinForms.Dom;

namespace XamlCSS.XamarinForms
{
    public class LoadedDetectionHelper
    {
        private static bool initialized = false;
        private static object lockObject = new object();
        private static Element rootElement;

        public static void Initialize(Element root)
        {
            lock (lockObject)
            {
                if (initialized == true)
                {
                    return;
                }

                Reset();

                rootElement = root;

                rootElement.DescendantAdded += RootElement_DescendantAdded;
                rootElement.DescendantRemoved += RootElement_DescendantRemoved;

                RootElement_DescendantAdded(root, new ElementEventArgs(rootElement));

                initialized = true;
            }

        }

        public static void Reset()
        {
            lock (lockObject)
            {
                if (initialized == false)
                {
                    return;
                }

                RootElement_DescendantRemoved(rootElement, new ElementEventArgs(rootElement));

                rootElement = null;

                initialized = false;
            }
        }

        private static void RootElement_DescendantRemoved(object sender, ElementEventArgs e)
        {
            Css.instance?.RemoveElement(e.Element);
            //Css.instance?.treeNodeProvider.Switch(SelectorType.LogicalTree);
            var dom = Css.instance?.treeNodeProvider.GetDomElement(e.Element) as DomElement;

            dom.ElementUnloaded();

            var logicalParent = dom?.LogicalParent?.Element;
            var visualParent = dom?.Parent?.Element;

            if (logicalParent != visualParent)
                Css.instance?.UpdateElement(visualParent);
            Css.instance?.UpdateElement(logicalParent);
        }

        private static void RootElement_DescendantAdded(object sender, ElementEventArgs e)
        {
            //Css.instance.treeNodeProvider.Switch(SelectorType.LogicalTree);
            var dom = Css.instance?.treeNodeProvider.GetDomElement(e.Element) as DomElement;
            dom.ElementLoaded();

            if(sender is ListView iv)
            {
                iv.ChildAdded += (s, ev) =>
                {
                    Debug.WriteLine("List: child added");
                };
                iv.ChildRemoved+= (s, ev) =>
                {
                    Debug.WriteLine("List: child Removed");
                };

                iv.ItemAppearing+= (s, ev) =>
                {
                    Debug.WriteLine("List: ItemAppearing");
                };
                iv.ItemDisappearing += (s, ev) =>
                {
                    Debug.WriteLine("List: ItemDisappearing");
                };
                iv.DescendantAdded+= (s, ev) =>
                {
                    Debug.WriteLine("List: child DescendantAdded");
                };
                iv.DescendantRemoved += (s, ev) =>
                {
                    Debug.WriteLine("List: child DescendantRemoved");
                };
            }

            //var visualPath = dom.GetPath(SelectorType.VisualTree);
            //Debug.WriteLine("new dom:\n    " + visualPath);
            //var logicalPath = dom.GetPath(SelectorType.LogicalTree);
            //Debug.WriteLine("    " + logicalPath);

            //var visualElementPath = ((DependencyObject)sender).GetElementPath(Css.instance?.treeNodeProvider, SelectorType.VisualTree);
            //Debug.WriteLine("  element:\n    " + visualElementPath);
            //var logicalElementPath = ((DependencyObject)sender).GetElementPath(Css.instance?.treeNodeProvider, SelectorType.LogicalTree);
            //Debug.WriteLine("    " + logicalElementPath);

            //SubTreeAdded?.Invoke(sender, e);
            Css.instance?.NewElement(sender as BindableObject);
        }
    }
}
