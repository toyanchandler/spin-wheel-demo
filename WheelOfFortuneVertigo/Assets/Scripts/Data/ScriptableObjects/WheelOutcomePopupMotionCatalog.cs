using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelOutcomePopupMotionCatalog", menuName = "Vertigo/Wheel Outcome Popup Motion Catalog")]
    public sealed class WheelOutcomePopupMotionCatalog : ScriptableObject
    {
        [SerializeField] private WheelOutcomePopupMotion _motion = WheelOutcomePopupMotion.Default();

        public WheelOutcomePopupMotion Motion { get { return _motion; } }

        public void ResetToDefaults()
        {
            _motion = WheelOutcomePopupMotion.Default();
        }
    }
}
