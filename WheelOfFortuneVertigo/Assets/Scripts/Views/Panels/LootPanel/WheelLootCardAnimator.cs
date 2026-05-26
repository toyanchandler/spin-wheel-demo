using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelLootCardAnimator
    {
        public static void PlayEntrance(Object tweenTarget, WheelLootCardBinding binding, int displayIndex)
        {
            DOTween.Kill(tweenTarget);
            binding.RootTransform.DOKill();

            Sequence sequence = DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .AppendInterval(WheelLootCardMotion.EntranceDelay(displayIndex))
                .Append(binding.CanvasGroup.DOFade(1f, WheelLootCardMotion.EntranceFadeDuration))
                .Join(binding.RootTransform.DOScale(Vector3.one, WheelLootCardMotion.EntranceScaleDuration).SetEase(Ease.OutBack))
                .Join(binding.RootTransform.DOLocalRotate(Vector3.zero, WheelLootCardMotion.EntranceRotateDuration, RotateMode.Fast).SetEase(Ease.OutCubic))
                .Join(GlowPulse(binding))
                .Append(PlayShimmer(binding, WheelLootCardMotion.EntranceShimmerDelay, WheelLootCardMotion.EntranceShimmerDuration));

            sequence.Join(binding.IconImage.transform.DOScale(Vector3.one, WheelLootCardMotion.IconEntranceScaleDuration).SetEase(Ease.OutBack));
        }

        public static void PlayLandingPulse(Object tweenTarget, WheelLootCardBinding binding, float delay)
        {
            DOTween.Kill(tweenTarget);

            DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(binding.RootTransform.DOScale(WheelLootCardMotion.LandingPulseScale, WheelLootCardMotion.LandingPulseScaleDuration).SetEase(Ease.OutQuad))
                .Join(binding.GlowImage.DOFade(WheelLootCardMotion.LandingPulsePeakGlowAlpha, WheelLootCardMotion.LandingPulseScaleDuration))
                .Append(binding.RootTransform.DOScale(Vector3.one, WheelLootCardMotion.LandingPulseSettleDuration).SetEase(Ease.OutBack))
                .Join(binding.GlowImage.DOFade(WheelLootCardMotion.LandingPulseSettleGlowAlpha, WheelLootCardMotion.LandingPulseGlowSettleDuration))
                .Join(PlayShimmer(binding, WheelLootCardMotion.LandingPulseShimmerDelay, WheelLootCardMotion.LandingPulseShimmerDuration));
        }

        private static Tween GlowPulse(WheelLootCardBinding binding)
        {
            binding.PrepareGlowTimeline();
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(binding.GlowImage.DOFade(WheelLootCardMotion.GlowPulsePeakAlpha, WheelLootCardMotion.GlowPulseFadeInDuration))
                .Join(binding.GlowImage.rectTransform.DOScale(WheelLootCardMotion.GlowPulseScale, WheelLootCardMotion.GlowPulseScaleDuration).SetEase(Ease.OutQuad))
                .Append(binding.GlowImage.DOFade(WheelLootCardMotion.RestingGlowAlpha, WheelLootCardMotion.GlowPulseSettleDuration))
                .Join(binding.GlowImage.rectTransform.DOScale(Vector3.one, WheelLootCardMotion.GlowPulseSettleDuration).SetEase(Ease.OutCubic));
        }

        private static Tween PlayShimmer(WheelLootCardBinding binding, float delay, float duration)
        {
            WheelLootCardBinding.TweenState state = binding.CaptureShimmerState();
            RectTransform shineRect = binding.ShineImage.rectTransform;
            return DOTween.Sequence()
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(binding.ShineImage.DOFade(WheelLootCardMotion.ShimmerAlpha, WheelLootCardMotion.ShimmerFadeInDuration))
                .Join(shineRect.DOAnchorPos(state.Home + WheelLootCardMotion.ShimmerEndOffset(state.Travel), duration).SetEase(Ease.InOutSine))
                .Append(binding.ShineImage.DOFade(0f, WheelLootCardMotion.ShimmerFadeOutDuration))
                .OnComplete(binding.CompleteShimmer);
        }
    }
}
