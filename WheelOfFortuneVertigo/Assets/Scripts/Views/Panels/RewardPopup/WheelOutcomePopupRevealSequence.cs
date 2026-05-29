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
            Sequence sequence = CreateRootSequence(binding, motion, tweenTarget);
            InsertChromeReveal(sequence, binding, snapshot);
            InsertRewardRevealStage(sequence, binding, snapshot);
            ScheduleRewardFlight(sequence, binding, tweenTarget, hasOutcome);
            return sequence;
        }

        private static Sequence CreateRootSequence(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            Object tweenTarget)
        {
            return DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .Append(binding.Root.FadeTo(1f, motion.FadeDuration))
                .Join(binding.ContentRoot.TweenHomeScale(motion.ScaleDuration, motion.ScaleEase));
        }

        private static void InsertChromeReveal(
            Sequence sequence,
            WheelOutcomePopupRefs binding,
            WheelOutcomeSnapshot snapshot)
        {
            sequence.Insert(0f, WheelOutcomePopupAnimator.PlayChromeReveal(binding, snapshot));
        }

        private static void InsertRewardRevealStage(
            Sequence sequence,
            WheelOutcomePopupRefs binding,
            WheelOutcomeSnapshot snapshot)
        {
            float stageStart = WheelOutcomePopupAnimationConfig.RevealStageStart;
            float finalAlpha = snapshot.Icon == null ? 0f : 1f;

            sequence
                .InsertCallback(stageStart, () => PrepareRewardReveal(binding, snapshot))
                .Insert(stageStart, binding.Icon.FadeTo(finalAlpha, WheelOutcomePopupAnimationConfig.IconFinalFadeDuration))
                .Insert(stageStart, binding.Icon.TweenScale(WheelOutcomePopupAnimationConfig.IconRewardSettleScale, WheelOutcomePopupAnimationConfig.IconPopDuration, Ease.OutBack))
                .Insert(stageStart, PlayFlash(binding, snapshot))
                .Insert(stageStart + WheelOutcomePopupAnimationConfig.TextFadeInDelay, binding.ResultText.FadeTo(1f, WheelOutcomePopupAnimationConfig.TextFadeInDuration))
                .Insert(stageStart, PlayShine(binding));
        }

        private static void ScheduleRewardFlight(
            Sequence sequence,
            WheelOutcomePopupRefs binding,
            Object tweenTarget,
            bool hasOutcome)
        {
            float flightStageStart = WheelOutcomePopupAnimationConfig.RevealStageStart
                + WheelOutcomePopupAnimationConfig.RewardHoldDuration;
            sequence.InsertCallback(
                flightStageStart,
                () => WheelOutcomePopupRewardFlight.Play(binding, tweenTarget, hasOutcome));
        }

        private static void PrepareRewardReveal(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            binding.Icon.ResetTransform(WheelOutcomePopupAnimationConfig.IconStartScale);
            binding.Icon.ApplySprite(
                snapshot.Icon,
                WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor),
                0f);

            if (snapshot.Phase == WheelGamePhase.Won && snapshot.Icon != null)
            {
                binding.RewardBurstParticle.Play(WheelOutcomePopupAnimationConfig.RewardBurstEmissionDuration);
            }
            else
            {
                binding.RewardBurstParticle.Clear();
            }
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
