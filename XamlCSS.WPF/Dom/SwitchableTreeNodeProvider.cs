using System.Collections.Generic;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    public class SwitchableTreeNodeProvider : TreeNodeProviderBase<DependencyObject, Style, DependencyProperty>, ISwitchableTreeNodeProvider<DependencyObject>
    {
        private ITreeNodeProvider<DependencyObject> currentTreeNodeProvider = null;
        private VisualTreeNodeProvider visualTreeNodeProvider;
        private LogicalTreeNodeProvider logicalTreeNodeProvider;

        public SwitchableTreeNodeProvider(IDependencyPropertyService<DependencyObject, DependencyObject, Style, DependencyProperty> dependencyPropertyService,
            VisualTreeNodeProvider visualTreeNodeProvider,
            LogicalTreeNodeProvider logicalTreeNodeProvider
            )
            : base(dependencyPropertyService, SelectorType.LogicalTree)
        {
            this.visualTreeNodeProvider = visualTreeNodeProvider;
            this.logicalTreeNodeProvider = logicalTreeNodeProvider;

            currentTreeNodeProvider = logicalTreeNodeProvider;
        }

        public SelectorType CurrentSelectorType => selectorType;

        public void Switch(SelectorType type)
        {
            selectorType = type;

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
            if (selectorType == SelectorType.LogicalTree)
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
            if (selectorType == SelectorType.LogicalTree)
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
}
