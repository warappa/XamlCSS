using System.Collections.Generic;
using XamlCSS.Dom;
using Xamarin.Forms;
using System;
using System.Diagnostics;

namespace XamlCSS.XamarinForms.Dom
{
    [DebuggerDisplay("{dependencyObject.GetType().Name} Id={Id} Class={string.Join(\", \", ClassList)}")]
    public class DomElement : DomElementBase<BindableObject, BindableProperty>, IDisposable
    {
        public DomElement(BindableObject dependencyObject, IDomElement<BindableObject> parent, IDomElement<BindableObject> logicalParent, ITreeNodeProvider<BindableObject> treeNodeProvider)
            : base(dependencyObject, parent, logicalParent, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
        {
        }

        private void UnregisterChildrenChangeHandler()
        {
        }

        public void ElementLoaded()
        {
            ElementLoaded(dependencyObject);
        }

        public void ElementUnloaded()
        {
            ElementUnloaded(dependencyObject);
        }

        public void ReloadChildren()
        {
            this.childNodes = null;
            this.logicalChildNodes = null;
        }

        public new void Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
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
            // return (dependencyObject as Element).Id.ToString();
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
