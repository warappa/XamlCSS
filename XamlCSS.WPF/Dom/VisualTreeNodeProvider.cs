using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class SwitchableTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>, ISwitchableTreeNodeProvider<DependencyObject>
    {
        private SelectorType currentSelectorType = SelectorType.LogicalTree;
        private ITreeNodeProvider<DependencyObject> currentTreeNodeProvider = null;
        private VisualWithLogicalFallbackTreeNodeProvider visualTreeNodeProvider;
        private LogicalTreeNodeProvider logicalTreeNodeProvider;

        public SwitchableTreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService,
            VisualWithLogicalFallbackTreeNodeProvider visualTreeNodeProvider,
            LogicalTreeNodeProvider logicalTreeNodeProvider
            )
            : base(dependencyPropertyService)
        {
            this.visualTreeNodeProvider = visualTreeNodeProvider;
            this.logicalTreeNodeProvider = logicalTreeNodeProvider;

            currentTreeNodeProvider = logicalTreeNodeProvider;
        }

        public SelectorType CurrentSelectorType => currentSelectorType;

        public void Switch(SelectorType type)
        {
            currentSelectorType = type;

            if (type == SelectorType.LogicalTree)
            {
                currentTreeNodeProvider = logicalTreeNodeProvider;
            }
            else
            {
                currentTreeNodeProvider = visualTreeNodeProvider;
            }
        }

        protected internal override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            if (currentSelectorType == SelectorType.LogicalTree)
            {
                return logicalTreeNodeProvider.CreateTreeNode(dependencyObject);
            }
            else
            {
                return visualTreeNodeProvider.CreateTreeNode(dependencyObject);
            }
        }

        protected internal override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
        {
            if (currentSelectorType == SelectorType.LogicalTree)
            {
                return logicalTreeNodeProvider.IsCorrectTreeNode(node);
            }
            else
            {
                return visualTreeNodeProvider.IsCorrectTreeNode(node);
            }
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            return currentTreeNodeProvider.GetChildren(element);
        }

        public override DependencyObject GetParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            return currentTreeNodeProvider.GetParent(element);
        }

        public override bool IsInTree(DependencyObject tUIElement)
        {
            return currentTreeNodeProvider.IsInTree(tUIElement);
        }
    }
    public class VisualWithLogicalFallbackTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public ITreeNodeProvider<DependencyObject> VisualTreeNodeProvider { get; }
        public ITreeNodeProvider<DependencyObject> LogicalTreeNodeProvider { get; }

        public VisualWithLogicalFallbackTreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService,
            ITreeNodeProvider<DependencyObject> visualTreeNodeProvider,
            ITreeNodeProvider<DependencyObject> logicalTreeNodeProvider
            )
            : base(dependencyPropertyService)
        {
            this.VisualTreeNodeProvider = visualTreeNodeProvider;
            this.LogicalTreeNodeProvider = logicalTreeNodeProvider;
        }

        protected internal override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new VisualDomElement(dependencyObject, this);
        }

        protected internal override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
        {
            return node is VisualDomElement;
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            var list = new List<DependencyObject>();

            if (element == null)
            {
                return list;
            }

            try
            {
                if (element is Visual ||
                    element is Visual3D)
                {
                    var children = VisualTreeNodeProvider.GetChildren(element).ToList();
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];

                        if (child != null)
                        {
                            list.Add(child);
                        }
                    }
                }
                else
                {
                    var children = LogicalTreeNodeProvider.GetChildren(element).ToList();
                    for (int i = 0; i < children.Count; i++)
                    {
                        var child = children[i];

                        if (child != null)
                        {
                            list.Add(child);
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        public override DependencyObject GetParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            if (element is Visual ||
                       element is Visual3D)
            {

                return VisualTreeNodeProvider.GetParent(element);
            }
            else
            {
                return LogicalTreeNodeProvider.GetParent(element);
            }
        }

        public override bool IsInTree(DependencyObject element)
        {
            return VisualTreeNodeProvider.IsInTree(element);
        }
    }
    public class VisualTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>
    {
        public VisualTreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService)
            : base(dependencyPropertyService)
        {
        }

        protected internal override IDomElement<DependencyObject> CreateTreeNode(DependencyObject dependencyObject)
        {
            return new VisualDomElement(dependencyObject, this);
        }

        protected internal override bool IsCorrectTreeNode(IDomElement<DependencyObject> node)
        {
            return node is VisualDomElement;
        }

        public override IEnumerable<DependencyObject> GetChildren(DependencyObject element)
        {
            var list = new List<DependencyObject>();

            if (element == null)
            {
                return list;
            }

            try
            {
                if (element is Visual ||
                    element is Visual3D)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
                    {
                        var child = VisualTreeHelper.GetChild(element, i) as DependencyObject;

                        if (child != null)
                        {
                            list.Add(child);
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        public override DependencyObject GetParent(DependencyObject element)
        {
            if (element == null)
            {
                return null;
            }

            return VisualTreeHelper.GetParent(element);
        }

        public override bool IsInTree(DependencyObject tUIElement)
        {
            return true;
        }
    }
}
