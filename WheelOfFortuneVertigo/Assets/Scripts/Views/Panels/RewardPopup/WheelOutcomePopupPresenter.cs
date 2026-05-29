using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupPresenter
    {
        private readonly WheelOutcomePopupRefs _binding;
        private readonly Object _tweenTarget;
        private bool _hasOutcome;
        private bool _isPopupAllowed;
        private Sequence _sequence;
        private WheelOutcomePopupMotion _motion;
        private WheelOutcomeSnapshot _currentSnapshot;

        public WheelOutcomeSnapshot CurrentSnapshot => _currentSnapshot;

        public WheelOutcomePopupPresenter(WheelOutcomePopupRefs binding, Object tweenTarget)
        {
            _binding = binding;
            _tweenTarget = tweenTarget;
            _motion = WheelOutcomePopupMotion.Default();
            _isPopupAllowed = true;
        }

        public void Reset()
        {
            _hasOutcome = false;
            _isPopupAllowed = true;
            WheelOutcomePopupAnimator.Hide(_binding, ref _sequence);
        }

        public void HandleOutcome(WheelOutcomeSnapshot snapshot)
        {
            WheelOutcomePopupContentApplier.Apply(_binding, snapshot);
            _motion = snapshot.Motion;
            _currentSnapshot = snapshot;
            _hasOutcome = snapshot.HasPresentableOutcome;
            SyncVisibility();
        }

        public void HandleHud(bool isOutcomePopupAllowed)
        {
            _isPopupAllowed = isOutcomePopupAllowed;
            SyncVisibility();
        }

        public void MarkPresentationComplete()
        {
            _hasOutcome = false;
            WheelOutcomePopupAnimator.ClearMainIcon(_binding);
        }

        private void SyncVisibility()
        {
            WheelOutcomePopupVisibility.Apply(
                _binding,
                _motion,
                _tweenTarget,
                _currentSnapshot,
                _hasOutcome && _isPopupAllowed,
                _hasOutcome,
                ref _sequence);
        }
    }
}
