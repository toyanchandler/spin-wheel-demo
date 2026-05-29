using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupObjectHandle
    {
        private readonly GameObject _target;

        public WheelOutcomePopupObjectHandle(GameObject target) => _target = target;
        public void SetActive(bool active) => _target.SetActive(active);
    }
}
