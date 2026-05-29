using System;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Gameplay → UI snapshot events. Raised only by <see cref="WheelStatePublisher"/> and flow after resolve.</summary>
    public sealed partial class WheelEventBus
    {
        public event Action<WheelZoneSnapshot> ZoneChanged;
        public event Action<WheelHudSnapshot> HudStateChanged;
        public event Action<WheelSpinResult> SpinLanded;
        public event Action<WheelOutcomeSnapshot> OutcomeResolved;

        public void RaiseZoneChanged(WheelZoneSnapshot snapshot)
        {
            ZoneChanged?.Invoke(snapshot);
        }

        public void RaiseHudState(WheelHudSnapshot snapshot)
        {
            HudStateChanged?.Invoke(snapshot);
        }

        public void RaiseSpinLanded(WheelSpinResult result)
        {
            SpinLanded?.Invoke(result);
        }

        public void RaiseOutcome(WheelOutcomeSnapshot snapshot)
        {
            OutcomeResolved?.Invoke(snapshot);
        }

        private void ClearSnapshotEvents()
        {
            ZoneChanged = null;
            HudStateChanged = null;
            SpinLanded = null;
            OutcomeResolved = null;
        }
    }
}
