using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        internal Selector(IEnumerable<SelectorMatcher> fragments)
        {
            this.selectorMatchers = fragments.ToArray();
            this.selectorMatchersLength = this.selectorMatchers.Length;

            SetGroups();
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
            var result = SpecificityCalculator.Calculate(value);

            IdSpecificity = result.IdSpecificity;
            ClassSpecificity = result.ClassSpecificity;
            SimpleSpecificity = result.SimpleSpecificity;
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
            selectorMatchers = selectorAst.Children.SelectMany(x =>
            {
                if (x.Type == CssNodeType.DirectDescendantCombinator ||
                    x.Type == CssNodeType.GeneralDescendantCombinator ||
                    x.Type == CssNodeType.DirectSiblingCombinator ||
                    x.Type == CssNodeType.GeneralSiblingCombinator)
                {
                    return new[] { new SelectorMatcherFactory().Create(x.Type, x.Text) };
                }
                return x.Children.Select(y => new SelectorMatcherFactory().Create(y.Type, y.Text));
            })
            .ToArray();

            selectorMatchersLength = selectorMatchers.Length;
            SetGroups();
        }

        private void SetGroups()
        {
            var groupStartIndexes = new List<int>() { 0 };
            var groupEndIndexes = new List<int>();

            for (var i = 0; i < selectorMatchersLength; i++)
            {
                var selectorMatcher = selectorMatchers.ElementAt(i);
                if (selectorMatcher.Type == CssNodeType.GeneralDescendantCombinator)
                {
                    groupEndIndexes.Add(i - 1);
                    groupStartIndexes.Add(i);
                }
                else
                {
                }
            }

            if (groupEndIndexes.LastOrDefault() != selectorMatchersLength)
            {
                groupEndIndexes.Add(selectorMatchersLength - 1);
            }

            selectorMatcherGroupStartIndices = groupStartIndexes.ToArray();
            selectorMatcherGroupEndIndices = groupEndIndexes.ToArray();
            GroupCount = groupStartIndexes.Count;
        }

        public int SimpleSpecificity { get; private set; }
        public int ClassSpecificity { get; private set; }
        public int IdSpecificity { get; private set; }

        internal SelectorMatcher[] selectorMatchers;
        private int[] selectorMatcherGroupStartIndices;
        private int[] selectorMatcherGroupEndIndices;

        public int GroupCount { get; private set; }

        private int selectorMatchersLength;

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

        public MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, IDomElement<TDependencyObject, TDependencyProperty> domElement)
            where TDependencyObject : class
        {
            return Match(styleSheet, domElement, -1, 0);
        }

        public MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, IDomElement<TDependencyObject, TDependencyProperty> domElement, int startGroupIndex, int endGroupIndex)
            where TDependencyObject : class
        {
            if (startGroupIndex == -1)
            {
                startGroupIndex = GroupCount - 1;
            }

            var currentGroupIndex = startGroupIndex;

            var startOfIteration = selectorMatcherGroupEndIndices[startGroupIndex];
            var endOfIteration = selectorMatcherGroupStartIndices[endGroupIndex];
            
            for (var i = startOfIteration; i >= endOfIteration; i--)
            {
                var selectorMatcher = selectorMatchers[i];
                if (i < selectorMatcherGroupStartIndices[currentGroupIndex])
                {
                    currentGroupIndex--;
                }

                var match = selectorMatcher.Match(styleSheet, ref domElement, selectorMatchers, ref i);
                if (!match.IsSuccess)
                {
                    match.Group = currentGroupIndex;
                    return match;
                }
            }

            return MatchResult.Success;
        }

        //public MatchResult Match<TDependencyObject, TDependencyProperty>(StyleSheet styleSheet, IDomElement<TDependencyObject, TDependencyProperty> domElement)
        //    where TDependencyObject : class
        //{
        //    for (var i = selectorMatchersLength - 1; i >= 0; i--)
        //    {
        //        var selectorMatcher = selectorMatchers[i];

        //        var match = selectorMatcher.Match(styleSheet, ref domElement, selectorMatchers, ref i);
        //        if (!match.IsSuccess)
        //        {
        //            return match;
        //        }
        //    }

        //    return MatchResult.Success;
        //}

        public bool StartOnVisualTree()
        {
            if (selectorMatchers[selectorMatchersLength - 1].Type == CssNodeType.PseudoSelector &&
                selectorMatchers[selectorMatchersLength - 1].Text == ":visualtree")
            {
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Selector;
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            return other.Value == this.Value;
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
