using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupRewardBurstCameraBinding : WheelOutcomePopupSceneComponentBinding<Camera>
    {
        public Camera Camera { get { return RequiredComponent; } }
    }
}
