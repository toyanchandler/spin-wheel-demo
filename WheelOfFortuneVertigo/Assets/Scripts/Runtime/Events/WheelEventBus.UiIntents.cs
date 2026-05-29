using System;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>UI → gameplay intents. Handled by <see cref="WheelGameFlowController"/> or confirmation views.</summary>
    public sealed partial class WheelEventBus
    {
        public event Action SpinRequested;
        public event Action LeaveConfirmationRequested;
        public event Action LeaveRequested;
        public event Action RestartRequested;

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

        private void ClearUiIntents()
        {
            SpinRequested = null;
            LeaveConfirmationRequested = null;
            LeaveRequested = null;
            RestartRequested = null;
        }
    }
}
