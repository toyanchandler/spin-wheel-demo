using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRestartButtonAction : WheelButtonAction
    {
        [UnityEngine.SerializeField] private WheelRestartButtonRole _role = WheelRestartButtonRole.OutcomeRetry;

        protected override bool IsVisible(WheelHudSnapshot snapshot)
        {
            if (_role == WheelRestartButtonRole.RewardOpening) return snapshot.Actions.ShowRewardOpeningRestart;
            if (_role == WheelRestartButtonRole.HiddenMain) return false;
            return snapshot.Actions.ShowOutcomeRetryRestart;
        }

        protected override bool IsInteractable(WheelHudSnapshot snapshot) => snapshot.Actions.CanRestart && IsVisible(snapshot);

        protected override string ResolveLabel(WheelHudSnapshot snapshot) => snapshot.Actions.RestartButtonLabel;

        protected override void Execute()
        {
            UiIntents.RequestRestart();
        }
    }

    public enum WheelRestartButtonRole
    {
        OutcomeRetry = 0,
        HiddenMain = 1,
        RewardOpening = 2
    }
}
