using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelStatePublisher : MonoBehaviour
    {
        private WheelEventBus _eventBus;
        private WheelGameState _state;

        public void Bind(WheelEventBus eventBus, WheelGameState state)
        {
            _eventBus = eventBus;
            _state = state;
        }

        public void Unbind()
        {
            _eventBus = null;
            _state = null;
        }

        public void PublishAll()
        {
            PublishZone();
            PublishHud();
        }

        public void PublishZone()
        {
            _eventBus.RaiseZoneChanged(WheelSnapshotFactory.CreateZone(_state));
        }

        public void PublishHud()
        {
            _eventBus.RaiseHudState(WheelSnapshotFactory.CreateHud(_state));
        }

        public void PublishOutcome(WheelSpinResult result, bool hasResult)
        {
            _eventBus.RaiseOutcome(WheelSnapshotFactory.CreateOutcome(
                _state.Phase,
                result,
                hasResult,
                _state.Inventory,
                _state.Settings));
        }
    }
}
