using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelLeaveButtonAction : WheelButtonAction
    {
        [SerializeField] private WheelLeaveButtonRole _role = WheelLeaveButtonRole.Leave;

        protected override bool IsVisible(WheelHudSnapshot snapshot)
        {
            if (_role == WheelLeaveButtonRole.OutcomeExit)
            {
                return false;
            }

            return snapshot.CanLeave;
        }

        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            if (_role == WheelLeaveButtonRole.OutcomeExit && snapshot.Phase == WheelGamePhase.Bombed)
            {
                return false;
            }

            return snapshot.CanLeave;
        }

        protected override string ResolveLabel(WheelHudSnapshot snapshot)
        {
            return snapshot.LeaveButtonLabel;
        }

        protected override void Execute()
        {
            EventBus.RequestLeaveConfirmation();
        }
    }

    public enum WheelLeaveButtonRole
    {
        Leave = 0,
        OutcomeExit = 1
    }
}
