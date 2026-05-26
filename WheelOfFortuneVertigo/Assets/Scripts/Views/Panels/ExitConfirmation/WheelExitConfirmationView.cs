using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class WheelExitConfirmationView : MonoBehaviour
    {
        [WheelInject] private WheelEventBus _eventBus;
        [WheelInject] private WheelExitConfirmationBindings _bindings;

        private Sequence _sequence;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            _eventBus.LeaveConfirmationRequested += Show;
            _eventBus.HudStateChanged += OnHudStateChanged;
            _bindings.AddButtonListeners(CollectRewards, ComeBack);
            HideImmediate();
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.LeaveConfirmationRequested -= Show;
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _bindings.RemoveButtonListeners(CollectRewards, ComeBack);
            KillSequence();
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _bindings.ApplyCopy(snapshot);
            if (_bindings.IsVisible && !snapshot.Actions.CanLeave)
            {
                HideImmediate();
            }
        }

        private void Show()
        {
            KillSequence();
            _sequence = _bindings.Show();
        }

        private void ComeBack()
        {
            KillSequence();
            _sequence = _bindings.HideAnimated(HideImmediate);
        }

        private void CollectRewards()
        {
            HideImmediate();
            _eventBus.RequestLeave();
        }

        private void HideImmediate()
        {
            KillSequence();
            _bindings.HideImmediate();
        }

        private void OnDisable()
        {
            KillSequence();
        }

        private void ValidateWiring()
        {
            if (_eventBus == null || _bindings == null)
            {
                throw new System.InvalidOperationException(
                    name + " exit confirmation dependencies are incomplete. Add WheelExitConfirmationBindings to the overlay.");
            }

            _bindings.Validate();
        }

        private void KillSequence()
        {
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }
        }
    }
}
