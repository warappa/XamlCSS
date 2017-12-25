using System.Collections.Generic;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    public class SelectorFragment
    {
        public SelectorFragment(CssNodeType type, string text)
        {
            Type = type;
            Text = text;
        }

        public CssNodeType Type { get; }
        public string Text { get; }

        public bool Match<TDependencyObject>(ref IDomElement<TDependencyObject> domElement, List<SelectorFragment> fragments, ref int currentIndex)
        {
            if (Type == CssNodeType.TypeSelector)
            {
                var namespaceSeparatorIndex = Text.IndexOf('|');
                string @namespace = null;
                string localName = null;
                if (namespaceSeparatorIndex > -1)
                {
                    @namespace = Text.Substring(0, namespaceSeparatorIndex);
                    if (@namespace != "*")
                    {
                        @namespace = domElement.LookupNamespaceUri(@namespace);
                    }
                    localName = Text.Substring(namespaceSeparatorIndex + 1);
                }
                else
                {
                    @namespace = domElement.LookupNamespaceUri("");
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
                        @namespace = domElement.LookupNamespaceUri(@namespace);
                    }
                }
                else
                {
                    @namespace = domElement.LookupNamespaceUri("");
                }

                return domElement.NamespaceUri == @namespace || @namespace=="*";
            }
            else if (Type == CssNodeType.IdSelector)
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

                var result = fragments[currentIndex].Match(ref sibling, fragments, ref currentIndex);
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
                        if (fragments[currentIndex].Match(ref refSibling, fragments, ref currentIndex))
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
                    if (fragment.Match(ref current, fragments, ref currentIndex))
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