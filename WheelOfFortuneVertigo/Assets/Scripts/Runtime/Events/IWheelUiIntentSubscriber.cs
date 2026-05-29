using System;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Subscribe to UI intents raised by views. Gameplay flow should depend on this, not intent publishers.</summary>
    public interface IWheelUiIntentSubscriber
    {
        event Action SpinRequested;
        event Action LeaveConfirmationRequested;
        event Action LeaveRequested;
        event Action RestartRequested;
    }
}
