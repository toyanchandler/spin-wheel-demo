using Vertigo.Wheel.Runtime;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelLeaveButtonAction : WheelButtonAction
    {
        protected override bool IsVisible(WheelHudSnapshot snapshot) => snapshot.Actions.CanLeave;

        protected override bool IsInteractable(WheelHudSnapshot snapshot) => snapshot.Actions.CanLeave;

        protected override string ResolveLabel(WheelHudSnapshot snapshot) => snapshot.Actions.LeaveButtonLabel;

        protected override void Execute()
        {
            UiIntents.RequestLeaveConfirmation();
        }
    }
}
