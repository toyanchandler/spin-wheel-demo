using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Maps UI intents to state changes and snapshot publishing.
    ///
    /// Player journeys (see ARCHITECTURE_AND_LOGIC.md):
    /// - Spin  → ExecuteSpin → spinner → SpinLanded → resolve → publish
    /// - Leave → ExecuteLeave (cash out, no spin result)
    /// - Restart → ExecuteRestart
    /// </summary>
    public sealed class WheelGameFlowController : MonoBehaviour
    {
        private static readonly WheelSliceDefinition[] EmptySpinSlices = new WheelSliceDefinition[0];

        private IWheelUiIntentSubscriber _uiIntents;
        private IWheelSnapshotPublisher _snapshots;
        private WheelGameState _state;
        private WheelStatePublisher _publisher;
        private IWheelSpinDriver _spinner;
        private WheelSliceDefinition[] _spinSliceBuffer = EmptySpinSlices;

        public void Bind(
            WheelEventBus eventBus,
            WheelGameState state,
            WheelStatePublisher publisher,
            IWheelSpinDriver spinner)
        {
            _uiIntents = eventBus;
            _snapshots = eventBus;
            _state = state;
            _publisher = publisher;
            _spinner = spinner;

            _uiIntents.SpinRequested += OnSpinRequested;
            _uiIntents.LeaveRequested += OnLeaveRequested;
            _uiIntents.RestartRequested += OnRestartRequested;
            _spinner.LandingStarted += OnLandingStarted;
            _spinner.SpinCompleted += OnSpinCompleted;
        }

        public void Unbind()
        {
            if (_uiIntents != null)
            {
                _uiIntents.SpinRequested -= OnSpinRequested;
                _uiIntents.LeaveRequested -= OnLeaveRequested;
                _uiIntents.RestartRequested -= OnRestartRequested;
            }

            if (_spinner != null)
            {
                _spinner.LandingStarted -= OnLandingStarted;
                _spinner.SpinCompleted -= OnSpinCompleted;
            }

            _uiIntents = null;
            _snapshots = null;
            _state = null;
            _publisher = null;
            _spinner = null;
        }

        /// <summary>Editor/debug: same landing + resolve path as a finished spin, without tweens.</summary>
        public void ForceResolveOutcome(WheelSpinResult result)
        {
            if (!IsBound())
            {
                return;
            }

            OnLandingStarted(result);
            ResolveCompletedSpin(result);
        }

        private bool IsBound()
        {
            return _uiIntents != null && _snapshots != null && _state != null && _publisher != null;
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

        private void OnLandingStarted(WheelSpinResult result)
        {
            _snapshots.RaiseSpinLanded(result);
        }

        private void OnSpinCompleted(WheelSpinResult result)
        {
            ResolveCompletedSpin(result);
        }

        private void ResolveCompletedSpin(WheelSpinResult result)
        {
            _state.Resolve(result);
            _publisher.PublishOutcome(result, hasResult: true);
            PublishAfterSpin();
        }

        private void PublishAfterSpin()
        {
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

            int sliceIndex = _state.SelectSpinSliceIndex();
            _state.BeginSpin();
            _publisher.PublishHud();

            SupplySpinSlicesToSpinner();
            _spinner.Spin(sliceIndex);
        }

        private void SupplySpinSlicesToSpinner()
        {
            EnsureSpinSliceBuffer(_state.SliceCount);
            int count = _state.CopySliceDefinitions(_spinSliceBuffer);
            _spinner.AcceptSlices(_spinSliceBuffer, count);
        }

        private void EnsureSpinSliceBuffer(int capacity)
        {
            if (_spinSliceBuffer.Length == capacity && _spinSliceBuffer.Length > 0 && _spinSliceBuffer[0] != null)
            {
                return;
            }

            _spinSliceBuffer = new WheelSliceDefinition[capacity];
            for (int i = 0; i < capacity; i++)
            {
                _spinSliceBuffer[i] = new WheelSliceDefinition();
            }
        }

        private void ExecuteLeave()
        {
            _state.CashOut();
            _publisher.PublishHud();
            _publisher.PublishOutcome(default(WheelSpinResult), hasResult: false);
        }

        private void ExecuteRestart()
        {
            _state.Restart();
            _publisher.PublishAll();
        }
    }
}
