using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelSpinButtonAction : WheelButtonAction
    {
        protected override bool ShouldPlayIdlePulse => true;

        protected override bool IsInteractable(WheelHudSnapshot snapshot) => snapshot.Actions.CanSpin;

        protected override string ResolveLabel(WheelHudSnapshot snapshot) => snapshot.Actions.SpinButtonLabel;

        protected override void Execute()
        {
            UiIntents.RequestSpin();
        }

        protected override void PlayClickFeedback()
        {
            base.PlayClickFeedback();
            if (Button.image != null)
            {
                WheelButtonFeedbackAnimator.PlayImageGlow(Button.image, this);
            }
        }
    }
}
