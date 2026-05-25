using System;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelEventBus
    {
        public event Action SpinRequested;
        public event Action LeaveConfirmationRequested;
        public event Action LeaveRequested;
        public event Action RestartRequested;
        public event Action<WheelZoneSnapshot> ZoneChanged;
        public event Action<WheelHudSnapshot> HudStateChanged;
        public event Action<WheelSpinResult> SpinLanded;
        public event Action<WheelOutcomeSnapshot> OutcomeResolved;

        public void RequestSpin()
        {
            SpinRequested?.Invoke();
        }

        public void RequestLeaveConfirmation()
        {
            LeaveConfirmationRequested?.Invoke();
        }

        public void RequestLeave()
        {
            LeaveRequested?.Invoke();
        }

        public void RequestRestart()
        {
            RestartRequested?.Invoke();
        }

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

        public void Clear()
        {
            SpinRequested = null;
            LeaveConfirmationRequested = null;
            LeaveRequested = null;
            RestartRequested = null;
            ZoneChanged = null;
            HudStateChanged = null;
            SpinLanded = null;
            OutcomeResolved = null;
        }
    }
}
