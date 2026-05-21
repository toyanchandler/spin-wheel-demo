using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelStatePublisher : MonoBehaviour
    {
        private WheelEventBus _eventBus;

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void Unbind()
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
            _eventBus.RaiseZoneChanged(WheelSnapshotFactory.CreateZone(WheelRuntimeLocator.State));
        }

        public void PublishHud()
        {
            _eventBus.RaiseHudState(WheelSnapshotFactory.CreateHud(WheelRuntimeLocator.State));
        }

        public void PublishOutcome(WheelSpinResult result, bool hasResult)
        {
            WheelGameState state = WheelRuntimeLocator.State;
            _eventBus.RaiseOutcome(WheelSnapshotFactory.CreateOutcome(
                state.Phase,
                result,
                hasResult,
                state.Inventory,
                state.Settings));
        }
    }
}
