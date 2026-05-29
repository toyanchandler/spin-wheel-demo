using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardCardAnimator
    {
        public static void PlayFlipReveal(
            Object tweenTarget,
            WheelRewardCardBinding binding,
            int displayIndex,
            int visibleCardCount,
            bool featured,
            Color accentColor)
        {
            DOTween.Kill(tweenTarget);
            DOTween.Kill(binding.RootTransform);

            float foldDuration = featured ? WheelRewardCardMotion.FeaturedFoldDuration : WheelRewardCardMotion.StandardFoldDuration;
            float revealDuration = featured ? WheelRewardCardMotion.FeaturedRevealDuration : WheelRewardCardMotion.StandardRevealDuration;

            DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .AppendInterval(WheelRewardCardMotion.RevealDelay(displayIndex))
                .Append(binding.CanvasGroup.DOFade(1f, WheelRewardCardMotion.CanvasFadeDuration).SetEase(Ease.OutQuad))
                .Join(binding.RootTransform.DOScale(WheelRewardCardMotion.FoldScale, foldDuration).SetEase(Ease.OutCubic))
                .Join(binding.RootTransform.DOLocalRotate(featured ? WheelRewardCardMotion.FeaturedFoldRotation : WheelRewardCardMotion.StandardFoldRotation, foldDuration, RotateMode.Fast).SetEase(Ease.InCubic))
                .AppendCallback(binding.ShowFrontForFlip)
                .Append(binding.RootTransform.DOLocalRotate(featured ? WheelRewardCardMotion.FeaturedRevealRotation : WheelRewardCardMotion.StandardRevealRotation, revealDuration, RotateMode.Fast).SetEase(Ease.OutCubic))
                .Join(binding.RootTransform.DOScale(WheelRewardCardMotion.RevealPeakScale, revealDuration).SetEase(Ease.OutBack))
                .Join(binding.FrontImage.DOFade(1f, WheelRewardCardMotion.FrontFadeDuration))
                .Join(binding.IconImage.DOFade(1f, WheelRewardCardMotion.ContentFadeDuration))
                .Join(binding.TitleText.DOFade(1f, WheelRewardCardMotion.ContentFadeDuration))
                .Join(binding.AmountText.enabled ? binding.AmountText.DOFade(1f, WheelRewardCardMotion.ContentFadeDuration) : WheelUiTweenUtility.EmptyTween())
                .Join(PlayHalo(binding, featured, accentColor))
                .Join(binding.IconImage.transform.DOScale(Vector3.one, WheelRewardCardMotion.IconScaleSettleDuration).SetEase(Ease.OutBack))
                .Join(binding.TitleText.transform.DOScale(Vector3.one, WheelRewardCardMotion.TextScaleSettleDuration).SetEase(Ease.OutBack))
                .Join(binding.AmountText.transform.DOScale(Vector3.one, WheelRewardCardMotion.TextScaleSettleDuration).SetEase(Ease.OutBack))
                .Append(binding.RootTransform.DOLocalRotate(Vector3.zero, WheelRewardCardMotion.CardSettleDuration, RotateMode.Fast).SetEase(Ease.OutQuad))
                .Join(binding.RootTransform.DOScale(Vector3.one, WheelRewardCardMotion.CardSettleDuration).SetEase(Ease.OutQuad))
                .AppendInterval(WheelRewardCardMotion.RevealIdleDelay(displayIndex, visibleCardCount))
                .AppendCallback(() => PlayRevealIdleScale(tweenTarget, binding));
        }

        public static void PlayLandingPulse(Object tweenTarget, WheelRewardCardBinding binding, float delay)
        {
            DOTween.Kill(tweenTarget);

            DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(binding.RootTransform.DOScale(WheelRewardCardMotion.LandingPulseScale, WheelRewardCardMotion.LandingPulseScaleDuration).SetEase(Ease.OutQuad))
                .Join(binding.HaloImage.DOFade(WheelRewardCardMotion.LandingPulsePeakHaloAlpha, WheelRewardCardMotion.LandingPulseScaleDuration))
                .Append(binding.RootTransform.DOScale(Vector3.one, WheelRewardCardMotion.LandingPulseSettleDuration).SetEase(Ease.OutBack))
                .Join(binding.HaloImage.DOFade(WheelRewardCardMotion.LandingPulseSettleHaloAlpha, WheelRewardCardMotion.IconScaleSettleDuration));
        }

        private static void PlayRevealIdleScale(Object tweenTarget, WheelRewardCardBinding binding)
        {
            binding.RootTransform.localScale = Vector3.one;
            binding.RootTransform
                .DOScale(WheelRewardCardMotion.RevealIdleScale, WheelRewardCardMotion.RevealIdleScaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(tweenTarget)
                .SetUpdate(true);
        }

        private static Tween PlayHalo(WheelRewardCardBinding binding, bool featured, Color accentColor)
        {
            binding.PrepareHaloTimeline(accentColor);
            RectTransform haloRect = binding.HaloImage.rectTransform;
            float peakAlpha = featured ? WheelRewardCardMotion.FeaturedHaloPeakAlpha : WheelRewardCardMotion.StandardHaloPeakAlpha;
            float settleAlpha = featured ? WheelRewardCardMotion.FeaturedHaloSettleAlpha : WheelRewardCardMotion.StandardHaloSettleAlpha;
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(binding.HaloImage.DOFade(peakAlpha, WheelRewardCardMotion.HaloPeakFadeDuration))
                .Join(haloRect.DOScale(WheelRewardCardMotion.HaloPeakScale, WheelRewardCardMotion.HaloPeakScaleDuration).SetEase(Ease.OutQuad))
                .Join(haloRect.DOLocalRotate(WheelRewardCardMotion.HaloPeakRotation, WheelRewardCardMotion.HaloPeakRotateDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
                .Append(binding.HaloImage.DOFade(settleAlpha, WheelRewardCardMotion.HaloSettleDuration))
                .Join(haloRect.DOScale(WheelRewardCardMotion.HaloSettleScale, WheelRewardCardMotion.HaloSettleDuration).SetEase(Ease.OutCubic))
                .Join(haloRect.DOLocalRotate(WheelRewardCardMotion.HaloSettleRotation, WheelRewardCardMotion.HaloSettleDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        }
    }
}
