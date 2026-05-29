namespace Vertigo.Wheel.Runtime
{
    /// <summary>UI → gameplay intent requests. Views and buttons should depend on this, not snapshot publishers.</summary>
    public interface IWheelUiIntentPublisher
    {
        void RequestSpin();
        void RequestLeaveConfirmation();
        void RequestLeave();
        void RequestRestart();
    }
}
