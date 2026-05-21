using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelLeaveButtonAction : WheelButtonAction
    {
        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            return snapshot.CanLeave;
        }

        protected override void Execute()
        {
            EventBus.RequestLeave();
        }
    }
}
