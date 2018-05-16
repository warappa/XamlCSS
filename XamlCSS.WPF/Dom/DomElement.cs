using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    [DebuggerDisplay("{IsInLogicalTree ? \"L\" : \" \"}{IsInVisualTree ? \"V\" : \" \"} {Element.GetType().Name} {(Element as System.Windows.Controls.TextBlock)?.Text} Id={Id} Class={string.Join(\", \", this.ClassList)}")]
    public class DomElement : DomElementBase<DependencyObject, DependencyProperty>
    {
        private IDictionary<DependencyProperty, Action<object, EventArgs>> watchers = new Dictionary<DependencyProperty, Action<object, EventArgs>>();

        public DomElement(DependencyObject dependencyObject, IDomElement<DependencyObject, DependencyProperty> parent, IDomElement<DependencyObject, DependencyProperty> logicalParent, ITreeNodeProvider<DependencyObject, DependencyProperty> treeNodeProvider)
            : base(dependencyObject, parent, logicalParent, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();

            ApplyStyleImmediately = Css.GetApplyStyleImmediately(dependencyObject);
        }

        private void RegisterChildrenChangeHandler()
        {
            if (dependencyObject is FrameworkElement f)
            {
                f.Loaded += DomElement_Loaded;
                f.Unloaded += DomElement_Unloaded;
            }
            if (dependencyObject is FrameworkContentElement fc)
            {
                fc.Loaded += DomElement_Loaded;
                fc.Unloaded += DomElement_Unloaded;
            }
        }

        private void DomElement_Unloaded(object sender, RoutedEventArgs e)
        {
            ElementUnloaded(sender as DependencyObject);
        }

        private void DomElement_Loaded(object sender, RoutedEventArgs e)
        {
            ElementLoaded(sender);
        }

        public new void Dispose()
        {
            UnregisterChildrenChangeHandler();

            base.Dispose();
        }

        private void UnregisterChildrenChangeHandler()
        {
            if (dependencyObject is FrameworkElement f)
            {
                f.Loaded -= DomElement_Loaded;
                f.Unloaded -= DomElement_Unloaded;
            }
            if (dependencyObject is FrameworkContentElement fc)
            {
                fc.Loaded -= DomElement_Loaded;
                fc.Unloaded -= DomElement_Unloaded;
            }
        }

        public override void EnsureAttributeWatcher(DependencyProperty dependencyProperty)
        {
            if (!watchers.ContainsKey(dependencyProperty))
            {
                DependencyPropertyDescriptor.FromProperty(dependencyProperty, dependencyObject.GetType()).AddValueChanged(dependencyObject, PropertyUpdated);
                watchers[dependencyProperty] = PropertyUpdated;
            }
        }

        public override void ClearAttributeWatcher()
        {
            foreach (var item in watchers)
            {
                DependencyPropertyDescriptor.FromProperty(item.Key, dependencyObject.GetType()).RemoveValueChanged(dependencyObject, PropertyUpdated);
            }

            watchers.Clear();
        }

        private void PropertyUpdated(object sender, EventArgs e)
        {
            Css.instance?.UpdateElement(dependencyObject);
        }

        public override object GetAttributeValue(DependencyProperty dependencyProperty)
        {
            return dependencyObject.GetValue(dependencyProperty);
        }

        protected override HashSet<string> GetClassList(DependencyObject dependencyObject)
        {
            var list = new HashSet<string>();

            var classNames = Css.GetClass(dependencyObject)?.Split(classSplitter, StringSplitOptions.RemoveEmptyEntries);
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
            if (dependencyObject is FrameworkElement)
            {
                return dependencyObject.ReadLocalValue(FrameworkElement.NameProperty) as string;
            }

            return dependencyObject.ReadLocalValue(FrameworkContentElement.NameProperty) as string;
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

        protected override IDictionary<string, DependencyProperty> CreateNamedNodeMap(DependencyObject dependencyObject)
        {
            return new LazyDependencyPropertyDictionary<DependencyProperty>(dependencyObject.GetType());
        }

        public override bool ApplyStyleImmediately { get; }

        public override void UpdateIsReady()
        {
            if (dependencyObject is ApplicationDependencyObject)
            {
                IsReady = true;
            }
            else if (dependencyObject is FrameworkElement f)
            {
                IsReady = f.IsLoaded;
            }
            else if (dependencyObject is FrameworkContentElement fc)
            {
                IsReady = fc.IsLoaded;
            }
            else
            {
                IsReady = true;
            }
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

        public override int GetHashCode()
        {
            return dependencyObject.GetHashCode();
        }
    }
}
