using System.Diagnostics;
using System.Text;

namespace XamlCSS.CssParsing
{
	[DebuggerDisplay("{Type} {Text}")]
	public class CssToken
	{
		public CssTokenType Type { get; set; }
		public string Text { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public CssToken(CssTokenType type, string text, int line, int column)
		{
			Type = type;
			Text = text;
            Line = line;
            Column = column;
        }

		public override bool Equals(object obj)
		{
			var other = obj as CssToken;

			if (object.ReferenceEquals(other, null))
			{
				return false;
			}

			return this.Type == other.Type &&
				this.Text == other.Text;
		}

		public override int GetHashCode()
		{
			return this.Type.GetHashCode() ^ this.Text.GetHashCode();
		}

        private bool? cachedIsLetterOrDigit;
        public bool? IsLetterOrDigit(StringBuilder value)
        {
            if (cachedIsLetterOrDigit != null)
                return cachedIsLetterOrDigit.Value;

            if (value.Length == 0)
                return null;

            cachedIsLetterOrDigit = char.IsLetterOrDigit(value[0]);

            return cachedIsLetterOrDigit.Value;
        }
    }
}
