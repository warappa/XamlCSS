using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS
{
    [DebuggerDisplay("{Value}")]
    public class Selector
    {
        public Selector()
        {

        }

        public Selector(string selectorString)
        {
            this.Value = selectorString;
        }

        protected string val;
        public string Value
        {
            get
            {
                return val;
            }
            set
            {
                val = value;

                var charArray = value.ToCharArray();

                IdSpecificity = charArray.Count(x => x == '#');
                ClassSpecificity = charArray.Count(x => x == '.' || x == ':' || x == '[');

                var simpleSpecifitySplit = value.Split(new[] { ' ', '>', '*' }, StringSplitOptions.RemoveEmptyEntries);
                SimpleSpecificity = simpleSpecifitySplit.Count(x => !x.StartsWith(".") && !x.StartsWith("#"));


                GetFragments(val);
            }
        }

        private void GetFragments(string val)
        {
            bool isInString = false;


            var tokenizer = new Tokenizer();
            var selectorTokens = Tokenizer.Tokenize(val);
            var a = new AstGenerator();
            var ast = a.GetAst(selectorTokens);
            var selectorAst = ast.Root.Children.First().Children.First();

            Fragments = selectorAst.Children.SelectMany(x =>
            {
                if (x.Type == CssNodeType.DirectDescendantCombinator ||
                    x.Type == CssNodeType.GeneralDescendantCombinator ||
                    x.Type == CssNodeType.DirectSiblingCombinator ||
                    x.Type == CssNodeType.GeneralSiblingCombinator)
                {
                    return new[] { new SelectorFragment(x.Type, x.Text) };
                }
                return x.Children.Select(y => new SelectorFragment(y.Type, y.Text));
            })
            .ToList();
        }

        public int SimpleSpecificity { get; private set; }
        public int ClassSpecificity { get; private set; }
        public int IdSpecificity { get; private set; }
        public List<SelectorFragment> Fragments { get; private set; }

        public string Specificity
        {
            get
            {
                if (IdSpecificity > 0)
                {
                    return $"{IdSpecificity},{ClassSpecificity},{SimpleSpecificity}";
                }
                else if (ClassSpecificity > 0)
                {
                    return $"{ClassSpecificity},{SimpleSpecificity}";
                }
                else
                {
                    return $"{SimpleSpecificity}";
                }
            }
        }

        public static bool operator <(Selector e1, Selector e2)
        {
            if (e1.IdSpecificity < e2.IdSpecificity)
                return true;
            if (e1.ClassSpecificity < e2.ClassSpecificity)
                return true;
            if (e1.SimpleSpecificity < e2.SimpleSpecificity)
                return true;

            return false;
        }

        public static bool operator >(Selector e1, Selector e2)
        {
            if (e1.IdSpecificity > e2.IdSpecificity)
                return true;
            if (e1.ClassSpecificity > e2.ClassSpecificity)
                return true;
            if (e1.SimpleSpecificity > e2.SimpleSpecificity)
                return true;

            return false;
        }

        public bool Match<TDependencyObject>(IDomElement<TDependencyObject> domElement)
        {
            for (var i = Fragments.Count - 1; i >= 0; i--)
            {
                var fragment = Fragments[i];

                if (!fragment.Match(ref domElement, Fragments, ref i))
                {
                    return false;
                }
            }

            return true;
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

        public bool Match<TDependencyObject>(ref IDomElement<TDependencyObject> domElement, List<SelectorFragment> fragments, ref int currentIndex)
        {
            if (Type == CssNodeType.TypeSelector)
            {
                return domElement.TagName == Text;
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
                var thisIndex = domElement.ParentElement.Children.IndexOf(domElement);

                if (thisIndex == 0)
                {
                    return false;
                }

                var sibling = (IDomElement<TDependencyObject>)domElement.ParentElement.ChildNodes[thisIndex - 1];
                currentIndex--;

                var result = fragments[currentIndex].Match(ref sibling, fragments, ref currentIndex);
                domElement = sibling;

                return result;
            }

            else if (Type == CssNodeType.GeneralSiblingCombinator)
            {
                var thisIndex = domElement.ParentElement.Children.IndexOf(domElement);

                if (thisIndex == 0)
                {
                    return false;
                }

                currentIndex--;

                foreach (IDomElement<TDependencyObject> sibling in domElement.ParentElement.ChildNodes.Take(thisIndex))
                {
                    var refSibling = sibling;
                    if (fragments[currentIndex].Match(ref refSibling, fragments, ref currentIndex))
                    {
                        domElement = sibling;
                        return true;
                    }
                }
                return false;

            }

            else if (Type == CssNodeType.DirectDescendantCombinator)
            {
                currentIndex--;
                var result = domElement.ParentElement.Children.Contains(domElement) == true;
                domElement = (IDomElement<TDependencyObject>)domElement.ParentElement;
                return result;
            }

            else if (Type == CssNodeType.GeneralDescendantCombinator)
            {
                currentIndex--;
                var fragment = fragments[currentIndex];

                var current = (IDomElement<TDependencyObject>)domElement.ParentElement;
                while (current != null)
                {
                    if (fragment.Match(ref current, fragments, ref currentIndex))
                    {
                        domElement = current;
                        return true;
                    }
                    current = (IDomElement<TDependencyObject>)current.ParentElement;
                }
                return false;
            }

            else if (Type == CssNodeType.PseudoSelector)
            {
                if (Text == ":first-child")
                {
                    return (domElement.ParentElement?.Children.IndexOf(domElement) ?? -1) == 0;
                }
                else if (Text == ":last-child")
                {
                    return domElement.ParentElement?.Children.IndexOf(domElement) == (domElement.ParentElement?.Children.Count()) - 1;
                }
                return false;
            }
            return false;
        }
    }
}