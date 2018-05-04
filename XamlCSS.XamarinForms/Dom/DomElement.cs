using System.Collections.Generic;
using XamlCSS.Dom;
using Xamarin.Forms;
using System;
using System.Diagnostics;
using System.ComponentModel;

namespace XamlCSS.XamarinForms.Dom
{
    [DebuggerDisplay("{dependencyObject.GetType().Name} Id={Id} Class={string.Join(\", \", ClassList)}")]
    public class DomElement : DomElementBase<BindableObject, BindableProperty>, IDisposable
    {
        private IDictionary<string, bool> watchers = new Dictionary<string, bool>();

        public DomElement(BindableObject dependencyObject, IDomElement<BindableObject, BindableProperty> parent, IDomElement<BindableObject, BindableProperty> logicalParent, ITreeNodeProvider<BindableObject, BindableProperty> treeNodeProvider)
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

        public override void UpdateIsReady()
        {
            IsReady = true;
        }

        public override void EnsureAttributeWatcher(BindableProperty dependencyProperty)
        {
            if (!watchers.ContainsKey(dependencyProperty.PropertyName))
            {
                if (watchers.Count == 0)
                {
                    dependencyObject.PropertyChanged += DependencyObject_PropertyChanged;
                }

                watchers.Add(dependencyProperty.PropertyName, true);
            }
        }

        public override void ClearAttributeWatcher()
        {
            dependencyObject.PropertyChanged -= DependencyObject_PropertyChanged;

            watchers.Clear();
        }

        private void DependencyObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (watchers.ContainsKey(e.PropertyName))
            {
                Css.instance?.UpdateElement(dependencyObject);
            }
        }

        public override object GetAttributeValue(BindableProperty dependencyProperty)
        {
            return dependencyObject.GetValue(dependencyProperty);
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
