using System.Diagnostics;

namespace XamlCSS
{
    [DebuggerDisplay("{ChangeKind} {RenderTargetKind} {(StartFrom ?? StyleSheetHolder).GetType().Name} {GetHashCode()}")]
    public class RenderInfo<TDependencyObject>
        where TDependencyObject : class
    {
        public TDependencyObject StyleSheetHolder { get; set; }
        public StyleSheet StyleSheet { get; set; }
        public TDependencyObject StartFrom { get; set; }
        public RenderTargetKind RenderTargetKind { get; internal set; }
        public ChangeKind ChangeKind { get; internal set; }

        public override bool Equals(object obj)
        {
            var other = obj as RenderInfo<TDependencyObject>;
            if (other == null)
            {
                return false;
            }

            return StyleSheetHolder == other.StyleSheetHolder &&
                StyleSheet == other.StyleSheet &&
                StartFrom == other.StartFrom &&
                RenderTargetKind == other.RenderTargetKind &&
                ChangeKind == other.ChangeKind;
        }

        public override int GetHashCode()
        {
            var result = 0;
            result = (result * 397) ^ (StyleSheetHolder?.GetHashCode() ?? 0);
            result = (result * 397) ^ (StyleSheet?.GetHashCode() ?? 0);
            result = (result * 397) ^ (StartFrom?.GetHashCode() ?? 0);
            result = (result * 397) ^ (RenderTargetKind.GetHashCode());
            result = (result * 397) ^ (ChangeKind.GetHashCode());
            return result;
        }

        public override string ToString()
        {
            return $"{ChangeKind} {RenderTargetKind} {StartFrom?.GetType().Name ?? "null"} {GetHashCode()}";
        }
    }
}
