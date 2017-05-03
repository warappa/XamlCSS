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
    }
}
