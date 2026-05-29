using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    /// <summary>
    /// Outcome-popup DOTween flight choreography. Uses <see cref="WheelLootPresentationChannel"/> /
    /// <see cref="IWheelLootFlightHandler"/> for loot-panel coordination — not a duplicate handler API.
    /// </summary>
    internal static class WheelOutcomePopupRewardFlight
    {
        public static void Play(WheelOutcomePopupRefs binding, Object tweenTarget, bool hasOutcome)
        {
            if (!hasOutcome || binding.LootPresentation == null || binding.GetCurrentSnapshot == null)
            {
                CompletePresentation(binding);
                return;
            }

            WheelOutcomeSnapshot snapshot = binding.GetCurrentSnapshot();
            if (snapshot.Phase == WheelGamePhase.Bombed) return;

            if (snapshot.Phase != WheelGamePhase.Won || snapshot.RewardAmount <= 0 || snapshot.Icon == null)
            {
                CompletePresentation(binding);
                return;
            }

            binding.LootPresentation.HoldForArrival();
            PlayPopupIconFlight(binding, snapshot, tweenTarget);
        }

        private static void PlayPopupIconFlight(
            WheelOutcomePopupRefs binding,
            WheelOutcomeSnapshot snapshot,
            Object tweenTarget)
        {
            binding.Icon.KillTweens();
            binding.Icon.ApplySprite(snapshot.Icon, WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor), 1f);

            Vector3 target = binding.LootPresentation.ResolveLandingWorldPosition(snapshot.RewardId, 0, 1);
            DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .AppendInterval(WheelOutcomePopupAnimationConfig.RewardFlightDelay)
                .Append(binding.Icon.TweenWorldPosition(target, WheelOutcomePopupAnimationConfig.RewardFlightDuration, Ease.InOutQuad))
                .Join(binding.Icon.TweenScale(WheelOutcomePopupAnimationConfig.IconFlightEndScale, WheelOutcomePopupAnimationConfig.RewardFlightDuration, Ease.InOutQuad))
                .Join(binding.Icon.TweenLocalRotate(new Vector3(0f, 0f, WheelOutcomePopupAnimationConfig.RewardFlightRotation), WheelOutcomePopupAnimationConfig.RewardFlightDuration * 0.48f, Ease.InOutSine))
                .Join(binding.Icon.FadeTo(1f, WheelOutcomePopupAnimationConfig.RewardFlightDuration * WheelOutcomePopupAnimationConfig.RewardFlightFadeRatio))
                .AppendCallback(() => binding.LootPresentation.CommitPendingNow())
                .Append(binding.Icon.FadeTo(0f, WheelOutcomePopupAnimationConfig.IconFadeOutDuration))
                .Append(binding.Root.FadeTo(0f, WheelOutcomePopupAnimationConfig.PopupFadeOutDuration))
                .AppendCallback(() => CompletePresentation(binding));
        }

        private static void CompletePresentation(WheelOutcomePopupRefs binding)
        {
            WheelOutcomePopupAnimator.ClearMainIcon(binding);
            binding.Root.Hide();
            binding.MarkPresentationComplete?.Invoke();
        }
    }
}
