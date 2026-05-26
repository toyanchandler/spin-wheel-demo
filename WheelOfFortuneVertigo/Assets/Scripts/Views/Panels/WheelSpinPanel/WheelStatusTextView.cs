using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelStatusTextView : WheelHudTextView
    {
        protected override void Apply(WheelHudSnapshot snapshot)
        {
            WheelHudStatusSnapshot status = snapshot.StatusBar;
            ApplyVisibleText(Text, status.IsStatusVisible, status.StatusText, status.StatusColor);
        }
    }
}
