using System.Diagnostics;

namespace XamlCSS
{
    [DebuggerDisplay("{Remove} {StartFrom.GetType().Name}  {GetHashCode()}")]
    public class RenderInfo<TDependencyObject, TUIElement>
        where TDependencyObject : class
        where TUIElement : class, TDependencyObject
    {
        public TUIElement StyleSheetHolder { get; set; }
        public StyleSheet StyleSheet { get; set; }
        public TUIElement StartFrom { get; set; }
        public bool Remove { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RenderInfo<TDependencyObject, TUIElement>;
            if (other == null)
            {
                return false;
            }

            return StyleSheetHolder == other.StyleSheetHolder &&
                StyleSheet == other.StyleSheet &&
                StartFrom == other.StartFrom &&
                Remove == other.Remove;
        }

        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 397) ^ (StyleSheetHolder?.GetHashCode() ?? 0);
            result = (result * 397) ^ (StyleSheet?.GetHashCode() ?? 0);
            result = (result * 397) ^ (StartFrom?.GetHashCode() ?? 0);
            result = (result * 397) ^ (Remove.GetHashCode());
            return result;
        }
    }
}
