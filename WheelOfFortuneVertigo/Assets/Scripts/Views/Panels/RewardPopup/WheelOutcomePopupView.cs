using System;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed partial class WheelOutcomePopupView : MonoBehaviour
    {
        [WheelInject] private WheelEventBus _eventBus;
        [WheelInject] private WheelRewardPanelView _rewardPanelView;
        [WheelInject] private WheelOutcomePopupBindings _bindings;

        private WheelOutcomePopupPresenter _presenter;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            _bindings.CaptureHome();
            _presenter = new WheelOutcomePopupPresenter(
                _bindings.CreateRefs(
                    _rewardPanelView,
                    () => _presenter?.CurrentSnapshot ?? default,
                    () => _presenter?.MarkPresentationComplete()),
                this);
            _presenter.Reset();
            _eventBus.OutcomeResolved += OnOutcomeResolved;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.OutcomeResolved -= OnOutcomeResolved;
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _presenter?.Reset();
            _presenter = null;
        }

        private void OnOutcomeResolved(WheelOutcomeSnapshot snapshot)
        {
            _presenter.HandleOutcome(snapshot);
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _presenter.HandleHud(snapshot.IsOutcomePopupAllowed);
        }

        private void ValidateWiring()
        {
            if (_eventBus == null || _rewardPanelView == null || _bindings == null)
            {
                throw new InvalidOperationException(
                    name + " outcome popup dependencies are incomplete. Add WheelOutcomePopupBindings to the popup controller.");
            }

            _bindings.Validate();
        }
    }
}
