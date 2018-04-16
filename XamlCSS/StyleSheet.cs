using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using XamlCSS.CssParsing;
using XamlCSS.Dom;
using XamlCSS.Utils;

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

        public int Version { get; private set; }

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

        public StyleSheet SingleBaseStyleSheet
        {
            get
            {
                return BaseStyleSheets.FirstOrDefault();
            }
            set
            {
                UninitializeBaseStyleSheet(BaseStyleSheets);
                BaseStyleSheets.Clear();
                BaseStyleSheets.Add(value);
                InitializeBaseStyleSheet(BaseStyleSheets);
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
            Version++;
            Reset();

            var sheet = CssParser.Parse(content ?? "", BaseStyleSheets.LastOrDefault()?.GetNamespaceUri("", "Button"), GetCombinedVariables());

            foreach (var error in sheet.Errors)
            {
                this.LocalErrors.Add(error);
            }
            foreach (var warning in sheet.Warnings)
            {
                this.LocalWarnings.Add(warning);
            }

            this.localNamespaces = sheet.LocalNamespaces;
            this.localRules = sheet.LocalRules;
            this.variables = sheet.Variables;

            EagerLoading();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Content"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalRules"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalNamespaces"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Errors"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Warnings"));
        }

        private void EagerLoading()
        {
            object a = Namespaces;
            a = Rules;
            a = Errors;
            a = Warnings;

            foreach(var rule in Rules)
            {
                CachedSelectorProvider.Instance.GetOrAdd(rule.SelectorString);
            }
        }

        private ObservableCollection<string> errors = new ObservableCollection<string>();
        virtual public ObservableCollection<string> Errors
        {
            get
            {
                return errors ?? (errors = GetCombinedErrors());
            }
        }

        private ObservableCollection<string> localErrors = new ObservableCollection<string>();
        virtual public ObservableCollection<string> LocalErrors
        {
            get
            {
                return localErrors;
            }
            set
            {
                localErrors = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalErrors"));
            }
        }

        private ObservableCollection<string> localWarnings = new ObservableCollection<string>();
        virtual public ObservableCollection<string> LocalWarnings
        {
            get
            {
                return localWarnings;
            }
            set
            {
                localWarnings = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LocalWarnings"));
            }
        }

        private ObservableCollection<string> warnings = new ObservableCollection<string>();
        private IDictionary<string, string> variables;

        virtual public ObservableCollection<string> Warnings
        {
            get
            {
                return warnings ?? (warnings = GetCombinedWarnings());
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
                if (attachedTo == value)
                {
                    // return;
                }

                attachedTo = value;

                inheritedStyleSheets = null;

                Reset();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AttachedTo"));
                
                // EagerLoading();
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
                if (combinedNamespaces == null)
                {
                    combinedNamespaces = GetCombinedNamespaces();
                    // WarmupNamespaceResolution();
                }
                return combinedNamespaces;
            }
        }

        public StyleRuleCollection Rules
        {
            get
            {
                return combinedRules ?? (combinedRules = new StyleRuleCollection(GetCombinedStyleRules()));
            }
        }

        private StyleSheetCollection InitializeBaseStyleSheet(StyleSheetCollection styleSheetCollection)
        {
            if (styleSheetCollection == null)
            {
                return null;
            }

            foreach (var added in styleSheetCollection)
            {
                added.PropertyChanged += BaseStyleSheet_PropertyChanged;
            }

            styleSheetCollection.CollectionChanged += BaseStyleSheets_CollectionChanged;

            return styleSheetCollection;
        }

        private StyleSheetCollection UninitializeBaseStyleSheet(StyleSheetCollection styleSheetCollection)
        {
            if (styleSheetCollection == null)
            {
                return null;
            }

            foreach (var added in styleSheetCollection)
            {
                added.PropertyChanged -= BaseStyleSheet_PropertyChanged;
            }

            styleSheetCollection.CollectionChanged -= BaseStyleSheets_CollectionChanged;

            return styleSheetCollection;
        }

        public StyleSheetCollection BaseStyleSheets
        {
            get
            {
                return baseStyleSheets ?? (baseStyleSheets = InitializeBaseStyleSheet(new StyleSheetCollection()));
            }

            set
            {
                UninitializeBaseStyleSheet(baseStyleSheets);

                baseStyleSheets = value;

                Reset();

                InitializeBaseStyleSheet(baseStyleSheets);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BaseStyleSheets"));

                // EagerLoading();
            }
        }

        private void BaseStyleSheets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (StyleSheet item in e.OldItems)
                {
                    item.PropertyChanged -= BaseStyleSheet_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (StyleSheet item in e.NewItems)
                {
                    item.PropertyChanged += BaseStyleSheet_PropertyChanged;
                }
            }
            Invalidate();
        }

        private void BaseStyleSheet_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Content) ||
                e.PropertyName == nameof(LocalRules) ||
                e.PropertyName == nameof(LocalNamespaces))
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

        private Dictionary<string, string> prefix2NamespaceUri = new Dictionary<string, string>();
        private Dictionary<string, string> namespaceUri2Alias = new Dictionary<string, string>();
        public string GetNamespaceUri(string alias, string shortTypename)
        {
            if (!prefix2NamespaceUri.TryGetValue(alias + shortTypename, out string value))
            {
                value = Namespaces.Where(x => x.Alias == alias)
                    .Select(x => x.Namespace)
                    .FirstOrDefault();

                value = TypeHelpers.EnsureAssemblyQualifiedName(value, shortTypename);

                prefix2NamespaceUri[alias + shortTypename] = value;
            }

            return value;
        }

        public string GetAlias(string namespaceUri)
        {
            if (!namespaceUri2Alias.TryGetValue(namespaceUri, out string value))
            {
                value = Namespaces.Where(x => x.Namespace == namespaceUri)
                    .Select(x => x.Alias)
                    .FirstOrDefault();

                namespaceUri2Alias[namespaceUri] = value;
            }

            return value;
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
                if (!ReferenceEquals(styleSheet, null))
                {
                    styleSheets.Add(styleSheet);
                }
                current = GetParent(current);
            }

            return styleSheets;
        }

        protected CssNamespaceCollection GetCombinedNamespaces()
        {
            return "GetCombinedNamespaces".Measure(() =>
            {
                if (BaseStyleSheets.Count == 0 &&
                    InheritedStyleSheets.Count == 0)
                {
                    return LocalNamespaces;
                }

                var result = new CssNamespaceCollection(
                    InheritedStyleSheets
                    .Select(x => x.Namespaces)
                    .Concat(BaseStyleSheets.Select(x => x.Namespaces))
                    .Aggregate((a, b) => new CssNamespaceCollection(a.Concat(b)))
                    .Concat(LocalNamespaces)
                    .GroupBy(x => x.Alias)
                    .Select(x => x.Last()));

                return result;
            });
        }

        private void WarmupNamespaceResolution()
        {
            // warmup
            foreach (var item in Namespaces)
            {
                GetAlias(item.Namespace);
                GetNamespaceUri(item.Alias, "");
            }
        }

        protected ObservableCollection<string> GetCombinedErrors()
        {
            if (BaseStyleSheets.Count == 0 &&
                InheritedStyleSheets.Count == 0)
            {
                return LocalErrors;
            }

            return new ObservableCollection<string>(
                InheritedStyleSheets
                .Select(x => x.Errors)
                .Concat(BaseStyleSheets.Select(x => x.Errors))
                .Aggregate((a, b) => new ObservableCollection<string>(a.Concat(b)))
                .Concat(LocalErrors)
                .GroupBy(x => x)
                .Select(x => x.First()));
        }

        protected ObservableCollection<string> GetCombinedWarnings()
        {
            if (BaseStyleSheets.Count == 0 &&
                InheritedStyleSheets.Count == 0)
            {
                return LocalWarnings;
            }

            return new ObservableCollection<string>(
                InheritedStyleSheets
                .Select(x => x.Warnings)
                .Concat(BaseStyleSheets.Select(x => x.Warnings))
                .Aggregate((a, b) => new ObservableCollection<string>(a.Concat(b)))
                .Concat(LocalWarnings)
                .GroupBy(x => x)
                .Select(x => x.First()));
        }

        protected IDictionary<string, string> GetCombinedVariables()
        {
            if (BaseStyleSheets.Count == 0 &&
                InheritedStyleSheets.Count == 0)
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
            if (BaseStyleSheets.Count == 0 &&
                InheritedStyleSheets.Count == 0)
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
                        DeclarationBlock = new StyleDeclarationBlock(GetMergedStyleDeclarations(x.ToList()), x.SelectMany(y => y.DeclarationBlock.Triggers).ToList())
                    })
                    .ToList();
        }

        protected List<StyleDeclaration> GetMergedStyleDeclarations(List<StyleRule> styleRules)
        {
            if (styleRules == null)
            {
                return new List<StyleDeclaration>();
            }

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
            namespaceUri2Alias.Clear();
            prefix2NamespaceUri.Clear();
            variables = null;

            this.localErrors.Clear();
            this.localWarnings.Clear();
            this.errors = null;
            this.warnings = null;
        }

        internal void AddError(string error)
        {
            Errors.Add(error);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Errors"));
        }
    }
}
