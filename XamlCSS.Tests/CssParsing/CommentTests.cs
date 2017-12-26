using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class CommentTests
    {
        [Test]
        public void Can_ignore_comments()
        {
            var css = @"
/* some comment */
// some comment
Element {
    /* some comment */
    PropertyA: Red /* some comment */;
    // some comment
    PropertyB: /* some comment */ Orange;
    PropertyC: Blue; // some comment
}
.class /* some comment */ .other-class {
    /* some comment */
    PropertyA: Red /* some comment */;
    // some comment
    PropertyB: /* some comment */ Orange;
    PropertyC: Blue; // some comment
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(2);

            styleSheet.Rules[0].SelectorString.Should().Be("Element");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("PropertyA");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Red");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("PropertyB");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Orange");
            styleSheet.Rules[0].DeclarationBlock[2].Property.Should().Be("PropertyC");
            styleSheet.Rules[0].DeclarationBlock[2].Value.Should().Be("Blue");

            styleSheet.Rules[1].SelectorString.Should().Be(".class .other-class");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("PropertyA");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("Red");
            styleSheet.Rules[1].DeclarationBlock[1].Property.Should().Be("PropertyB");
            styleSheet.Rules[1].DeclarationBlock[1].Value.Should().Be("Orange");
            styleSheet.Rules[1].DeclarationBlock[2].Property.Should().Be("PropertyC");
            styleSheet.Rules[1].DeclarationBlock[2].Value.Should().Be("Blue");
        }

        [Test]
        public void Can_override_imported_css_file()
        {
            CssParser.Initialize(null, new TestCssFileProvider());
            var css = @"
@import 'CssParsing\\TestData\\ImportCss.scss';

$textColor: Pink;

@mixin InputElement()
{
    Background: Orange;
}

SomeElement
{
    @include InputElement();

    TextColor: $textColor;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(2);

            styleSheet.Rules[0].SelectorString.Should().Be("SomeElement");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("Background");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Orange");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Pink");

            styleSheet.Rules[1].SelectorString.Should().Be(".header");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("TextColor");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("Red");
            styleSheet.Rules[1].DeclarationBlock[1].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[1].DeclarationBlock[1].Value.Should().Be("White");
        }

        [Test]
        public void Dont_hang_on_first_char_of_comment()
        {
            CssParser.Initialize(null, new TestCssFileProvider());
            var css = @"
.fa {
    FontFamily: ""test"";
/
                    
    &.fa-css3 {
        Text: ""\f13c"";
    }
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);
            styleSheet.Errors.Count.Should().Be(1);

            styleSheet.Rules[0].SelectorString.Should().Be(".fa");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("FontFamily");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("test");

        }

        [Test]
        public void Dont_hang_on_first_char_of_comment_2()
        {
            CssParser.Initialize(null, new TestCssFileProvider());
            var css = @"
/
.fa {
    FontFamily: ""test"";
                    
    &.fa-css3 {
        Text: ""\f13c"";
    }
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(2);
            styleSheet.Errors.Count.Should().Be(1);

            styleSheet.Rules[0].SelectorString.Should().Be(".fa");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("FontFamily");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("test");

        }

        [Test]
        public void Can_parse_slash_in_comment()
        {
            CssParser.Initialize(null, new TestCssFileProvider());
            var css = @"
.fa {
    /*/asdfasdfsd*/
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);
            styleSheet.Errors.Count.Should().Be(0);

        }
    }
}
