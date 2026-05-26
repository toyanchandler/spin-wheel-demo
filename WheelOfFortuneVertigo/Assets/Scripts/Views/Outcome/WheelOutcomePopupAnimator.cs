using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupAnimator
    {
        public static void Hide(WheelOutcomePopupRefs binding, ref Sequence sequence)
        {
            WheelUiTweenUtility.Kill(ref sequence);
            StopRewardBurst(binding);
            ClearMainIcon(binding);
            binding.Root.SetActive(false);
        }

        public static void ClearMainIcon(WheelOutcomePopupRefs binding)
        {
            if (binding.IconImage == null)
            {
                return;
            }

            RectTransform iconRect = binding.IconImage.rectTransform;
            binding.IconImage.sprite = null;
            WheelUiGraphicUtility.SetGraphicAlpha(binding.IconImage, 0f);
            iconRect.anchoredPosition = binding.IconHomeAnchoredPosition;
            iconRect.localScale = Vector3.one;
            iconRect.localRotation = Quaternion.identity;
        }

        public static void Show(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            Object tweenTarget,
            WheelOutcomeSnapshot snapshot,
            bool hasOutcome,
            ref Sequence sequence)
        {
            if (binding.Root.activeSelf)
            {
                return;
            }

            WheelUiTweenUtility.Kill(ref sequence);
            binding.Root.SetActive(true);

            if (binding.CanvasGroup == null || binding.ContentRoot == null)
            {
                return;
            }

            float flightStageStart = WheelOutcomePopupAnimationConfig.RevealStageStart + WheelOutcomePopupAnimationConfig.RewardHoldDuration;
            RectTransform iconRect = binding.IconImage.rectTransform;
            Vector2 revealIconPosition = binding.IconHomeAnchoredPosition;
            Sprite icon = snapshot.Icon;

            PreparePopup(binding, motion, snapshot, iconRect, icon);
            sequence = BuildShowSequence(binding, motion, tweenTarget, snapshot, hasOutcome, iconRect, revealIconPosition, icon, flightStageStart);
            AddSummaryFade(binding, sequence, flightStageStart);
        }

        private static Sequence BuildShowSequence(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            Object tweenTarget,
            WheelOutcomeSnapshot snapshot,
            bool hasOutcome,
            RectTransform iconRect,
            Vector2 revealIconPosition,
            Sprite icon,
            float flightStageStart)
        {
            return DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .Append(binding.CanvasGroup.DOFade(1f, WheelOutcomePopupAnimationConfig.PopupFadeInDuration))
                .Join(binding.ContentRoot.DOScale(binding.ContentHomeScale, motion.ScaleDuration).SetEase(motion.ScaleEase))
                .Join(binding.IconImage.DOFade(icon == null ? 0f : WheelOutcomePopupAnimationConfig.IconPreviewAlpha, WheelOutcomePopupAnimationConfig.IconPreviewFadeDuration))
                .Join(iconRect.DOAnchorPos(revealIconPosition, WheelOutcomePopupAnimationConfig.RevealStageStart).SetEase(Ease.OutCubic))
                .Join(iconRect.DOScale(WheelOutcomePopupAnimationConfig.IconRevealScale, WheelOutcomePopupAnimationConfig.RevealStageStart).SetEase(Ease.OutBack))
                .Insert(0f, PlayChromeReveal(binding, snapshot))
                .InsertCallback(WheelOutcomePopupAnimationConfig.RevealStageStart, () =>
                {
                    PrepareRewardReveal(binding, snapshot, icon, revealIconPosition);
                    PlayRewardBurst(binding, snapshot);
                })
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, binding.IconImage.DOFade(icon == null ? 0f : 1f, WheelOutcomePopupAnimationConfig.IconFinalFadeDuration))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, iconRect.DOScale(WheelOutcomePopupAnimationConfig.IconRewardPeakScale, WheelOutcomePopupAnimationConfig.TextFadeInDuration).SetEase(Ease.OutBack))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart + WheelOutcomePopupAnimationConfig.TextFadeInDuration, iconRect.DOScale(WheelOutcomePopupAnimationConfig.IconRewardSettleScale, WheelOutcomePopupAnimationConfig.TextFadeInDuration).SetEase(Ease.OutQuad))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, PlayFlash(binding, snapshot))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart + WheelOutcomePopupAnimationConfig.TextFadeInDelay, binding.ResultText.DOFade(1f, WheelOutcomePopupAnimationConfig.TextFadeInDuration))
                .Insert(WheelOutcomePopupAnimationConfig.RevealStageStart, PlayShine(binding))
                .InsertCallback(WheelOutcomePopupAnimationConfig.RewardBurstStopTime, () => StopRewardBurst(binding))
                .InsertCallback(flightStageStart, () => PlayRewardFlight(binding, tweenTarget, hasOutcome));
        }

        private static void AddSummaryFade(WheelOutcomePopupRefs binding, Sequence sequence, float flightStageStart)
        {
            if (binding.SummaryText?.gameObject.activeInHierarchy == true)
            {
                sequence.Insert(flightStageStart - WheelOutcomePopupAnimationConfig.SummaryFadeBeforeFlight, binding.SummaryText.DOFade(1f, WheelOutcomePopupAnimationConfig.TextFadeInDuration));
            }
        }

        private static void PreparePopup(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            WheelOutcomeSnapshot snapshot,
            RectTransform iconRect,
            Sprite icon)
        {
            RestoreBakedContentTransform(binding);
            binding.CanvasGroup.alpha = 0f;
            ConfigureChromeForPhase(binding, snapshot.Phase);
            PrepareChrome(binding);
            HideFlightIconPool(binding);
            StopRewardBurst(binding);

            binding.ContentRoot.localScale = WheelUiTweenUtility.Scale(binding.ContentHomeScale, motion.StartScale);
            iconRect.anchoredPosition = binding.IconHomeAnchoredPosition;
            iconRect.localScale = WheelOutcomePopupAnimationConfig.IconStartScale;
            iconRect.localRotation = Quaternion.identity;

            binding.IconImage.sprite = icon;
            binding.IconImage.enabled = icon != null;
            binding.IconImage.color = WheelUiGraphicUtility.WithAlpha(WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor), 0f);
            binding.IconImage.preserveAspect = true;

            WheelUiGraphicUtility.SetTextAlpha(binding.ResultText, 0f);
            WheelUiGraphicUtility.SetTextAlpha(binding.SummaryText, 0f);
            WheelUiGraphicUtility.SetGraphicAlpha(binding.FlashImage, 0f);
            WheelUiGraphicUtility.SetGraphicAlpha(binding.ShineImage, 0f);
            WheelUiGraphicUtility.SetLocalScale(binding.ShineImage?.rectTransform, Vector3.one);
        }

        private static void RestoreBakedContentTransform(WheelOutcomePopupRefs binding)
        {
            binding.ContentRoot.anchorMin = binding.ContentHomeAnchorMin;
            binding.ContentRoot.anchorMax = binding.ContentHomeAnchorMax;
            binding.ContentRoot.pivot = binding.ContentHomePivot;
            binding.ContentRoot.anchoredPosition = binding.ContentHomeAnchoredPosition;
        }

        private static void PrepareChrome(WheelOutcomePopupRefs binding)
        {
            if (binding.RewardChromeGroup != null)
            {
                binding.RewardChromeGroup.alpha = 0f;
            }
        }

        private static Tween PlayChromeReveal(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            if (binding.RewardChromeGroup == null)
            {
                return WheelUiTweenUtility.EmptyTween();
            }

            ConfigureChromeForPhase(binding, snapshot.Phase);
            return binding.RewardChromeGroup.DOFade(1f, WheelOutcomePopupAnimationConfig.ChromeFadeInDuration).SetUpdate(true);
        }

        private static void ConfigureChromeForPhase(WheelOutcomePopupRefs binding, WheelGamePhase phase)
        {
            bool isBombed = phase == WheelGamePhase.Bombed;
            WheelUiGraphicUtility.SetActive(binding.RewardPopupBackground, true);
            WheelUiGraphicUtility.SetActive(binding.BombCardShadow, isBombed);
            WheelUiGraphicUtility.SetActive(binding.BombWarmTopGlow, isBombed);
            WheelUiGraphicUtility.SetActive(binding.BombHalo, isBombed);
            WheelUiGraphicUtility.SetActive(binding.OutcomeRetryButton, isBombed);
        }

        private static void PrepareRewardReveal(
            WheelOutcomePopupRefs binding,
            WheelOutcomeSnapshot snapshot,
            Sprite icon,
            Vector2 revealIconPosition)
        {
            RectTransform iconRect = binding.IconImage.rectTransform;
            iconRect.anchoredPosition = revealIconPosition;
            iconRect.localScale = WheelOutcomePopupAnimationConfig.IconRevealScale;
            iconRect.localRotation = Quaternion.identity;
            binding.IconImage.sprite = icon;
            binding.IconImage.enabled = icon != null;
            binding.IconImage.color = WheelUiGraphicUtility.WithAlpha(WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor), 0f);
            binding.IconImage.preserveAspect = true;
        }

        private static Tween PlayFlash(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            if (binding.FlashImage == null)
            {
                return WheelUiTweenUtility.EmptyTween();
            }

            binding.FlashImage.color = WheelUiGraphicUtility.WithAlpha(WheelOutcomePopupPalette.FlashColor(snapshot), 0f);

            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(binding.FlashImage.DOFade(WheelOutcomePopupPalette.FlashAlpha(snapshot), WheelOutcomePopupAnimationConfig.FlashFadeInDuration))
                .Append(binding.FlashImage.DOFade(0f, WheelOutcomePopupAnimationConfig.FlashFadeOutDuration));
        }

        private static Tween PlayShine(WheelOutcomePopupRefs binding)
        {
            if (binding.ShineImage == null)
            {
                return WheelUiTweenUtility.EmptyTween();
            }

            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(binding.ShineImage.DOFade(WheelOutcomePopupAnimationConfig.ShineAlpha, WheelOutcomePopupAnimationConfig.ShineFadeInDuration))
                .AppendInterval(WheelOutcomePopupAnimationConfig.ShineHoldDuration)
                .Append(binding.ShineImage.DOFade(0f, WheelOutcomePopupAnimationConfig.ShineFadeOutDuration));
        }

        private static void PlayRewardFlight(WheelOutcomePopupRefs binding, Object tweenTarget, bool hasOutcome)
        {
            if (!hasOutcome || binding.RewardPanelView == null || binding.GetCurrentSnapshot == null)
            {
                CompletePresentation(binding);
                return;
            }

            WheelOutcomeSnapshot snapshot = binding.GetCurrentSnapshot();
            if (snapshot.Phase == WheelGamePhase.Bombed)
            {
                return;
            }

            if (snapshot.Phase != WheelGamePhase.Won || snapshot.RewardAmount <= 0)
            {
                CompletePresentation(binding);
                return;
            }

            Sprite icon = snapshot.Icon;
            if (icon == null)
            {
                CompletePresentation(binding);
                return;
            }

            binding.RewardPanelView.HoldPendingRewardsForArrival();
            PlayPopupIconFlight(binding, snapshot, icon, tweenTarget);
        }

        private static void CompletePresentation(WheelOutcomePopupRefs binding)
        {
            StopRewardBurst(binding);
            ClearMainIcon(binding);
            binding.Root.SetActive(false);
            binding.MarkPresentationComplete?.Invoke();
        }

        private static void PlayRewardBurst(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            if (snapshot.Phase != WheelGamePhase.Won || snapshot.Icon == null || binding.RewardBurstParticle == null)
            {
                StopRewardBurst(binding);
                return;
            }

            WheelUiGraphicUtility.SetEnabled(binding.RewardBurstCamera, true);
            WheelUiGraphicUtility.SetEnabled(binding.RewardBurstDisplay, true);

            binding.RewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            binding.RewardBurstParticle.Play(true);
        }

        private static void StopRewardBurst(WheelOutcomePopupRefs binding)
        {
            if (binding.RewardBurstParticle != null)
            {
                binding.RewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            WheelUiGraphicUtility.SetEnabled(binding.RewardBurstDisplay, false);

            WheelUiGraphicUtility.SetEnabled(binding.RewardBurstCamera, false);
        }

        private static void PlayPopupIconFlight(
            WheelOutcomePopupRefs binding,
            WheelOutcomeSnapshot snapshot,
            Sprite icon,
            Object tweenTarget)
        {
            RectTransform rect = binding.IconImage.rectTransform;
            WheelUiTweenUtility.KillImageTweens(binding.IconImage);
            binding.IconImage.sprite = icon;
            binding.IconImage.enabled = true;
            binding.IconImage.color = WheelOutcomePopupPalette.VisibleIconColor(snapshot.IconImageColor);
            binding.IconImage.preserveAspect = true;

            Vector3 target = binding.RewardPanelView.ResolveLandingWorldPosition(snapshot.RewardId, 0, 1);
            DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .AppendInterval(WheelOutcomePopupAnimationConfig.RewardFlightDelay)
                .Append(rect.DOMove(target, WheelOutcomePopupAnimationConfig.RewardFlightDuration).SetEase(Ease.InOutQuad))
                .Join(rect.DOScale(WheelOutcomePopupAnimationConfig.IconFlightEndScale, WheelOutcomePopupAnimationConfig.RewardFlightDuration).SetEase(Ease.InOutQuad))
                .Join(rect.DOLocalRotate(new Vector3(0f, 0f, WheelOutcomePopupAnimationConfig.RewardFlightRotation), WheelOutcomePopupAnimationConfig.RewardFlightDuration * 0.48f, RotateMode.Fast).SetEase(Ease.OutSine))
                .Join(binding.IconImage.DOFade(1f, WheelOutcomePopupAnimationConfig.RewardFlightDuration * WheelOutcomePopupAnimationConfig.RewardFlightFadeRatio))
                .AppendCallback(() => binding.RewardPanelView.CommitPendingRewardsNow())
                .Append(binding.IconImage.DOFade(0f, WheelOutcomePopupAnimationConfig.IconFadeOutDuration))
                .Append(binding.CanvasGroup.DOFade(0f, WheelOutcomePopupAnimationConfig.PopupFadeOutDuration))
                .AppendCallback(() => CompletePresentation(binding));
        }

        private static void HideFlightIconPool(WheelOutcomePopupRefs binding)
        {
            if (binding.FlightIconPool == null)
            {
                return;
            }

            for (int i = 0; i < binding.FlightIconPool.Length; i++)
            {
                Image image = binding.FlightIconPool[i];
                if (image == null)
                {
                    continue;
                }

                WheelUiTweenUtility.KillImageTweens(image);
                image.gameObject.SetActive(false);
            }
        }

    }
}
