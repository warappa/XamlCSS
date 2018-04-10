using System;
using XamlCSS.Utils;

namespace XamlCSS
{
    public class CssNamespace
    {
        private string @namespace;
        private string[] namespaceFragments;

        public CssNamespace() { }
        public CssNamespace(string alias, string @namespace)
        {
            this.Alias = alias;
            this.Namespace = @namespace;
        }

        public string Alias { get; set; }
        public string Namespace
        {
            get => @namespace;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("namespace");

                if (value.Contains("clr-namespace:"))
                {
                    var strs = value.Substring(14).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length == 2)
                    {
                        var ns = strs[0];
                        var assembly = strs[1].Replace("assembly=", "");

                        if (assembly == "Windows")
                        {
                            // for UWP types
                            assembly += ", ContentType=WindowsRuntime";
                        }

                        this.@namespace = $"{ns}, {assembly}";
                    }
                    else
                    {
                        this.@namespace = value;
                    }
                }
                else
                {
                    this.@namespace = value;
                }

                namespaceFragments = this.@namespace.Split(',');
            }
        }

        public string[] NamespaceFragments => namespaceFragments;

        public override bool Equals(object obj)
        {
            var other = obj as CssNamespace;
            if (other == null)
            {
                return false;
            }

            return other.Alias == Alias &&
                other.Namespace == Namespace;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                hash = (hash * 17) + Alias.GetHashCode();
                hash = (hash * 17) + Namespace.GetHashCode();

                return hash;
            }
        }
    }
}
