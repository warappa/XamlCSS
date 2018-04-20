using System.Collections.Generic;
using XamlCSS.Dom;
using Windows.UI.Xaml;
using System;
using System.Diagnostics;

namespace XamlCSS.UWP.Dom
{
    [DebuggerDisplay("Id={Id} Class={Class}")]
    public class DomElement : DomElementBase<DependencyObject, DependencyProperty>, IDisposable
    {
        public DomElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parent, IDomElement<DependencyObject> logicalParent, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(dependencyObject, parent, logicalParent, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded += ElementAdded;
            LoadedDetectionHelper.SubTreeRemoved += ElementRemoved;
        }

        void IDisposable.Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        private void UnregisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded -= ElementAdded;
            LoadedDetectionHelper.SubTreeRemoved -= ElementRemoved;
        }

        protected override IList<string> GetClassList(DependencyObject dependencyObject)
        {
            var list = new List<string>();
            var classNames = Css.GetClass(dependencyObject)?.Split(' ');

            if (classNames != null)
            {
                list.AddRange(classNames);
            }

            return list;
        }
        protected override string GetId(DependencyObject dependencyObject)
        {
            return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
        }

        protected override IDictionary<string, DependencyProperty> CreateNamedNodeMap(DependencyObject dependencyObject)
        {
            return new LazyDependencyPropertyDictionary<DependencyProperty>(dependencyObject.GetType());
        }

        public override bool Equals(object obj)
        {
            var other = obj as DomElement;
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.dependencyObject == other.dependencyObject;
        }

        public static bool operator ==(DomElement a, DomElement b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }
            return a?.Equals(b) == true;
        }
        public static bool operator !=(DomElement a, DomElement b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return dependencyObject?.GetHashCode() ?? 0;
        }
    }
}
