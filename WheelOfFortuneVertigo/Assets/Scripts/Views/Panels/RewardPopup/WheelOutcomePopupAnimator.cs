using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupAnimator
    {
        public static void Hide(WheelOutcomePopupRefs binding, ref Sequence sequence)
        {
            WheelUiTweenUtility.Kill(ref sequence);
            binding.RewardBurstParticle.Clear();
            ClearMainIcon(binding);
            binding.Root.Hide();
        }

        public static void ClearMainIcon(WheelOutcomePopupRefs binding)
        {
            binding.Icon.Clear();
        }

        public static void Show(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            Object tweenTarget,
            WheelOutcomeSnapshot snapshot,
            bool hasOutcome,
            ref Sequence sequence)
        {
            if (binding.Root.IsVisible)
            {
                return;
            }

            WheelUiTweenUtility.Kill(ref sequence);
            binding.Root.Show();
            PreparePopup(binding, motion, snapshot);
            sequence = WheelOutcomePopupRevealSequence.Build(binding, motion, tweenTarget, snapshot, hasOutcome);
        }

        private static void PreparePopup(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            WheelOutcomeSnapshot snapshot)
        {
            ConfigureChromeForPhase(binding, snapshot.Phase);
            binding.ContentRoot.RestoreHome();
            binding.ContentRoot.SetScale(WheelUiTweenUtility.Scale(binding.ContentRoot.HomeScale, motion.StartScale));
            binding.Root.SetAlpha(0f);
            binding.Chrome.Hide();
            binding.FlightIcon.SetGameObjectActive(false);
            binding.RewardBurstParticle.Clear();

            Sprite icon = snapshot.Icon;
            Color iconColor = WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor);
            binding.Icon.ResetTransform(WheelOutcomePopupAnimationConfig.IconStartScale);
            binding.Icon.ApplySprite(icon, iconColor, 0f);
            binding.ResultText.SetAlpha(0f);
            binding.Flash.SetAlpha(0f);
            binding.Shine.SetAlpha(0f);
            WheelUiGraphicUtility.SetLocalScale(binding.Shine.RectTransform, Vector3.one);
        }

        internal static Tween PlayChromeReveal(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            ConfigureChromeForPhase(binding, snapshot.Phase);
            return binding.Chrome.FadeIn();
        }

        internal static void ConfigureChromeForPhase(WheelOutcomePopupRefs binding, WheelGamePhase phase)
        {
            bool isBombed = phase == WheelGamePhase.Bombed;
            binding.RewardPopupBackground.SetActive(true);
            binding.BombCardShadow.SetActive(isBombed);
            binding.OutcomeRetryButton.SetActive(isBombed);
        }
    }

}
