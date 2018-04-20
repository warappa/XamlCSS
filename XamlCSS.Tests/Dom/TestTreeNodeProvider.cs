using System;
using System.Collections.Generic;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
    public class TestTreeNodeProvider : ITreeNodeProvider<UIElement>
    {
        private static TestTreeNodeProvider instance = new TestTreeNodeProvider();
        public static TestTreeNodeProvider Instance => instance;

        public IDomElement<UIElement> CreateTreeNode(UIElement dependencyObject)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UIElement> GetChildren(UIElement element, SelectorType type)
        {
            throw new NotImplementedException();
        }

        public IDomElement<UIElement> GetDomElement(UIElement obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDomElement<UIElement>> GetDomElementChildren(IDomElement<UIElement> node, SelectorType type)
        {
            throw new NotImplementedException();
        }

        public UIElement GetParent(UIElement dependencyObject, SelectorType type)
        {
            throw new NotImplementedException();
        }

        public bool IsInTree(UIElement dependencyObject, SelectorType type)
        {
            return true;
        }
    }
}