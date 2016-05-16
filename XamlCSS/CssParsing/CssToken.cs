using System.Diagnostics;

namespace XamlCSS.CssParsing
{
	[DebuggerDisplay("{Type} {Text}")]
	public class CssToken
	{
		public CssTokenType Type { get; set; }
		public string Text { get; set; }

		public CssToken() { }
		public CssToken(CssTokenType type, string text)
		{
			Type = type;
			Text = text;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssToken;

			if (object.ReferenceEquals(other, null))
				return false;

			return this.Type == other.Type &&
				this.Text == other.Text;
		}

		public override int GetHashCode()
		{
			return this.Type.GetHashCode() ^ this.Text.GetHashCode();
		}
	}
}
