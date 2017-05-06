using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlCSS.CssParsing
{
    public class AstGenerationException : Exception
    {
        public AstGenerationException(string message, CssToken token)
            :base(message)
        {
            Token = token;
        }

        public CssToken Token { get; private set; }
    }
}
