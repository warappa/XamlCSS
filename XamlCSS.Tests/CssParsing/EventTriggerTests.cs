using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class EventTriggerTests
    {
        [Test]
        public void EventTrigger_should_be_added_to_Triggers()
        {
            var content = @"
Button
{
    @Event Clicked
    {
        BeginStoryboard: { Storyboard: #StaticResource fadeOutAndInStoryboard; }
        Transition: 
        {
            FontSize: initial 50 500ms ease-in-out;
            Width: 100 200 500ms;
            Height: initial 300 200ms;
            Height: initial 200 500ms;
        }
    }
}
";
            var styleSheet = CssParser.Parse(content);
            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as EventTrigger;

            first.Event.Should().Be("Clicked");

            first.Actions[0].Action.Should().Be("BeginStoryboard");
            first.Actions[0].Parameters[0].Value.Should().Be("#StaticResource fadeOutAndInStoryboard");

            first.Actions[1].Action.Should().Be("Transition");
            first.Actions[1].Parameters[0].Property.Should().Be("FontSize");
            first.Actions[1].Parameters[0].Value.Should().Be("initial 50 500ms ease-in-out");

            first.Actions[1].Parameters[1].Property.Should().Be("Width");
            first.Actions[1].Parameters[1].Value.Should().Be("100 200 500ms");

            first.Actions[1].Parameters[2].Property.Should().Be("Height");
            first.Actions[1].Parameters[2].Value.Should().Be("initial 300 200ms");

            first.Actions[1].Parameters[3].Property.Should().Be("Height");
            first.Actions[1].Parameters[3].Value.Should().Be("initial 200 500ms");
        }
    }
}
