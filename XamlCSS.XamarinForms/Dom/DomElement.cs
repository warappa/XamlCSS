using System.Collections.Generic;
using XamlCSS.Dom;
using Xamarin.Forms;
using System;
using XamlCSS.Windows.Media;
using System.Diagnostics;

namespace XamlCSS.XamarinForms.Dom
{
    [DebuggerDisplay("Id={Id} Class={Class}")]
    public class DomElement : DomElementBase<BindableObject, BindableProperty>, IDisposable
    {
        public DomElement(BindableObject dependencyObject, IDomElement<BindableObject> parent, IDomElement<BindableObject> logicalParent, ITreeNodeProvider<BindableObject> treeNodeProvider)
            : base(dependencyObject, parent, logicalParent, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
        {
            VisualTreeHelper.SubTreeAdded += VisualTreeHelper_SubTreeAdded; ;
            VisualTreeHelper.SubTreeRemoved += VisualTreeHelper_SubTreeRemoved;
        }

        private void VisualTreeHelper_SubTreeRemoved(object sender, EventArgs e)
        {
            ElementUnloaded(sender as BindableObject);
        }

        private void VisualTreeHelper_SubTreeAdded(object sender, EventArgs e)
        {
            ElementLoaded(sender as BindableObject);
        }

        public new void Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        private void UnregisterChildrenChangeHandler()
        {
            VisualTreeHelper.SubTreeAdded -= VisualTreeHelper_SubTreeAdded; ;
            VisualTreeHelper.SubTreeRemoved -= VisualTreeHelper_SubTreeRemoved;
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
            return new LazyDependencyPropertyDictionary<BindableProperty>(dependencyObject.GetType());
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
            return dependencyObject.GetHashCode();
        }
    }
}
