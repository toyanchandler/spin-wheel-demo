using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelStatusTextView : WheelHudTextView
    {
        protected override void Apply(WheelHudSnapshot snapshot)
        {
            ApplyVisibleText(Text, snapshot.IsStatusVisible, snapshot.StatusText, snapshot.StatusColor);
        }
    }
}
