using FluentAssertions;
using NUnit.Framework;
using XamlCSS.CssParsing;

namespace XamlCSS.Tests.CssParsing
{
    [TestFixture]
    public class PropertyTriggerTests
    {
        [Test]
        public void PropertyTrigger_should_be_added_to_Triggers()
        {
            var content = @"
Button
{
    @Property IsFocussed True
    {
        BackgroundColor: Red;
        ForegroundColor: Green;
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as Trigger;
            first.Property.Should().Be("IsFocussed");
            first.Value.Should().Be("True");

            first.StyleDeclarationBlock[0].Property.Should().Be("BackgroundColor");
            first.StyleDeclarationBlock[0].Value.Should().Be("Red");
            first.StyleDeclarationBlock[1].Property.Should().Be("ForegroundColor");
            first.StyleDeclarationBlock[1].Value.Should().Be("Green");
        }

        [Test]
        public void PropertyTrigger_with_quoted_value_should_be_added_to_Triggers()
        {
            var content = @"
Button
{
    @Property Text ""SomeValue""
    {
        BackgroundColor: Red;
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as Trigger;
            first.Property.Should().Be("Text");
            first.Value.Should().Be("SomeValue");

            first.StyleDeclarationBlock[0].Property.Should().Be("BackgroundColor");
            first.StyleDeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void PropertyTriggerProperty_should_support_variables()
        {

            var content = @"
$variable-property: ""Text"";

Button
{
    @Property $variable-property SomeValue{
        BackgroundColor: Red;
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as Trigger;
            first.Property.Should().Be("Text");
            first.Value.Should().Be("SomeValue");

            first.StyleDeclarationBlock[0].Property.Should().Be("BackgroundColor");
            first.StyleDeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void PropertyTriggerValue_should_support_variables()
        {

            var content = @"
$variable-value: ""SomeValue"";

Button
{
    @Property Text $variable-value
    {
        BackgroundColor: Red;
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as Trigger;
            first.Property.Should().Be("Text");
            first.Value.Should().Be("SomeValue");

            first.StyleDeclarationBlock[0].Property.Should().Be("BackgroundColor");
            first.StyleDeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void PropertyTriggerProperty_with_PropertyTriggerValue_should_support_variables()
        {

            var content = @"
$property-value: Text;
$variable-value: ""SomeValue"";

Button
{
    @Property $property-value $variable-value
    {
        BackgroundColor: Red;
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as Trigger;
            first.Property.Should().Be("Text");
            first.Value.Should().Be("SomeValue");

            first.StyleDeclarationBlock[0].Property.Should().Be("BackgroundColor");
            first.StyleDeclarationBlock[0].Value.Should().Be("Red");
        }

        [Test]
        public void EnterActions_and_ExitActions_should_be_added_to_PropertyTriggers()
        {
            var content = @"
Button
{
    @Property IsFocussed True
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
            BeginStoryboard: { Storyboard: ""#StaticResource fadeOutAndInStoryboard3""; }
        }
    }
}
";
            var styleSheet = CssParser.Parse(content);

            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as Trigger;
            first.Property.Should().Be("IsFocussed");
            first.Value.Should().Be("True");

            first.StyleDeclarationBlock[0].Property.Should().Be("BackgroundColor");
            first.StyleDeclarationBlock[0].Value.Should().Be("Red");
            first.StyleDeclarationBlock[1].Property.Should().Be("ForegroundColor");
            first.StyleDeclarationBlock[1].Value.Should().Be("Green");

            first.EnterActions.Count.Should().Be(2);

            first.EnterActions[0].Action.Should().Be("BeginStoryboard");
            first.EnterActions[0].Parameters[0].Property.Should().Be("Storyboard");
            first.EnterActions[0].Parameters[0].Value.Should().Be("#StaticResource fadeOutAndInStoryboard");

            first.EnterActions[1].Action.Should().Be("BeginStoryboard");
            first.EnterActions[1].Parameters[0].Property.Should().Be("Storyboard");
            first.EnterActions[1].Parameters[0].Value.Should().Be("#StaticResource fadeOutAndInStoryboard2");

            first.ExitActions.Count.Should().Be(3);

            first.ExitActions[0].Action.Should().Be("BeginStoryboard");
            first.ExitActions[0].Parameters[0].Property.Should().Be("Storyboard");
            first.ExitActions[0].Parameters[0].Value.Should().Be("#StaticResource fadeOutAndInStoryboard");

            first.ExitActions[1].Action.Should().Be("BeginStoryboard");
            first.ExitActions[1].Parameters[0].Property.Should().Be("Storyboard");
            first.ExitActions[1].Parameters[0].Value.Should().Be("#StaticResource fadeOutAndInStoryboard2");

            first.ExitActions[2].Action.Should().Be("BeginStoryboard");
            first.ExitActions[2].Parameters[0].Property.Should().Be("Storyboard");
            first.ExitActions[2].Parameters[0].Value.Should().Be("#StaticResource fadeOutAndInStoryboard3");
        }

        [Test]
        [Ignore("animations not yet supported")]
        public void Animations()
        {
            var content = @"
@keyframes fade-out-and-in
{
    0% { Opacity: 1; }
    50% { Opacity: 0; }
    100% { Opacity: 1; }
}

Button
{
    @EventTrigger Clicked
    {
        Animation: fade-out-and-in 5s;
    }
}
";
            var styleSheet = CssParser.Parse(content);
            var first = styleSheet.Rules[0].DeclarationBlock.Triggers[0] as EventTrigger;

            first.Event.Should().Be("Clicked");
        }
    }
}
