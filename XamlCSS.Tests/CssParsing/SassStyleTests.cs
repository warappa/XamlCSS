using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class SassStyleTests
    {
        [Test]
        public void Can_parse_rule_to_ast()
        {
            var css = @"
.header {
    BackgroundColor: Green;
}
";

            var ast = CssParser.GetAst(css);

            ast.GetSelectorNode(0, 0, 0).Text.Should().Be(".header");
        }

        [Test]
        public void Can_parse_nested_rule_to_ast()
        {
            var css = @"
.header {
    BackgroundColor: Green;

    Label {
        BackgroundColor: Red;
    }
}
";

            var ast = CssParser.GetAst(css);
            var headerRuleNode = ast.GetRootStyleRuleNode(0);
            var labelRuleNode = headerRuleNode
                .GetSubStyleRuleNode(0);
            labelRuleNode
                .GetSelectorNode(0, 0, 0).Text.Should().Be("Label");


        }

        [Test]
        public void Can_parse_nested_rule_to_stylesheet()
        {
            var css = @"
.header {
    BackgroundColor: Green;

    Label {
        BackgroundColor: Red;
    }
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(2);

            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Green");

            styleSheet.Rules[1].SelectorString.Should().Be(".header Label");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void Can_parse_nested_rule_to_stylesheet_with_multiple_selectors_on_root_rule()
        {
            var css = @"
.header,
StackLayout {
    BackgroundColor: Green;

    Label {
        BackgroundColor: Red;
    }
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(4);

            styleSheet.Rules[0].SelectorString.Should().Be("StackLayout");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Green");

            styleSheet.Rules[1].SelectorString.Should().Be("StackLayout Label");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("Red");

            styleSheet.Rules[2].SelectorString.Should().Be(".header");
            styleSheet.Rules[2].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[2].DeclarationBlock[0].Value.Should().Be("Green");

            styleSheet.Rules[3].SelectorString.Should().Be(".header Label");
            styleSheet.Rules[3].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[3].DeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void Can_parse_nested_ampersand_class_selectors_to_stylesheet_with_multiple_selectors_on_root_rule()
        {
            var css = @"
.header,
StackLayout {
    BackgroundColor: Green;

    &.active,
    &.warning {
        BackgroundColor: Red;
    }
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(6);

            styleSheet.Rules[0].SelectorString.Should().Be("StackLayout");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Green");

            styleSheet.Rules[1].SelectorString.Should().Be(".header");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("Green");

            styleSheet.Rules[2].SelectorString.Should().Be("StackLayout.active");
            styleSheet.Rules[2].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[2].DeclarationBlock[0].Value.Should().Be("Red");

            styleSheet.Rules[3].SelectorString.Should().Be("StackLayout.warning");
            styleSheet.Rules[3].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[3].DeclarationBlock[0].Value.Should().Be("Red");

            styleSheet.Rules[4].SelectorString.Should().Be(".header.active");
            styleSheet.Rules[4].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[4].DeclarationBlock[0].Value.Should().Be("Red");

            styleSheet.Rules[5].SelectorString.Should().Be(".header.warning");
            styleSheet.Rules[5].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[5].DeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void Can_parse_nested_ampersand_element_selector_to_stylesheet_with_multiple_selectors_on_root_rule()
        {
            var css = @"
.header {
    BackgroundColor: Green;

    &StackLayout,
    &Button {
        BackgroundColor: Red;
    }
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(3);


            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Green");

            styleSheet.Rules[1].SelectorString.Should().Be(".headerStackLayout");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("Red");

            styleSheet.Rules[2].SelectorString.Should().Be(".headerButton");
            styleSheet.Rules[2].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[2].DeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void Can_parse_nested_selector_to_stylesheet_with_multiple_selectors_on_root_rule()
        {
            var css = @"
.header {
    BackgroundColor: Green;

    .inner {
        .active {
            BackgroundColor: Red;
        }
    }
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(3);

            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Green");

            styleSheet.Rules[1].SelectorString.Should().Be(".header .inner");

            styleSheet.Rules[2].SelectorString.Should().Be(".header .inner .active");
            styleSheet.Rules[2].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[2].DeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void Can_parse_and_use_color_variables()
        {
            var css = @"
$background: #ff00ff;
$foreground: #00ff00;
.header {
    BackgroundColor: $background;
    TextColor: $foreground;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("#ff00ff");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("#00ff00");
        }

        [Test]
        public void Can_parse_and_use_markup_extensions_variables()
        {
            var css = @"
$background: #Binding BackgroundColor;
$foreground: #Binding ForegroundColor;
.header {
    BackgroundColor: $background;
    TextColor: $foreground;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("#Binding BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("#Binding ForegroundColor");
        }

        [Test]
        public void Can_parse_and_use_markup_extensions_variables_with_Xaml_syntax()
        {
            var css = @"
$background: ""{Binding BackgroundColor}"";
$foreground: ""{Binding ForegroundColor}"";
.header {
    BackgroundColor: $background;
    TextColor: $foreground;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("{Binding BackgroundColor}");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("{Binding ForegroundColor}");
        }

        [Test]
        public void Can_parse_and_use_text_variables()
        {
            var css = @"
$textVariable1: ""Title"";
$textVariable2: ""Subtitle"";
.header {
    Title: $textVariable1;
    SubTitle: $textVariable2;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(1);

            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("Title");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("Subtitle");
        }

        [Test]
        public void Can_parse_and_use_redefined_color_variables()
        {
            var css = @"
$background: #ff00ff;
$foreground: #00ff00;
.header {
    $background: red;
    
    BackgroundColor: $background;
    TextColor: $foreground;
}
.footer {
    BackgroundColor: $background;
    TextColor: $foreground;
}
";

            var styleSheet = CssParser.Parse(css);

            styleSheet.Rules.Count.Should().Be(2);

            styleSheet.Rules[0].SelectorString.Should().Be(".header");
            styleSheet.Rules[0].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[0].DeclarationBlock[0].Value.Should().Be("red");
            styleSheet.Rules[0].DeclarationBlock[1].Property.Should().Be("TextColor");
            styleSheet.Rules[0].DeclarationBlock[1].Value.Should().Be("#00ff00");

            styleSheet.Rules[1].SelectorString.Should().Be(".footer");
            styleSheet.Rules[1].DeclarationBlock[0].Property.Should().Be("BackgroundColor");
            styleSheet.Rules[1].DeclarationBlock[0].Value.Should().Be("#ff00ff");
            styleSheet.Rules[1].DeclarationBlock[1].Property.Should().Be("TextColor");
            styleSheet.Rules[1].DeclarationBlock[1].Value.Should().Be("#00ff00");
        }
    }
}
