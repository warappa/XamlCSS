using System.Collections.Generic;
using AngleSharp.Dom;

namespace XamlCSS.Dom
{
	public class TokenList : List<string>, ITokenList
	{
		public int Length { get { return this.Count; } }

		public void Add(params string[] tokens)
		{
			this.AddRange(tokens);
		}

		public void Remove(params string[] tokens)
		{
			foreach (var t in tokens)
			{
				this.Remove(t);
			}
		}

		public bool Toggle(string token, bool force = false)
		{
			if (this.Contains(token))
				Remove(token);
			else
				Add(token);

			return true;
		}
	}
}
