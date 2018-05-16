using System.Collections.Generic;
using XamlCSS.Dom;
using Windows.UI.Xaml;
using System;
using System.Diagnostics;
using System.Linq;

namespace XamlCSS.UWP.Dom
{
    [DebuggerDisplay("Id={Id} Class={Class}")]
    public class DomElement : DomElementBase<DependencyObject, DependencyProperty>, IDisposable
    {
        private IDictionary<DependencyProperty, long> watchers = new Dictionary<DependencyProperty, long>();

        public DomElement(DependencyObject dependencyObject, IDomElement<DependencyObject, DependencyProperty> parent, IDomElement<DependencyObject, DependencyProperty> logicalParent, ITreeNodeProvider<DependencyObject, DependencyProperty> treeNodeProvider)
            : base(dependencyObject, parent, logicalParent, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();
        }

        private void RegisterChildrenChangeHandler()
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

        public override void EnsureAttributeWatcher(DependencyProperty dependencyProperty)
        {
            if (!watchers.ContainsKey(dependencyProperty))
            {
                var token = dependencyObject.RegisterPropertyChangedCallback(dependencyProperty, DependencyPropertyChangedCallback);

                watchers.Add(dependencyProperty, token);
            }
        }

        public override void ClearAttributeWatcher()
        {
            foreach (var item in watchers)
            {
                dependencyObject.UnregisterPropertyChangedCallback(item.Key, item.Value);
            }

            watchers.Clear();
        }

        private void DependencyPropertyChangedCallback(DependencyObject sender, DependencyProperty dependencyProperty)
        {
            Css.instance?.UpdateElement(dependencyObject);
        }

        public override object GetAttributeValue(DependencyProperty dependencyProperty)
        {
            return dependencyObject.GetValue(dependencyProperty);
        }

        void IDisposable.Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        public override void UpdateIsReady()
        {
            IsReady = true;
        }

        private void UnregisterChildrenChangeHandler()
        {
        }

        protected override HashSet<string> GetClassList(DependencyObject dependencyObject)
        {
            var list = new HashSet<string>();
            var classNames = Css.GetClass(dependencyObject)?.Split(' ');

            if (classNames?.Length > 0)
            {
                foreach (var classname in classNames.Distinct())
                {
                    list.Add(classname);
                }
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
