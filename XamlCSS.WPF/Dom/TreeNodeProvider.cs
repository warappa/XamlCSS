using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class TreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        private ApplicationDependencyObject applicationDependencyObject;
        private readonly bool isInDesigner;
        private DependencyObject designerWindowInstance;

        public TreeNodeProvider(IDependencyPropertyService<DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
            this.applicationDependencyObject = new ApplicationDependencyObject(Application.Current);
            this.isInDesigner = DesignerProperties.GetIsInDesignMode(new DependencyObject());
        }

        public override IDomElement<DependencyObject, DependencyProperty> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new DomElement(dependencyObject, GetDomElement(GetParent(dependencyObject, SelectorType.VisualTree)), GetDomElement(GetParent(dependencyObject, SelectorType.LogicalTree)), this);
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element, SelectorType type)
        {
            var list = new List<DependencyObject>();

            if (element == null)
            {
                return list;
            }

            if (element == applicationDependencyObject)
            {
                if (isInDesigner)
                {
                    if (designerWindowInstance == null)
                    {
                        return list;
                    }

                    return new List<DependencyObject> { designerWindowInstance };
                }
                return Application.Current.Windows.Cast<Window>().ToList();
            }

            if (type == SelectorType.VisualTree)
            {
                try
                {
                    list.AddRange(GetVisualChildren(element));
                }
                catch { }
            }
            else
            {
                list = GetLogicalChildren(element).ToList();
            }

            return list;
        }

        public IEnumerable<DependencyObject> GetLogicalChildren(DependencyObject element)
        {
            if (element == null)
            {
                yield break;
            }

            if (element is ItemsControl ic)
            {
                for (int i = 0; i < ic.Items.Count; i++)
                {
                    var uiElement = ic.ItemContainerGenerator.ContainerFromIndex(i);
                    if (uiElement != null)
                    {
                        var found = GetLogicalChildren(uiElement).FirstOrDefault();
                        if (found == null)
                        {
                            yield return uiElement;
                        }
                        else
                        {
                            yield return found;
                        }
                    }
                }
            }

            if (element is ItemsPresenter itemsPresenter)
            {
                var itemshost = itemsPresenter != null ? VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel : null;

                if (itemshost == null)
                {
                    yield break;
                }

                var p = GetVisualChildren(itemshost).FirstOrDefault();
                if (p != null)
                {
                    var children = GetLogicalChildren(p);
                    foreach (var child in children)
                    {
                        yield return child;
                    }
                }

                yield break;
            }
            else if (element is ContentPresenter c)
            {
                var childrenOfLogicalParent = GetChildrenOfLogicalParent(element, GetVisualChildren(element));
                foreach (var child in childrenOfLogicalParent)
                {
                    yield return child;
                }
                yield break;
            }
            else if (element is ContentControl frame)
            {
                var content = frame.Content as DependencyObject;
                if (content != null)
                {
                    yield return content;
                }
                yield break;
            }

            var childrenOfLogicalTreeHelper = LogicalTreeHelper.GetChildren(element)
                .Cast<object>()
                .OfType<DependencyObject>();

            foreach (var child in childrenOfLogicalTreeHelper)
            {
                yield return child;
            }
        }

        private IEnumerable<DependencyObject> GetChildrenOfLogicalParent(DependencyObject searchParent, IEnumerable<DependencyObject> elements)
        {
            foreach (var element in elements)
            {
                var found = false;
                if (element is FrameworkElement f)
                {
                    if (f.Parent == searchParent ||
                        f.TemplatedParent == searchParent)
                    {
                        yield return f;
                        found = true;
                    }
                }
                else if (element is FrameworkContentElement fc)
                {
                    if (fc.Parent == searchParent ||
                        fc.TemplatedParent == searchParent)
                    {
                        yield return fc;
                        found = true;
                    }
                }

                if (!found)
                {
                    var childrenOfLogicalParent = GetChildrenOfLogicalParent(searchParent, GetVisualChildren(element));
                    foreach (var child in childrenOfLogicalParent)
                    {
                        yield return child;
                    }
                }
            }
        }

        public IEnumerable<DependencyObject> GetVisualChildren(DependencyObject element)
        {
            if (element == null)
            {
                yield break;
            }

            if (element is Visual ||
                element is Visual3D)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                {
                    DependencyObject child = null;
                    try
                    {
                        child = VisualTreeHelper.GetChild(element, i) as DependencyObject;
                    }
                    catch
                    {

                    }
                    if (child != null)
                    {
                        yield return child;
                    }
                }
            }
        }

        public override bool IsInTree(DependencyObject element, SelectorType type)
        {
            if (type == SelectorType.LogicalTree)
            {
                return IsInLogicalTree(element);
            }
            return IsInVisualTree(element);
        }
        private bool IsInVisualTree(DependencyObject element)
        {
            var p = GetVisualParent(element);
            if (p == null)
                return element is Window || element == applicationDependencyObject;

            if (isInDesigner &&
                element.GetType().Name == "WindowInstance")
            {
                EnsureDesignerWindowInstanceCaptured(element);
                return true;
            }

            var isElementInParentsChildren = GetChildren(p, SelectorType.VisualTree).Contains(element);

            return isElementInParentsChildren;
        }
        private bool IsInLogicalTree(DependencyObject element)
        {
            var p = GetLogicalParent(element);
            if (p == null)
                return element is Window || element == applicationDependencyObject;

            if (isInDesigner &&
                element.GetType().Name == "WindowInstance")
            {
                EnsureDesignerWindowInstanceCaptured(element);
                return true;
            }

            if (element is ContentPresenter ||
                element is ItemsPresenter)
            {
                return false;
            }

            var isElementInParentsChildren = GetChildren(p, SelectorType.LogicalTree).Contains(element);

            return isElementInParentsChildren;
        }

        public override DependencyObject GetParent(DependencyObject element, SelectorType type)
        {
            if (element == null)
            {
                return null;
            }

            DependencyObject parent = null;

            if (element is Window ||
                (isInDesigner && element.GetType().Name == "WindowInstance"))
            {
                EnsureDesignerWindowInstanceCaptured(element);
                return applicationDependencyObject;
            }

            if (type == SelectorType.VisualTree)
            {
                if (element is Visual ||
                    element is Visual3D)
                {
                    parent = GetVisualParent(element);
                }
                else if (element is FrameworkContentElement)
                {
                    parent = GetLogicalParent(element);
                }
            }
            else
            {
                parent = GetLogicalParent(element);
            }

            return parent;
        }

        private void EnsureDesignerWindowInstanceCaptured(DependencyObject element)
        {
            if (isInDesigner &&
                designerWindowInstance == null &&
                element is FrameworkElement fe && fe.Name == "windowInstance")
            {
                designerWindowInstance = element;
            }
        }

        private DependencyObject GetVisualParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            if (element is Window ||
                (isInDesigner && element.GetType().Name == "WindowInstance"))
            {
                EnsureDesignerWindowInstanceCaptured(element);
                return applicationDependencyObject;
            }

            if (element is Visual ||
                element is Visual3D)
            {
                return VisualTreeHelper.GetParent(element);
            }

            // LoadedDetection: would insert into Logical Dom Tree
            return null;// LogicalTreeHelper.GetParent(element);
        }
        private DependencyObject GetLogicalParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            var p = LogicalTreeHelper.GetParent(element);

            if (p == null) // templated?
            {
                if (element is FrameworkElement f)
                {
                    p = f.TemplatedParent;
                }
                else if (element is FrameworkContentElement fc)
                {
                    p = fc.TemplatedParent;
                }

                if (p == null)
                {
                    if (element is ContentControl cc)
                    {
                        p = cc.TemplatedParent ?? GetVisualParent(cc);
                    }
                }

                if (p == null)
                {
                    p = GetVisualParent(element);
                }

                if (p is ContentPresenter cp)
                {
                    p = cp.TemplatedParent ?? GetVisualParent(cp);
                }

                if (p is Frame frame)
                {

                }

                if (p is Panel panel)
                {
                    if (panel.IsItemsHost)
                    {
                        p = panel.TemplatedParent ?? GetVisualParent(panel);
                    }
                }

                if (p is ItemsPresenter ip)
                {
                    p = ip.TemplatedParent;
                }
            }

            return p;
        }
    }
}
