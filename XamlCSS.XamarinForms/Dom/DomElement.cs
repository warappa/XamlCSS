using System.Collections.Generic;
using XamlCSS.Dom;
using Xamarin.Forms;
using System;
using XamlCSS.Windows.Media;
using XamlCSS.Utils;
using System.Linq;

namespace XamlCSS.XamarinForms.Dom
{
    public abstract class DomElement : DomElementBase<BindableObject, BindableProperty>, IDisposable
    {
        public DomElement(BindableObject dependencyObject, ITreeNodeProvider<BindableObject> treeNodeProvider, INamespaceProvider<BindableObject> namespaceProvider)
            : base(dependencyObject, treeNodeProvider, namespaceProvider)
        {
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
        {
            VisualTreeHelper.SubTreeAdded += DomElementAdded;
            VisualTreeHelper.SubTreeRemoved += DomElementRemoved;
        }

        public new void Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        private void UnregisterChildrenChangeHandler()
        {
            VisualTreeHelper.SubTreeAdded -= DomElementAdded;
            VisualTreeHelper.SubTreeRemoved -= DomElementRemoved;
        }

        protected override IList<string> GetClassList(BindableObject dependencyObject)
        {
            var list = new List<string>();

            var classNames = Css.GetClass(dependencyObject)?.Split(classSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (classNames?.Length > 0)
            {
                list.AddRange(classNames);
            }

            return list;
        }

        protected override IDictionary<string, BindableProperty> CreateNamedNodeMap(BindableObject dependencyObject)
        {
            return TypeHelpers.DeclaredDependencyProperties<BindableProperty>(dependencyObject.GetType())
                .ToDictionary(x => x.Name, x => x.Property);
        }

        protected override string GetId(BindableObject dependencyObject)
        {
            return Css.GetId(dependencyObject);
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
