using Vertigo.Wheel.Runtime;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZoneTextView : WheelHudTextView
    {
        protected override void Apply(WheelHudSnapshot snapshot)
        {
            Text.text = string.Empty;
            Text.color = Color.clear;
        }
    }
}
