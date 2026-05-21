using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRestartButtonAction : WheelButtonAction
    {
        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            return snapshot.CanRestart;
        }

        protected override void Execute()
        {
            EventBus.RequestRestart();
        }
    }
}
