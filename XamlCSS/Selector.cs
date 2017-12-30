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
    public class Selector : ISelector
    {
        public Selector()
        {

        }

        public Selector(string selectorString)
            : this(selectorString, null)
        {

        }

        internal Selector(string selectorString, CssNode selectorAst)
        {
            if (selectorAst != null)
            {
                this.selectorAst = selectorAst;
                this.val = selectorString;
                CalculateSpecificity(selectorString);
            }
            else
            {
                this.Value = selectorString;
            }
        }

        protected string val;
        private CssNode selectorAst;

        public string Value
        {
            get
            {
                return val;
            }
            set
            {
                val = value;

                CalculateSpecificity(value);

                GetFragments(val);
            }
        }

        private void CalculateSpecificity(string value)
        {
            var charArray = value.ToCharArray();

            IdSpecificity = charArray.Count(x => x == '#');
            ClassSpecificity = charArray.Count(x => x == '.' || x == ':' || x == '[');

            var simpleSpecifitySplit = value.Split(new[] { ' ', '>', '*' }, StringSplitOptions.RemoveEmptyEntries);
            SimpleSpecificity = simpleSpecifitySplit.Count(x => !x.StartsWith(".") && !x.StartsWith("#"));
        }

        private void GetFragments(string val)
        {
            var tokenizer = new Tokenizer();
            var selectorTokens = Tokenizer.Tokenize(val);
            var a = new AstGenerator();
            var ast = a.GetAst(selectorTokens);

            selectorAst = ast.Root.Children.First().Children.First();

            SetFragmentsFromAst();
        }

        private void SetFragmentsFromAst()
        {
            Fragments = selectorAst.Children.SelectMany(x =>
            {
                if (x.Type == CssNodeType.DirectDescendantCombinator ||
                    x.Type == CssNodeType.GeneralDescendantCombinator ||
                    x.Type == CssNodeType.DirectSiblingCombinator ||
                    x.Type == CssNodeType.GeneralSiblingCombinator)
                {
                    //return new[] { new SelectorFragment(x.Type, x.Text) };
                    return new[] { new SelectorFragmentFactory().Create(x.Type, x.Text) };
                }
                //return x.Children.Select(y => new SelectorFragment(y.Type, y.Text));
                return x.Children.Select(y => new SelectorFragmentFactory().Create(y.Type, y.Text));
            })
            .ToArray();
        }

        public int SimpleSpecificity { get; private set; }
        public int ClassSpecificity { get; private set; }
        public int IdSpecificity { get; private set; }
        public SelectorFragment[] Fragments { get; private set; }

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

        public bool Match<TDependencyObject>(StyleSheet styleSheet, IDomElement<TDependencyObject> domElement)
            where TDependencyObject : class
        {
            for (var i = Fragments.Length - 1; i >= 0; i--)
            {
                var fragment = Fragments[i];

                if (!fragment.Match(styleSheet, ref domElement, Fragments, ref i))
                {
                    return false;
                }
            }

            return true;
        }
    }
}