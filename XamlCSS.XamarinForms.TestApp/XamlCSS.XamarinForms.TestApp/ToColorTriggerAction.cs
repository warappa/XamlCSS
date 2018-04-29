using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamlCSS.XamarinForms.TestApp
{
    [XamlCSS.Linker.Preserve(AllMembers = true)]
    public class ToColorTriggerAction : TriggerAction<VisualElement>
    {
        public static Task<bool> ColorTo(VisualElement self, Color fromColor, Color toColor, Action<Color> callback, uint length = 250, Easing easing = null)
        {
            Func<double, Color> transform = (t) =>
              Color.FromRgba(fromColor.R + t * (toColor.R - fromColor.R),
                             fromColor.G + t * (toColor.G - fromColor.G),
                             fromColor.B + t * (toColor.B - fromColor.B),
                             fromColor.A + t * (toColor.A - fromColor.A));

            return ColorAnimation(self, "ToColorTriggerAction", transform, callback, length, easing);
        }

        static Task<bool> ColorAnimation(VisualElement element, string name, Func<double, Color> transform, Action<Color> callback, uint length, Easing easing)
        {
            easing = easing ?? Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();
            if (element.AnimationIsRunning("ToColorTriggerAction"))
            {
                element.AbortAnimation("ToColorTriggerAction");
            }
            element.Animate<Color>(name, transform, callback, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }

        public Color To { get; set; }
        public uint Duration { get; set; }

        protected override void Invoke(VisualElement entry)
        {
            ColorTo(entry, entry.BackgroundColor, To, c => entry.BackgroundColor = c, Duration, Easing.Linear);
        }
    }
}
