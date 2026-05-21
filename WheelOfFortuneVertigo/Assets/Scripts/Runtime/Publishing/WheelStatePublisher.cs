using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelStatePublisher : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private WheelGameState _state;
        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Dispose()
        {
            _eventBus = null;
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
