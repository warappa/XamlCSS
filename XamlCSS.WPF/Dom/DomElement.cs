using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using XamlCSS.Dom;
using XamlCSS.Utils;

namespace XamlCSS.WPF.Dom
{
    public abstract class DomElement : DomElementBase<DependencyObject, DependencyProperty>
    {
        public DomElement(DependencyObject dependencyObject, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(dependencyObject, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded += DomElementAdded;
            LoadedDetectionHelper.SubTreeRemoved += DomElementRemoved;
        }

        public new void Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        private void UnregisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded -= DomElementAdded;
            LoadedDetectionHelper.SubTreeRemoved -= DomElementRemoved;
        }

        protected override IList<string> GetClassList(DependencyObject dependencyObject)
        {
            var list = new List<string>();

            var classNames = Css.GetClass(dependencyObject)?.Split(classSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (classNames?.Length > 0)
            {
                list.AddRange(classNames);
            }

            return list;
        }
        protected override string GetId(DependencyObject dependencyObject)
        {
            if (dependencyObject is FrameworkElement)
            {
                return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
            }

            return dependencyObject.ReadLocalValue(FrameworkContentElement.NameProperty) as string;
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

        protected override IDictionary<string, DependencyProperty> CreateNamedNodeMap(DependencyObject dependencyObject)
        {
            return TypeHelpers.DeclaredDependencyProperties<DependencyProperty>(dependencyObject.GetType())
                .ToDictionary(x => x.Name, x => x.Property);
        }
    }
}
