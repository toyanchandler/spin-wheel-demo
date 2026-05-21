using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelSpinButtonAction : WheelButtonAction
    {
        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            return snapshot.CanSpin;
        }

        protected override void Execute()
        {
            EventBus.RequestSpin();
        }
    }
}
