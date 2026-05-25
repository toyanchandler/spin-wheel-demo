using Vertigo.Wheel.Runtime;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRestartButtonAction : WheelButtonAction
    {
        protected override bool IsVisible(WheelHudSnapshot snapshot)
        {
            if (name.Contains("reward_opening"))
            {
                return snapshot.Phase == WheelGamePhase.CashedOut;
            }

            if (name == "ui_button_restart")
            {
                return false;
            }

            return snapshot.Phase == WheelGamePhase.Bombed;
        }

        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            return snapshot.CanRestart && IsVisible(snapshot);
        }

        protected override void Execute()
        {
            EventBus.RequestRestart();
        }
    }
}
