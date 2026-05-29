using System;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    /// <summary>Outcome popup lifecycle only — presentation lives in <see cref="WheelOutcomePopupPresenter"/>.</summary>
    [WheelBind]
    public sealed class WheelOutcomePopupView : MonoBehaviour
    {
        [SerializeField] private WheelOutcomePopupBindings _bindings;

        [WheelInject] private WheelEventBus _eventBus;

        private WheelOutcomePopupPresenter _presenter;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            _bindings.CaptureHome();
            _presenter = new WheelOutcomePopupPresenter(
                _bindings.CreateRefs(
                    _eventBus.Presentation.Loot,
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
            _presenter.HandleHud(snapshot.Actions.IsOutcomePopupAllowed);
        }

        private void ValidateWiring()
        {
            if (_eventBus == null)
            {
                throw new InvalidOperationException(name + " requires a bound WheelEventBus.");
            }

            _bindings ??= GetComponent<WheelOutcomePopupBindings>();
            if (_bindings == null)
            {
                throw new InvalidOperationException(
                    name + " outcome popup dependencies are incomplete. Add WheelOutcomePopupBindings on the same GameObject.");
            }

            _bindings.Validate();
        }
    }
}
