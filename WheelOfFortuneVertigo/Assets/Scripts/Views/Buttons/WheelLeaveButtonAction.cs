using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelLeaveButtonAction : WheelButtonAction
    {
        protected override bool IsVisible(WheelHudSnapshot snapshot)
        {
            if (name == "ui_button_outcome_exit")
            {
                return false;
            }

            return snapshot.CanLeave;
        }

        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            if (name == "ui_button_outcome_exit" && snapshot.Phase == WheelGamePhase.Bombed)
            {
                return false;
            }

            return snapshot.CanLeave;
        }

        protected override void Execute()
        {
            EventBus.RequestLeaveConfirmation();
        }
    }
}
