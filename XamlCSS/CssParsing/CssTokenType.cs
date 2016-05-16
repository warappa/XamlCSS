namespace XamlCSS.CssParsing
{
	public enum CssTokenType
	{
		Unknown = 0,
		Identifier = 1,
		At = 2,
		Dot = 3,
		BraceOpen = 4,
		BraceClose = 5,
		Semicolon = 6,
		Comma = 7,
		Colon = 8,
		AngleBraketOpen = 9,
		AngleBraketClose = 10,
		Pipe = 11,
		DoubleQuotes = 12,
		SingleQuotes = 13,
		ParenthesisOpen = 14,
		ParenthesisClose = 15,
		Whitespace = 16,
		Hash = 17
	}
}
