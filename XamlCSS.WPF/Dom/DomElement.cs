using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using XamlCSS.Dom;

namespace XamlCSS.WPF.Dom
{
    [DebuggerDisplay("{IsInLogicalTree ? \"L\" : \" \"}{IsInVisualTree ? \"V\" : \" \"} {Element.GetType().Name} {(Element as System.Windows.Controls.TextBlock)?.Text} Id={Id} Class={string.Join(\", \", this.ClassList)}")]
    public class DomElement : DomElementBase<DependencyObject, DependencyProperty>
    {
        public DomElement(DependencyObject dependencyObject, IDomElement<DependencyObject> parent, IDomElement<DependencyObject> logicalParent, ITreeNodeProvider<DependencyObject> treeNodeProvider)
            : base(dependencyObject, parent, logicalParent, treeNodeProvider)
        {
            RegisterChildrenChangeHandler();

            AddIfNotAdded();
        }

        internal void AddIfNotAdded()
        {
            if (IsInLogicalTree &&
                logicalParent != null)
            {
                if ((logicalParent as DomElement).logicalChildNodes?.Contains(this) == false)
                    (logicalParent as DomElement).logicalChildNodes?.Add(this);
            }

            if (IsInVisualTree &&
                parent != null)
            {
                if ((parent as DomElement).childNodes?.Contains(this) == false)
                    (parent as DomElement).childNodes?.Add(this);
            }
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
            if (IsInLogicalTree &&
                logicalParent != null)
            {
                if (((DomElement)logicalParent)?.logicalChildNodes?.Remove(this) == false)
                {
                    // should not happen - bug
                }
            }

            if (IsInVisualTree &&
                parent != null)
            {
                if (((DomElement)parent)?.childNodes?.Remove(this) == false)
                {
                    // should not happen - bug
                }
            }

            // got parent
            ReevaluateParent();
        }

        private void DomElement_Loaded(object sender, RoutedEventArgs e)
        {
            // got parent
            ReevaluateParent();

            AddIfNotAdded();

            if (IsInLogicalTree)
            {
                if (((DomElement)logicalParent)?.logicalChildNodes?.Contains(this) == false)
                {
                    // should not happen - bug
                }
            }

            if (IsInVisualTree)
            {
                if (((DomElement)parent)?.childNodes?.Contains(this) == false)
                {
                    // should not happen - bug
                }
            }
        }

        private void AddIfNotIn(IList<IDomElement<DependencyObject>> domElements)
        {
            if (!domElements.Contains(this))
            {
                domElements.Add(this);
            }
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
