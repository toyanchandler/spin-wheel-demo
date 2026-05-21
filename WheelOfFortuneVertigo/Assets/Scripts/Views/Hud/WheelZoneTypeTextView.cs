using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZoneTypeTextView : WheelHudTextView
    {
        protected override void Apply(WheelHudSnapshot snapshot)
        {
            Text.text = snapshot.ZoneTypeLabel;
            Text.color = snapshot.ZoneTypeColor;
        }
    }
}
