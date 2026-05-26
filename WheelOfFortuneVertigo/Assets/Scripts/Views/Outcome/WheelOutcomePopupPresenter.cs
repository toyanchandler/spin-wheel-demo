using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupPresenter
    {
        private struct RuntimeState
        {
            public bool HasOutcome;
            public bool PopupAllowed;
        }

        private readonly WheelOutcomePopupRefs _binding;
        private readonly Object _tweenTarget;
        private RuntimeState _state;
        private Sequence _sequence;
        private WheelOutcomePopupMotion _motion;
        private WheelOutcomeSnapshot _currentSnapshot;

        public WheelOutcomeSnapshot CurrentSnapshot { get { return _currentSnapshot; } }

        public WheelOutcomePopupPresenter(WheelOutcomePopupRefs binding, Object tweenTarget)
        {
            _binding = binding;
            _tweenTarget = tweenTarget;
            _motion = WheelOutcomePopupMotion.Default();
            _state.PopupAllowed = true;
        }

        public void Reset()
        {
            _state = default(RuntimeState);
            _state.PopupAllowed = true;
            WheelOutcomePopupAnimator.Hide(_binding, ref _sequence);
        }

        public void HandleOutcome(WheelOutcomeSnapshot snapshot)
        {
            WheelOutcomePopupContentApplier.Apply(_binding, snapshot);
            _motion = snapshot.Motion;
            _currentSnapshot = snapshot;
            _state.HasOutcome = snapshot.HasPresentableOutcome;
            SyncVisibility();
        }

        public void HandleHud(bool isOutcomePopupAllowed)
        {
            _state.PopupAllowed = isOutcomePopupAllowed;
            SyncVisibility();
        }

        public void MarkPresentationComplete()
        {
            _state.HasOutcome = false;
            WheelOutcomePopupAnimator.ClearMainIcon(_binding);
        }

        private void SyncVisibility()
        {
            int show = System.Convert.ToInt32(_state.HasOutcome && _state.PopupAllowed);
            WheelOutcomePopupVisibility.Apply(
                _binding,
                _motion,
                _tweenTarget,
                _currentSnapshot,
                show,
                _state.HasOutcome,
                ref _sequence);
        }
    }
}
