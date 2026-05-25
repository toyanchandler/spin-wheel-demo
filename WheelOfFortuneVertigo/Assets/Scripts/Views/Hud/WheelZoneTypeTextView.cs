using Vertigo.Wheel.Runtime;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZoneTypeTextView : WheelHudTextView
    {
        protected override void Apply(WheelHudSnapshot snapshot)
        {
            Text.text = snapshot.ZoneTypeLabel;
            Text.color = Color.Lerp(snapshot.ZoneTypeColor, new Color(0.74f, 0.90f, 1f, 1f), 0.74f);
        }
    }
}
