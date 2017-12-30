using System.Collections.Generic;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class UnivseralSelector : SelectorFragment
    {
        public UnivseralSelector(CssNodeType type, string text) : base(type, text)
        {
            
        }

        private void Initialize(StyleSheet styleSheet)
        {
            var namespaceSeparatorIndex = Text.IndexOf('|');
            string alias = "";
            string @namespace = null;
            if (namespaceSeparatorIndex > -1)
            {
                alias = Text.Substring(0, namespaceSeparatorIndex);
                if (alias != "*")
                {
                    @namespace = styleSheet.GetNamespaceUri(alias); //domElement.LookupNamespaceUri(@namespace);
                }
            }
            else
            {
                @namespace = styleSheet.GetNamespaceUri(""); //domElement.LookupNamespaceUri("");
            }

            this.Alias = alias;
            this.NamespaceUri = @namespace;
            this.initializedWith = styleSheet;
        }

        public override bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, List<SelectorFragment> fragments, ref int currentIndex)
        {
            if (initializedWith != styleSheet)
            {
                Initialize(styleSheet);
            }

            return domElement.NamespaceUri == NamespaceUri || NamespaceUri == "*";
        }

        public string Alias { get; private set; }
        public string NamespaceUri { get; private set; }

        private StyleSheet initializedWith;
    }

    public class TypeSelector : SelectorFragment
    {
        private StyleSheet initializedWith;

        public TypeSelector(CssNodeType type, string text) : base(type, text)
        {
            
        }

        private void Initialize(StyleSheet styleSheet)
        {
            var namespaceSeparatorIndex = Text.IndexOf('|');
            string @namespace = null;
            //string prefix = null;
            string alias = "";
            string localName = null;
            if (namespaceSeparatorIndex > -1)
            {
                alias = Text.Substring(0, namespaceSeparatorIndex);
                if (alias != "*")
                {
                    @namespace = styleSheet.GetNamespaceUri(alias);
                }
                else
                {

                }

                localName = Text.Substring(namespaceSeparatorIndex + 1);
            }
            else
            {
                @namespace = styleSheet.GetNamespaceUri("");
                localName = Text;
            }

            this.Alias = alias;
            this.LocalName = localName;
            this.NamespaceUri = @namespace;
            this.initializedWith = styleSheet;
        }

        public override bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, List<SelectorFragment> fragments, ref int currentIndex)
        {
            if (initializedWith != styleSheet)
            {
                Initialize(styleSheet);
            }

            return domElement.LocalName == LocalName && (Alias == "*" || domElement.NamespaceUri == NamespaceUri);
        }

        public string Alias { get; private set; }
        public string LocalName { get; private set; }
        public string NamespaceUri { get; private set; }
    }

    public class SelectorFragmentFactory
    {
        public SelectorFragment Create(CssNodeType type, string text)
        {
            if (type == CssNodeType.TypeSelector)
            {
                return new TypeSelector(type, text);
            }
            if (type == CssNodeType.UniversalSelector)
            {
                return new UnivseralSelector(type, text);
            }

            return new SelectorFragment(type, text);
        }
    }
    public class SelectorFragment
    {
        public SelectorFragment(CssNodeType type, string text)
        {
            Type = type;
            Text = text;
        }

        public CssNodeType Type { get; }
        public string Text { get; }

        virtual public bool Match<TDependencyObject>(StyleSheet styleSheet, ref IDomElement<TDependencyObject> domElement, List<SelectorFragment> fragments, ref int currentIndex)
        {
            /*if (Type == CssNodeType.TypeSelector)
            {
                var namespaceSeparatorIndex = Text.IndexOf('|');
                string @namespace = null;
                //string prefix = null;
                string localName = null;
                if (namespaceSeparatorIndex > -1)
                {
                    @namespace = Text.Substring(0, namespaceSeparatorIndex);
                    if (@namespace != "*")
                    {
                        @namespace = styleSheet.GetNamespaceUri(@namespace);
                    }

                    localName = Text.Substring(namespaceSeparatorIndex + 1);
                }
                else
                {
                    @namespace = styleSheet.GetNamespaceUri("");
                    localName = Text;
                }

                return domElement.NamespaceUri == @namespace && domElement.LocalName == localName;
            }
            else if (Type == CssNodeType.UniversalSelector)
            {
                var namespaceSeparatorIndex = Text.IndexOf('|');
                string @namespace = null;
                if (namespaceSeparatorIndex > -1)
                {
                    @namespace = Text.Substring(0, namespaceSeparatorIndex);
                    if (@namespace != "*")
                    {
                        @namespace = styleSheet.GetNamespaceUri(@namespace);
                    }
                }
                else
                {
                    @namespace = styleSheet.GetNamespaceUri("");
                }

                return domElement.NamespaceUri == @namespace || @namespace=="*";
            }
            else*/ if (Type == CssNodeType.IdSelector)
            {
                return domElement.Id == Text.Substring(1);
            }
            else if (Type == CssNodeType.ClassSelector)
            {
                return domElement.ClassList.Contains(Text.Substring(1));
            }

            else if (Type == CssNodeType.DirectSiblingCombinator)
            {
                var thisIndex = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex == 0)
                {
                    return false;
                }

                var sibling = domElement.Parent?.ChildNodes[thisIndex - 1];
                currentIndex--;

                var result = fragments[currentIndex].Match(styleSheet, ref sibling, fragments, ref currentIndex);
                domElement = sibling;

                return result;
            }

            else if (Type == CssNodeType.GeneralSiblingCombinator)
            {
                var thisIndex = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

                if (thisIndex == 0)
                {
                    return false;
                }

                currentIndex--;

                if (domElement.Parent?.ChildNodes.Any() == true)
                {
                    foreach (IDomElement<TDependencyObject> sibling in domElement.Parent?.ChildNodes.Take(thisIndex))
                    {
                        var refSibling = sibling;
                        if (fragments[currentIndex].Match(styleSheet, ref refSibling, fragments, ref currentIndex))
                        {
                            domElement = sibling;
                            return true;
                        }
                    }
                }

                return false;
            }

            else if (Type == CssNodeType.DirectDescendantCombinator)
            {
                var result = domElement.Parent?.ChildNodes.Contains(domElement) == true;
                domElement = domElement.Parent;
                return result;
            }

            else if (Type == CssNodeType.GeneralDescendantCombinator)
            {
                currentIndex--;
                var fragment = fragments[currentIndex];

                var current = domElement.Parent;
                while (current != null)
                {
                    if (fragment.Match(styleSheet, ref current, fragments, ref currentIndex))
                    {
                        domElement = current;
                        return true;
                    }
                    current = current.Parent;
                }
                return false;
            }

            else if (Type == CssNodeType.PseudoSelector)
            {
                if (Text == ":first-child")
                {
                    return (domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1) == 0;
                }
                else if (Text == ":last-child")
                {
                    return domElement.Parent?.ChildNodes.IndexOf(domElement) == (domElement.Parent?.ChildNodes.Count()) - 1;
                }
                else if (Text.StartsWith(":nth-child"))
                {
                    var expression = Text.Substring(11).Replace(")", "");
                    if (int.TryParse(expression, out int i))
                    {
                        var tagname = domElement.TagName;
                        var thisIndex = domElement.Parent?.ChildNodes.IndexOf(domElement) ?? -1;

                        return i == thisIndex + 1;
                    }
                }
                else if (Text.StartsWith(":nth-of-type"))
                {
                    var expression = Text.Substring(13).Replace(")", "");
                    if (int.TryParse(expression, out int i))
                    {
                        var tagname = domElement.TagName;
                        var thisIndex = domElement.Parent?.ChildNodes.Where(x => x.TagName == tagname).IndexOf(domElement) ?? -1;

                        return i == thisIndex + 1;
                    }
                }
                return false;

            }



            return false;
        }
    }
}