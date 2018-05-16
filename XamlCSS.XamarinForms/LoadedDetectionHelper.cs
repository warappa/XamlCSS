using Xamarin.Forms;
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

                rootElement.ChildAdded += RootElement_DescendantAdded;
                rootElement.ChildRemoved += RootElement_DescendantRemoved;

                if (rootElement is Application app)
                {
                    app.ModalPushing += App_ModalPushing;
                    app.ModalPopped += App_ModalPopped;
                }

                ElementAdded(root);

                initialized = true;
            }
        }

        private static void App_ModalPushing(object sender, ModalPushingEventArgs e)
        {
            e.Modal.Parent = sender as Application;

            ElementAdded(e.Modal);
        }

        private static void App_ModalPopped(object sender, ModalPoppedEventArgs e)
        {
            ElementRemoved(e.Modal);
        }

        private static void RootElement_DescendantAdded(object sender, ElementEventArgs e)
        {
            ElementAdded(e.Element);
        }

        private static void RootElement_DescendantRemoved(object sender, ElementEventArgs e)
        {
            ElementRemoved(e.Element);
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

        private static void ElementRemoved(Element dependencyObject)
        {
            Css.instance?.RemoveElement(dependencyObject);
            var dom = Css.instance?.treeNodeProvider.GetDomElement(dependencyObject) as DomElement;

            dom.ElementUnloaded();

            var logicalParent = dom?.LogicalParent?.Element;
            var visualParent = dom?.Parent?.Element;

            if (logicalParent != visualParent)
                Css.instance?.UpdateElement(visualParent);
            Css.instance?.UpdateElement(logicalParent);
        }

        private static void ElementAdded(Element dependencyObject)
        {
            if (Css.instance == null)
            {
                return;
            }

            var dom = Css.instance.treeNodeProvider.GetDomElement(dependencyObject) as DomElement;
            dom.ElementLoaded();

            Css.instance.NewElement(dependencyObject);

            if (dom.ApplyStyleImmediately)
            {
                Css.instance.ExecuteApplyStyles();
            }
        }
    }
}
