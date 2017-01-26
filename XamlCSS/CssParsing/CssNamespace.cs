namespace XamlCSS.CssParsing
{
    public class CssNamespace
    {
        public CssNamespace(string alias, string @namespace)
        {
            this.Alias = alias;
            this.Namespace = @namespace;
        }

        public string Alias { get; set; }
        public string Namespace { get; set; }

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
