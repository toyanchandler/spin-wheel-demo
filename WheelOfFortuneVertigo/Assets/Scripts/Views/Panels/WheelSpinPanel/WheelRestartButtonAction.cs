using Vertigo.Wheel.Runtime;
using Vertigo.Wheel.Data;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRestartButtonAction : WheelButtonAction
    {
        [SerializeField] private WheelRestartButtonRole _role = WheelRestartButtonRole.OutcomeRetry;

        protected override bool IsVisible(WheelHudSnapshot snapshot)
        {
            if (_role == WheelRestartButtonRole.RewardOpening)
            {
                return snapshot.Phase == WheelGamePhase.CashedOut;
            }

            if (_role == WheelRestartButtonRole.HiddenMain)
            {
                return false;
            }

            return snapshot.Phase == WheelGamePhase.Bombed;
        }

        protected override bool IsInteractable(WheelHudSnapshot snapshot)
        {
            return snapshot.CanRestart && IsVisible(snapshot);
        }

        protected override string ResolveLabel(WheelHudSnapshot snapshot)
        {
            return snapshot.RestartButtonLabel;
        }

        protected override void Execute()
        {
            EventBus.RequestRestart();
        }
    }

    public enum WheelRestartButtonRole
    {
        OutcomeRetry = 0,
        HiddenMain = 1,
        RewardOpening = 2
    }
}
