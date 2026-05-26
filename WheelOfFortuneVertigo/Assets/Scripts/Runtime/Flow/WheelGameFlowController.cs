using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelGameFlowController : MonoBehaviour
    {
        private WheelEventBus _eventBus;
        private WheelGameState _state;
        private WheelStatePublisher _publisher;
        private WheelSpinner _spinner;

        private const float LandingResolveDelay = 0.42f;

        public void Bind(
            WheelEventBus eventBus,
            WheelGameState state,
            WheelStatePublisher publisher,
            WheelSpinner spinner)
        {
            _eventBus = eventBus;
            _state = state;
            _publisher = publisher;
            _spinner = spinner;
            _eventBus.SpinRequested += OnSpinRequested;
            _eventBus.LeaveRequested += OnLeaveRequested;
            _eventBus.RestartRequested += OnRestartRequested;
            _spinner.SpinCompleted += OnSpinCompleted;
        }

        public void Unbind()
        {
            if (_eventBus != null)
            {
                _eventBus.SpinRequested -= OnSpinRequested;
                _eventBus.LeaveRequested -= OnLeaveRequested;
                _eventBus.RestartRequested -= OnRestartRequested;
            }

            if (_spinner != null)
            {
                _spinner.SpinCompleted -= OnSpinCompleted;
            }

            DOTween.Kill(this);
            _eventBus = null;
            _state = null;
            _publisher = null;
            _spinner = null;
        }

        private void OnSpinRequested()
        {
            if (!_state.CanSpin || _spinner.IsSpinning)
            {
                return;
            }

            ExecuteSpin();
        }

        private void OnLeaveRequested()
        {
            if (!_state.CanLeave)
            {
                return;
            }

            ExecuteLeave();
        }

        private void OnRestartRequested()
        {
            if (!_state.CanRestart)
            {
                return;
            }

            ExecuteRestart();
        }

        private void OnSpinCompleted(WheelSpinResult result)
        {
            _eventBus.RaiseSpinLanded(result);
            DOVirtual.DelayedCall(LandingResolveDelay, () => ResolveCompletedSpin(result), false)
                .SetTarget(this)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void ResolveCompletedSpin(WheelSpinResult result)
        {
            _state.Resolve(result);
            _publisher.PublishOutcome(result, true);

            if (_state.PhaseGameplay.PublishAllAfterSpin)
            {
                _publisher.PublishAll();
            }
            else
            {
                _publisher.PublishHud();
            }
        }

        private void ExecuteSpin()
        {
            _state.PrepareCurrentZone();
            _publisher.PublishZone();
            _state.BeginSpin();
            _publisher.PublishHud();
            _spinner.SetSlices(_state.Slices, _state.SliceCount);
            _spinner.Spin();
        }

        private void ExecuteLeave()
        {
            _state.CashOut();
            _publisher.PublishHud();
            _publisher.PublishOutcome(default(WheelSpinResult), false);
        }

        private void ExecuteRestart()
        {
            _state.Restart();
            _publisher.PublishAll();
        }
    }
}
