using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupRevealSequence
    {
        public static Sequence Build(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            Object tweenTarget,
            WheelOutcomeSnapshot snapshot,
            bool hasOutcome)
        {
            float flightStageStart = WheelOutcomePopupAnimationConfig.RevealStageStart
                + WheelOutcomePopupAnimationConfig.RewardHoldDuration;
            Sprite icon = snapshot.Icon;
            float finalAlpha = icon == null ? 0f : 1f;

            Sequence sequence = DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .Append(binding.Root.FadeTo(1f, motion.FadeDuration))
                .Join(binding.ContentRoot.TweenHomeScale(motion.ScaleDuration, motion.ScaleEase))
                .Insert(0f, WheelOutcomePopupAnimator.PlayChromeReveal(binding, snapshot))
                .InsertCallback(WheelOutcomePopupAnimationConfig.RevealStageStart, () => PrepareRewardReveal(binding, snapshot))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, binding.Icon.FadeTo(finalAlpha, WheelOutcomePopupAnimationConfig.IconFinalFadeDuration))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, binding.Icon.TweenScale(WheelOutcomePopupAnimationConfig.IconRewardSettleScale, WheelOutcomePopupAnimationConfig.IconPopDuration, Ease.OutBack))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, PlayFlash(binding, snapshot))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart + WheelOutcomePopupAnimationConfig.TextFadeInDelay, binding.ResultText.FadeTo(1f, WheelOutcomePopupAnimationConfig.TextFadeInDuration))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, PlayShine(binding))
                .InsertCallback(flightStageStart, () => WheelOutcomePopupRewardFlight.Play(binding, tweenTarget, hasOutcome));

            return sequence;
        }

        private static void PrepareRewardReveal(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            binding.Icon.ResetTransform(WheelOutcomePopupAnimationConfig.IconStartScale);
            binding.Icon.ApplySprite(
                snapshot.Icon,
                WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor),
                0f);
            binding.RewardBurstParticle.PlayFor(snapshot);
        }

        private static Tween PlayFlash(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            binding.Flash.Image.color = WheelUiGraphicUtility.WithAlpha(WheelOutcomePopupPalette.FlashColor(snapshot), 0f);
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(binding.Flash.FadeTo(WheelOutcomePopupPalette.FlashAlpha(snapshot), WheelOutcomePopupAnimationConfig.FlashFadeInDuration))
                .Append(binding.Flash.FadeTo(0f, WheelOutcomePopupAnimationConfig.FlashFadeOutDuration));
        }

        private static Tween PlayShine(WheelOutcomePopupRefs binding)
        {
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(binding.Shine.FadeTo(WheelOutcomePopupAnimationConfig.ShineAlpha, WheelOutcomePopupAnimationConfig.ShineFadeInDuration))
                .AppendInterval(WheelOutcomePopupAnimationConfig.ShineHoldDuration)
                .Append(binding.Shine.FadeTo(0f, WheelOutcomePopupAnimationConfig.ShineFadeOutDuration));
        }
    }
}
