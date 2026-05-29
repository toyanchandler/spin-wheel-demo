using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class WheelExitConfirmationView : MonoBehaviour
    {
        [SerializeField] private WheelExitConfirmationBindings _bindings;

        [WheelInject] private IWheelUiIntentPublisher _uiIntents;
        [WheelInject] private IWheelUiIntentSubscriber _uiIntentEvents;
        [WheelInject] private IWheelSnapshotSubscriber _snapshots;

        private Sequence _sequence;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            _uiIntentEvents.LeaveConfirmationRequested += Show;
            _snapshots.HudStateChanged += OnHudStateChanged;
            _bindings.AddButtonListeners(CollectRewards, ComeBack);
            HideImmediate();
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _uiIntentEvents.LeaveConfirmationRequested -= Show;
            _snapshots.HudStateChanged -= OnHudStateChanged;
            _bindings.RemoveButtonListeners(CollectRewards, ComeBack);
            KillSequence();
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _bindings.ApplyCopy(snapshot);
            if (_bindings.IsVisible && !snapshot.Actions.CanLeave) HideImmediate();
        }

        private void Show()
        {
            KillSequence();
            _sequence = WheelExitConfirmationAnimator.PlayShow(_bindings).SetTarget(this);
        }

        private void HideImmediate()
        {
            KillSequence();
            _bindings.HideImmediate();
        }

        private void CollectRewards()
        {
            _uiIntents.RequestLeave();
        }

        private void ComeBack()
        {
            HideImmediate();
        }

        private void KillSequence()
        {
            WheelUiTweenUtility.Kill(ref _sequence);
        }

        private void ValidateWiring()
        {
            if (_uiIntents == null || _uiIntentEvents == null || _snapshots == null)
            {
                throw new InvalidOperationException(name + " requires bound event bus capability interfaces.");
            }

            _bindings ??= GetComponent<WheelExitConfirmationBindings>();
            if (_bindings == null)
            {
                throw new InvalidOperationException(
                    name + " exit confirmation dependencies are incomplete. Add WheelExitConfirmationBindings on the same GameObject.");
            }

            _bindings.Validate();
        }
    }
}
