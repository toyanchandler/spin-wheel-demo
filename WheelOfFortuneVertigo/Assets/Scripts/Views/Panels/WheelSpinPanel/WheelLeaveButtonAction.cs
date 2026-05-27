using Vertigo.Wheel.Runtime;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelLeaveButtonAction : WheelButtonAction
    {
        protected override bool IsVisible(WheelHudSnapshot snapshot)
        {
            return snapshot.Actions.CanLeave;
        }

        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            return snapshot.Actions.CanLeave;
        }

        protected override string ResolveLabel(WheelHudSnapshot snapshot)
        {
            return snapshot.Actions.LeaveButtonLabel;
        }

        protected override void Execute()
        {
            EventBus.RequestLeaveConfirmation();
        }
    }
}
