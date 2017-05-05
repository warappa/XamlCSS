using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    [DebuggerDisplay("{Value.ToString()}")]
    public class RawToken
    {
        public StringBuilder Value { get; set; } = new StringBuilder();
        public int Line { get; set; }
        public int Column { get; set; }

        private bool? cachedIsWhitespace;
        public bool? IsWhitespace
        {
            get
            {
                if (cachedIsWhitespace != null)
                    return cachedIsWhitespace.Value;

                if (Value.Length == 0)
                    return null;

                cachedIsWhitespace = char.IsWhiteSpace(Value.ToString()[0]);

                return cachedIsWhitespace.Value;
            }
        }

        private bool? cachedIsLetterOrDigit;
        public bool? IsLetterOrDigit
        {
            get
            {
                if (cachedIsLetterOrDigit != null)
                    return cachedIsLetterOrDigit.Value;

                if (Value.Length == 0)
                    return null;

                cachedIsLetterOrDigit = char.IsLetterOrDigit(Value.ToString()[0]);

                return cachedIsLetterOrDigit.Value;
            }
        }
    }
}
