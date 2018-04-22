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
        }

        private void RegisterChildrenChangeHandler()
        {
            LoadedDetectionHelper.SubTreeAdded += ElementAdded;
            LoadedDetectionHelper.SubTreeRemoved += ElementRemoved;
        }

        public new void Dispose()
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
