using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupRewardBurstCameraBinding : WheelOutcomePopupComponentBinding<Camera>
    {
        public Camera Camera { get { return Component; } }
    }
}
