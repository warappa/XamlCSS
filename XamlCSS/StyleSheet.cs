using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS
{
    public class StyleSheet : INotifyPropertyChanged
    {
        public static readonly StyleSheet Empty = new StyleSheet();

        public StyleSheet()
        {
            Id = Guid.NewGuid().ToString();
        }

        public static Func<object, object> GetParent { get; internal set; }
        public static Func<object, StyleSheet> GetStyleSheet { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected CssNamespaceCollection combinedNamespaces;
        protected StyleRuleCollection combinedRules;
        protected CssNamespaceCollection localNamespaces = new CssNamespaceCollection();
        protected StyleRuleCollection localRules = new StyleRuleCollection();
        protected StyleSheetCollection baseStyleSheets = null;
        protected bool inheritStyleSheets = true;
        protected StyleSheetCollection inheritedStyleSheets = null;
        protected string content = null;
        protected object attachedTo;

        virtual public CssNamespaceCollection LocalNamespaces
        {
            get
            {
                return localNamespaces;
            }
            set
            {
                localNamespaces = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalNamespaces"));
            }
        }

        virtual public StyleRuleCollection LocalRules
        {
            get
            {
                return localRules;
            }
            set
            {
                localRules = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalRules"));
            }
        }

        virtual public IDictionary<string, string> Variables
        {
            get
            {
                return variables ?? (variables = new Dictionary<string, string>());
            }
            set
            {
                variables = value;

                //Invalidate();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Variables"));
            }
        }

        virtual public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;

                Invalidate();
            }
        }

        private void Invalidate()
        {
            Reset();

            var sheet = CssParser.Parse(content, null, GetCombinedVariables());

            foreach (var error in sheet.Errors)
            {
                this.Errors.Add(error);
            }
            foreach (var warning in sheet.Warnings)
            {
                this.Warnings.Add(warning);
            }

            this.localNamespaces = sheet.LocalNamespaces;
            this.localRules = sheet.LocalRules;
            this.variables = sheet.Variables;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Content"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Errors"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Warnings"));
        }

        private ObservableCollection<string> errors = new ObservableCollection<string>();
        virtual public ObservableCollection<string> Errors
        {
            get
            {
                return errors;
            }
            set
            {
                errors = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Errors"));
            }
        }

        private ObservableCollection<string> warnings = new ObservableCollection<string>();
        private IDictionary<string, string> variables;

        virtual public ObservableCollection<string> Warnings
        {
            get
            {
                return warnings;
            }
            set
            {
                warnings = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Warnings"));
            }
        }

        public object AttachedTo
        {
            get
            {
                return attachedTo;
            }
            set
            {
                attachedTo = value;

                inheritedStyleSheets = null;

                Reset();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AttachedTo"));
            }
        }

        public bool InheritStyleSheets
        {
            get { return inheritStyleSheets; }
            set
            {
                inheritStyleSheets = value;

                inheritedStyleSheets = null;

                Reset();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InheritStyleSheets"));
            }
        }

        public CssNamespaceCollection Namespaces
        {
            get
            {
                return combinedNamespaces ?? (combinedNamespaces = GetCombinedNamespaces());
            }
        }

        public StyleRuleCollection Rules
        {
            get
            {
                return combinedRules ?? (combinedRules = new StyleRuleCollection(GetCombinedStyleRules()));
            }
        }

        public StyleSheetCollection BaseStyleSheets
        {
            get
            {
                return baseStyleSheets ?? (baseStyleSheets = new StyleSheetCollection());
            }

            set
            {
                foreach (var added in BaseStyleSheets)
                {
                    added.PropertyChanged -= BaseStyleSheet_PropertyChanged;
                }
                BaseStyleSheets.CollectionChanged -= BaseStyleSheets_CollectionChanged;

                baseStyleSheets = value;

                Reset();

                foreach (var added in BaseStyleSheets)
                {
                    added.PropertyChanged += BaseStyleSheet_PropertyChanged;
                }

                BaseStyleSheets.CollectionChanged += BaseStyleSheets_CollectionChanged;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BaseStyleSheets"));
            }
        }

        private void BaseStyleSheets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Invalidate();
        }

        private void BaseStyleSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Content) ||
                e.PropertyName == nameof(LocalRules))
            {
                Invalidate();
            }
        }

        public StyleSheetCollection InheritedStyleSheets
        {
            get
            {
                return inheritedStyleSheets ?? (inheritedStyleSheets = InheritStyleSheets ? new StyleSheetCollection(GetParentStyleSheets(AttachedTo).Reverse<StyleSheet>()) : new StyleSheetCollection());
            }
        }

        public string Id { get; protected set; }

        protected StyleSheetCollection GetParentStyleSheets(object from)
        {
            StyleSheetCollection styleSheets = new StyleSheetCollection();

            if (from == null)
            {
                return styleSheets;
            }

            var current = GetParent(from);
            while (current != null)
            {
                var styleSheet = GetStyleSheet(current);
                if (styleSheet != null)
                {
                    styleSheets.Add(styleSheet);
                }
                current = GetParent(current);
            }

            return styleSheets;
        }

        protected CssNamespaceCollection GetCombinedNamespaces()
        {
            if (BaseStyleSheets?.Count == 0 &&
                InheritedStyleSheets?.Count == 0)
            {
                return LocalNamespaces;
            }

            return new CssNamespaceCollection(
                InheritedStyleSheets
                .Select(x => x.Namespaces)
                .Concat(BaseStyleSheets.Select(x => x.Namespaces))
                .Aggregate((a, b) => new CssNamespaceCollection(a.Concat(b)))
                .Concat(LocalNamespaces)
                .GroupBy(x => x.Alias)
                .Select(x => x.Last()));
        }

        protected IDictionary<string, string> GetCombinedVariables()
        {
            if (BaseStyleSheets?.Count == 0 &&
                InheritedStyleSheets?.Count == 0)
            {
                return Variables;
            }

            return InheritedStyleSheets
                .SelectMany(x => x.Variables.ToList())
                .Concat(BaseStyleSheets.SelectMany(x => x.Variables.ToList()))
                .Concat(Variables)
                .GroupBy(x => x.Key)
                .Select(x => x.Last())
                .ToDictionary(x => x.Key, x => x.Value);
        }

        protected List<StyleRule> GetCombinedStyleRules()
        {
            if (BaseStyleSheets?.Count == 0 &&
                InheritedStyleSheets?.Count == 0)
            {
                return LocalRules;
            }

            return InheritedStyleSheets
                    .SelectMany(x => x.Rules.ToList())
                    .Concat(BaseStyleSheets.SelectMany(x => x.Rules.ToList()))
                    .Concat(LocalRules)
                    .GroupBy(x => x.SelectorString)
                    .Select(x => new StyleRule
                    {
                        Selectors = x.First().Selectors,
                        SelectorType = x.First().SelectorType,
                        DeclarationBlock = new StyleDeclarationBlock(GetMergedStyleDeclarations(x.ToList()), x.SelectMany(y => y.DeclarationBlock.Triggers).ToList())
                    })
                    .ToList();
        }

        protected List<StyleDeclaration> GetMergedStyleDeclarations(List<StyleRule> styleRules)
        {
            return styleRules
                .SelectMany(x => x.DeclarationBlock.ToList())
                .GroupBy(x => x.Property)
                .Select(x => x.Last())
                .ToList();
        }

        protected void Reset()
        {
            combinedRules = null;
            combinedNamespaces = null;
            variables = null;

            this.Errors.Clear();
            this.Warnings.Clear();
        }

        internal void AddError(string error)
        {
            Errors.Add(error);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Errors"));
        }
    }
}
