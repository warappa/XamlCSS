using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using XamlCSS.Dom;

namespace XamlCSS.XamarinForms.Dom
{
    public class TreeNodeProvider : TreeNodeProviderBase<BindableObject, Style, BindableProperty>
    {
        public TreeNodeProvider(IDependencyPropertyService<BindableObject, Style, BindableProperty> BindablePropertyService)
            : base(BindablePropertyService)
        {
        }

        public override IDomElement<BindableObject> CreateTreeNode(BindableObject BindableObject)
        {
            return new DomElement(BindableObject, GetDomElement(GetParent(BindableObject, SelectorType.VisualTree)), GetDomElement(GetParent(BindableObject, SelectorType.LogicalTree)), this);
        }

        public override IEnumerable<BindableObject> GetChildren(BindableObject element, SelectorType type)
        {
            var list = GetLogicalChildren(element as Element)
                .Cast<BindableObject>()
                .ToList();

            return list;
        }

        private static List<BindableObject> GetLogicalChildren(Element element)
        {
            var list = new List<BindableObject>();
            if(element is ItemsView<Cell>)
            {
                foreach(var child in Css.GetOverriddenChildren(element))
                {
                    list.Add(child);
                }
            }
            else if (element is ILayoutController lc)
            {
                foreach (var item in lc.Children)
                {
                    list.Add(item);
                }
            }
            else if (element is IPageController pc)
            {
                foreach (var item in pc.InternalChildren)
                {
                    list.Add(item);
                }
            }
            else if (element is Application a)
            {
                list.Add(a.MainPage);
            }
            else if (element is IElementController ec)
            {
                foreach (var item in ec.LogicalChildren)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null &&
                toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.GetTypeInfo().BaseType;
            }

            return false;
        }

        public override BindableObject GetParent(BindableObject element, SelectorType type)
        {
            var p = (element as Element)?.Parent;
            
            return p;
        }

        public override bool IsInTree(BindableObject dependencyObject, SelectorType type)
        {
            var p = GetParent(dependencyObject, type);

            if (p == null)
            {
                return dependencyObject is Application;
            }

            var children = GetChildren(p, SelectorType.LogicalTree);

            return children.Contains(dependencyObject);
        }
    }
}
