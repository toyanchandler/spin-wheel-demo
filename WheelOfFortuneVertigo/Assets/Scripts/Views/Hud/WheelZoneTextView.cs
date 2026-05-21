using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZoneTextView : WheelHudTextView
    {
        protected override void Apply(WheelHudSnapshot snapshot)
        {
            Text.SetText("ZONE {0}", snapshot.Zone);
            Text.color = snapshot.ZoneNumberColor;
        }
    }
}
