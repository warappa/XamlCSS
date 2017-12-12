using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class DataTriggerTests
    {
        [Test]
        public void DataTrigger_should_be_added_to_Triggers()
        {
            var content = @"
Button
{
    @Data Text.Length 10
    {
        IsEnabled: False;
        ForegroundColor: Red;
    }
}
";
            var styleSheet = CssParser.Parse(content);
            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as DataTrigger;

            first.Binding.Should().Be("Text.Length");
            first.Value.Should().Be("10");
        }

        [Test]
        public void DataTrigger_should_support_markup_extensions()
        {
            var content = @"
.field {
    @Data ""{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}"" true
    {
        Background: Green;
        Foreground: White;
    }
}
";
            var styleSheet = CssParser.Parse(content);
            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as DataTrigger;

            first.Binding.Should().Be(@"{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}");
            first.Value.Should().Be("true");
        }

        [Test]
        public void DataTrigger_binding_expression_should_support_variables()
        {

            var content = @"
$variable-value: ""{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}"";
.field {
    @Data $variable-value true
    {
        Background: Green;
        Foreground: White;
    }
}
";
            var styleSheet = CssParser.Parse(content);
            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as DataTrigger;

            first.Binding.Should().Be(@"{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}");
            first.Value.Should().Be("true");
        }

        [Test]
        public void DataTriggerValue_should_support_variables()
        {

            var content = @"
$variable-value: true;
.field {
    @Data ""{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}""  $variable-value{
        Background: Green;
        Foreground: White;
    }
}
";
            var styleSheet = CssParser.Parse(content);
            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as DataTrigger;

            first.Binding.Should().Be(@"{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}");
            first.Value.Should().Be("true");
        }

        [Test]
        public void DataTriggerBinding_with_DataTriggerValue_should_support_variables()
        {

            var content = @"
$binding: ""{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}"";
$value: true;
.field {
    @Data $binding $value
    {
        Background: Green;
        Foreground: White;
    }
}
";
            var styleSheet = CssParser.Parse(content);
            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as DataTrigger;

            first.Binding.Should().Be(@"{ Binding RelativeSource={RelativeSource Self}, Path=IsFocused}");
            first.Value.Should().Be("true");
        }

        [Test]
        public void EnterActions_and_ExitActions_should_be_added_to_DataTriggers()
        {
            var content = @"
Button
{
    @Data Text.Length 10
    {
        @Enter: 
        {
            BeginStoryboard: 
            {
                Storyboard: #StaticResource fadeOutAndInStoryboard;
            }
            BeginStoryboard:
            {
                Storyboard: ""#StaticResource fadeOutAndInStoryboard2"";
            }
        }

        BackgroundColor: Red;
        ForegroundColor: Green;

        @Exit: 
        {
            BeginStoryboard: { Storyboard: #StaticResource fadeOutAndInStoryboard; }
            BeginStoryboard: { Storyboard: #StaticResource fadeOutAndInStoryboard2; }
            BeginStoryboard: { Storyboard: #StaticResource fadeOutAndInStoryboard3; }
        }
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as DataTrigger;
            first.Binding.Should().Be("Text.Length");
            first.Value.Should().Be("10");

            first.StyleDeclarationBlock[0].Property.Should().Be("BackgroundColor");
            first.StyleDeclarationBlock[0].Value.Should().Be("Red");
            first.StyleDeclarationBlock[1].Property.Should().Be("ForegroundColor");
            first.StyleDeclarationBlock[1].Value.Should().Be("Green");

            first.EnterActions.Count.Should().Be(2);
            first.ExitActions.Count.Should().Be(3);
        }

        [Test]
        public void EnterActions_and_ExitActions_should_be_added_to_DataTriggers_3()
        {
            var content = @"
Button
{
    @Data Message ""Hello World from DataContext!""
    {
        @Enter:
        {
            animation|BeginStoryboard: 
            {
                Storyboard: #StaticResource storyboard;
            }
        }
                    
        Background: #ff00ff;
        Grid.Row: 0;
	    Grid.Column: 0;
                    
        @Exit:
        {
            animation|BeginStoryboard: 
            {
                Storyboard: #StaticResource storyboard2;
            }
        }
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as DataTrigger;
            first.Binding.Should().Be("Message");
            first.Value.Should().Be("Hello World from DataContext!");

            first.EnterActions.Count.Should().Be(1);
            first.EnterActions[0].Parameters[0].Property.Should().Be("Storyboard");
            first.EnterActions[0].Parameters[0].Value.Should().Be("#StaticResource storyboard");

            first.StyleDeclarationBlock[0].Property.Should().Be("Background");
            first.StyleDeclarationBlock[0].Value.Should().Be("#ff00ff");
            first.StyleDeclarationBlock[1].Property.Should().Be("Grid.Row");
            first.StyleDeclarationBlock[1].Value.Should().Be("0");
            first.StyleDeclarationBlock[2].Property.Should().Be("Grid.Column");
            first.StyleDeclarationBlock[2].Value.Should().Be("0");

            first.ExitActions.Count.Should().Be(1);
            first.ExitActions[0].Parameters[0].Property.Should().Be("Storyboard");
            first.ExitActions[0].Parameters[0].Value.Should().Be("#StaticResource storyboard2");

            first.EnterActions.Count.Should().Be(1);
            first.ExitActions.Count.Should().Be(1);
        }
    }
}
