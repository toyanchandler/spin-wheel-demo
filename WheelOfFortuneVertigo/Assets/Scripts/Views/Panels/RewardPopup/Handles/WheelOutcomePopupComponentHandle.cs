using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupComponentHandle<TComponent> where TComponent : Component
    {
        public TComponent Component { get; }

        public WheelOutcomePopupComponentHandle(TComponent component) => Component = component;
    }
}
