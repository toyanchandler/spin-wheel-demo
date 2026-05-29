using System;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    /// <summary>
    /// Serialized outcome-popup hierarchy. Assign UI refs in <see cref="_wiring"/> (flat list).
    /// </summary>
    public sealed class WheelOutcomePopupBindings : MonoBehaviour
    {
        [SerializeField] private WheelOutcomePopupSceneWiring _wiring;

        private WheelOutcomePopupHandleSet _handles;

        internal WheelOutcomePopupHandleSet Handles
        {
            get
            {
                if (_handles == null)
                {
                    throw new System.InvalidOperationException(
                        name + " outcome popup handles are not validated. Call Validate() before use.");
                }

                return _handles;
            }
        }

        public void Validate()
        {
            _handles = WheelOutcomePopupHandleSet.FromWiring(this, _wiring);
        }

        public void CaptureHome()
        {
            Handles.CaptureHome();
        }

        internal WheelOutcomePopupRefs CreateRefs(
            WheelLootPresentationChannel lootPresentation,
            Func<WheelOutcomeSnapshot> getCurrentSnapshot,
            Action markPresentationComplete)
        {
            return WheelOutcomePopupRefs.From(Handles, lootPresentation, getCurrentSnapshot, markPresentationComplete);
        }
    }
}
