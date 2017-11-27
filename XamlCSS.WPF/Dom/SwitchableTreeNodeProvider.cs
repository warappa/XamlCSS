using System.Collections.Generic;
using System.Windows;
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
}
