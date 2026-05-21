using System;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelGameFlowController : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private WheelGameState _state;
        [SerializeField] private WheelSpinner _spinner;
        [SerializeField] private WheelStatePublisher _publisher;

        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.SpinRequested += OnSpinRequested;
            _eventBus.LeaveRequested += OnLeaveRequested;
            _eventBus.RestartRequested += OnRestartRequested;
            _spinner.SpinCompleted += OnSpinCompleted;
        }

        public void Dispose()
        {
            _eventBus.SpinRequested -= OnSpinRequested;
            _eventBus.LeaveRequested -= OnLeaveRequested;
            _eventBus.RestartRequested -= OnRestartRequested;
            _spinner.SpinCompleted -= OnSpinCompleted;
            _eventBus = null;
        }

        private void OnSpinRequested()
        {
            int allowed = Convert.ToInt32(_state.CanSpin && !_spinner.IsSpinning);
            FlowActions.Spin[allowed](this);
        }

        private void OnLeaveRequested()
        {
            FlowActions.Leave[Convert.ToInt32(_state.CanLeave)](this);
        }

        private void OnRestartRequested()
        {
            FlowActions.Restart[Convert.ToInt32(_state.CanRestart)](this);
        }

        private void OnSpinCompleted(WheelSpinResult result)
        {
            _state.Resolve(result);
            FlowActions.PublishAfterSpin[Convert.ToInt32(_state.PhaseGameplay.publishAllAfterSpin)](_publisher);
            _publisher.PublishOutcome(result, true);
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
