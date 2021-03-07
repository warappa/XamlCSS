using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class ExtendTests
    {
        [Test]
        public void Can_parse_simple_extend()
        {
            var css = @"
.message {
  border: 1px solid #ccc;
  padding: 10px;
  color: #333;
}

.success {
  @extend .message;
  border-color: green;
}

.error {
  @extend .message;
  border-color: red;
}

.warning {
  @extend .message;
  border-color: yellow;
}
";

            var expected = @"
.message, .success, .error, .warning {
  border: 1px solid #ccc;
  padding: 10px;
  color: #333;
}

.success {
  border-color: green;
}

.error {
  border-color: red;
}

.warning {
  border-color: yellow;
}";

            var styleSheet = CssParser.Parse(css);

            var styleSheet2 = CssParser.Parse(expected);

            styleSheet.Should().BeEquivalentTo(styleSheet2, options => options.Excluding(x => x.Id));
        }

        [Test]
        public void Can_parse_multiple_extends()
        {
            var css = @"
.message {
  border: 1px solid #ccc;
  padding: 10px;
  color: #333;
}

.box {
  text-decoration: none;
}

.success {
  @extend .message;
  @extend .box;
  border-color: green;
}

.error {
  @extend .message;
  @extend .box;
  border-color: red;
}

.warning {
  @extend .message;
  @extend .box;
  border-color: yellow;
}
";

            var expected = @"
.message, .success, .error, .warning {
  border: 1px solid #ccc;
  padding: 10px;
  color: #333;
}

.box, .success, .error, .warning {
    text-decoration: none;
}

.success {
  border-color: green;
}

.error {
  border-color: red;
}

.warning {
  border-color: yellow;
}";

            var styleSheet = CssParser.Parse(css);

            var styleSheet2 = CssParser.Parse(expected);

            styleSheet.Should().BeEquivalentTo(styleSheet2, options => options.Excluding(x => x.Id));
        }
    }
}
