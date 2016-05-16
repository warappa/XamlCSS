namespace XamlCSS
{
	public class StyleSheet
	{
		public static readonly StyleSheet Empty = new StyleSheet();

		private StyleRuleCollection _rules = new StyleRuleCollection();

		public StyleRuleCollection Rules
		{
			get { return _rules; }
			set { _rules = value; }
		}
	}
}
