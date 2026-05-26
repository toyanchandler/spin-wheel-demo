using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupRootBinding : WheelOutcomePopupComponentBinding<CanvasGroup>
    {
        public GameObject Root { get { return gameObject; } }
        public CanvasGroup CanvasGroup { get { return Component; } }
    }
}
