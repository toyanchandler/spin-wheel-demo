using DG.Tweening;
using UnityEngine;
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

        protected override void PlayClickFeedback()
        {
            base.PlayClickFeedback();
            if (Button == null || Button.image == null)
            {
                return;
            }

            ImageGlowPulse(Button.image);
        }

        private void ImageGlowPulse(UnityEngine.UI.Image image)
        {
            image.DOKill();
            Color baseColor = image.color;
            DOTween.Sequence()
                .SetTarget(image)
                .SetUpdate(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .Append(image.DOColor(new Color(0.88f, 1f, 1f, 1f), 0.11f).SetEase(Ease.OutQuad))
                .Append(image.DOColor(baseColor, 0.18f).SetEase(Ease.OutCubic));
        }
    }
}
