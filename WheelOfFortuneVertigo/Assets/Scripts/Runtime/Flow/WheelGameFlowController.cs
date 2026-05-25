using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelGameFlowController : MonoBehaviour
    {
        private WheelEventBus _eventBus;

        private const float LandingResolveDelay = 0.42f;

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.SpinRequested += OnSpinRequested;
            _eventBus.LeaveRequested += OnLeaveRequested;
            _eventBus.RestartRequested += OnRestartRequested;
            WheelRuntimeLocator.Spinner.SpinCompleted += OnSpinCompleted;
        }

        public void Unbind()
        {
            if (_eventBus != null)
            {
                _eventBus.SpinRequested -= OnSpinRequested;
                _eventBus.LeaveRequested -= OnLeaveRequested;
                _eventBus.RestartRequested -= OnRestartRequested;
            }

            if (WheelRuntimeLocator.Spinner != null)
            {
                WheelRuntimeLocator.Spinner.SpinCompleted -= OnSpinCompleted;
            }

            DOTween.Kill(this);
            _eventBus = null;
        }

        private void OnSpinRequested()
        {
            WheelGameState state = WheelRuntimeLocator.State;
            int allowed = Convert.ToInt32(state.CanSpin && !WheelRuntimeLocator.Spinner.IsSpinning);
            FlowActions.Spin[allowed](this);
        }

        private void OnLeaveRequested()
        {
            FlowActions.Leave[Convert.ToInt32(WheelRuntimeLocator.State.CanLeave)](this);
        }

        private void OnRestartRequested()
        {
            FlowActions.Restart[Convert.ToInt32(WheelRuntimeLocator.State.CanRestart)](this);
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
            WheelGameState state = WheelRuntimeLocator.State;
            state.Resolve(result);
            WheelRuntimeLocator.Publisher.PublishOutcome(result, true);
            FlowActions.PublishAfterSpin[Convert.ToInt32(state.PhaseGameplay.PublishAllAfterSpin)](WheelRuntimeLocator.Publisher);
        }

        private void ExecuteSpin()
        {
            WheelGameState state = WheelRuntimeLocator.State;
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            state.PrepareCurrentZone();
            publisher.PublishZone();
            state.BeginSpin();
            publisher.PublishHud();
            WheelRuntimeLocator.Spinner.SetSlices(state.Slices, state.SliceCount);
            WheelRuntimeLocator.Spinner.Spin();
        }

        private void ExecuteLeave()
        {
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            WheelRuntimeLocator.State.CashOut();
            publisher.PublishHud();
            publisher.PublishOutcome(default(WheelSpinResult), false);
        }

        private void ExecuteRestart()
        {
            WheelRuntimeLocator.State.Restart();
            WheelRuntimeLocator.Publisher.PublishAll();
        }

        private static class FlowActions
        {
            public static readonly Action<WheelGameFlowController>[] Spin =
            {
                controller => { },
                controller => controller.ExecuteSpin()
            };

            public static readonly Action<WheelGameFlowController>[] Leave =
            {
                controller => { },
                controller => controller.ExecuteLeave()
            };

            public static readonly Action<WheelGameFlowController>[] Restart =
            {
                controller => { },
                controller => controller.ExecuteRestart()
            };

            public static readonly Action<WheelStatePublisher>[] PublishAfterSpin =
            {
                publisher => publisher.PublishHud(),
                publisher => publisher.PublishAll()
            };
        }
    }
}
