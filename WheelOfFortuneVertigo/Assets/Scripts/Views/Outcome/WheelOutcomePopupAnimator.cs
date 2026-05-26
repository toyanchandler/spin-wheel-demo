using DG.Tweening;
using TMPro;
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
                Kill(ref sequence);
                RestoreSourceSliceRewardVisual();
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
                Color color = binding.IconImage.color;
                color.a = 0f;
                binding.IconImage.color = color;
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

                Kill(ref sequence);
                binding.Root.SetActive(true);

                if (binding.CanvasGroup == null || binding.ContentRoot == null)
                {
                    return;
                }

                const float burstStageDuration = 0.32f;
                const float revealStageStart = burstStageDuration;
                const float burstVisibleDuration = 3.6f;
                const float rewardHoldDuration = 1.86f;
                const float flightStageStart = revealStageStart + rewardHoldDuration;

                binding.CanvasGroup.alpha = 0f;
                ConfigureChromeForPhase(binding, snapshot.Phase);
                float startScale = motion.StartScale;
                Vector3 responsiveScale = ResolveResponsiveContentScale(binding);
                ApplyResponsiveContentPlacement(binding);
                binding.ContentRoot.localScale = MultiplyScale(responsiveScale, startScale);
                RectTransform iconRect = binding.IconImage.rectTransform;
                Vector2 revealIconPosition = ResolveRewardRevealPosition(binding);
                ApplyRewardRevealLayout(binding, snapshot, revealIconPosition);
                iconRect.anchoredPosition = ResolveIconStartPosition(binding, snapshot, iconRect);
                Sprite icon = ResolvePresentationIcon(snapshot);
                SuppressSourceSliceRewardVisual(snapshot);
                iconRect.localScale = new Vector3(0.38f, 0.38f, 1f);
                iconRect.localRotation = Quaternion.identity;
                binding.IconImage.sprite = icon;
                binding.IconImage.enabled = icon != null;
                binding.IconImage.color = WheelOutcomePopupColors.WithAlpha(ResolvePresentationIconColor(snapshot), 0f);
                SetTextAlpha(binding.TitleText, 0f);
                SetTextAlpha(binding.ResultText, 0f);
                SetOptionalTextAlpha(binding.SummaryText, 0f);
                PrepareVfx(binding);

                sequence = DOTween.Sequence()
                    .SetTarget(tweenTarget)
                    .SetUpdate(true)
                    .Append(binding.CanvasGroup.DOFade(1f, 0.09f))
                    .Join(binding.ContentRoot.DOScale(responsiveScale, motion.ScaleDuration).SetEase(motion.ScaleEase))
                    .Join(binding.IconImage.DOFade(icon == null ? 0f : 0.92f, 0.08f))
                    .Join(iconRect.DOAnchorPos(revealIconPosition, revealStageStart).SetEase(Ease.OutCubic))
                    .Join(iconRect.DOScale(new Vector3(0.58f, 0.58f, 1f), revealStageStart).SetEase(Ease.OutBack))
                    .Insert(0.04f, PlayChromeReveal(binding, snapshot))
                    .InsertCallback(revealStageStart, () =>
                    {
                        PrepareRewardReveal(binding, snapshot, icon, revealIconPosition);
                        PlayRewardBurst(binding, snapshot, revealIconPosition);
                    })
                    .Insert(revealStageStart, binding.IconImage.DOFade(1f, 0.08f))
                    .Insert(revealStageStart, iconRect.DOScale(new Vector3(1.02f, 1.02f, 1f), 0.30f).SetEase(Ease.OutBack))
                    .Insert(revealStageStart + 0.30f, iconRect.DOScale(new Vector3(0.92f, 0.92f, 1f), 0.18f).SetEase(Ease.OutQuad))
                    .Insert(revealStageStart + 0.04f, PlayFlash(binding, snapshot))
                    .Insert(revealStageStart + 0.10f, binding.TitleText.DOFade(1f, 0.16f))
                    .Insert(revealStageStart + 0.18f, binding.ResultText.DOFade(1f, 0.18f))
                    .Insert(revealStageStart + 0.38f, PlayShine(binding))
                    .InsertCallback(burstVisibleDuration, () => StopRewardBurst(binding))
                    .InsertCallback(flightStageStart, () => PlayRewardFlight(binding, tweenTarget, hasOutcome));

                if (binding.SummaryText != null && binding.SummaryText.gameObject.activeInHierarchy)
                {
                    sequence.Insert(flightStageStart - 0.18f, binding.SummaryText.DOFade(1f, 0.18f));
                }
            }

            private static void Kill(ref Sequence sequence)
            {
                if (sequence != null)
                {
                    sequence.Kill();
                    sequence = null;
                }
            }

            private static void PrepareVfx(WheelOutcomePopupRefs binding)
            {
                HideFlightIconPool(binding);
                PrepareChrome(binding);

                if (binding.FlashImage != null)
                {
                    Color color = binding.FlashImage.color;
                    color.a = 0f;
                    binding.FlashImage.color = color;
                    binding.FlashImage.rectTransform.localScale = new Vector3(0.54f, 0.54f, 1f);
                    binding.FlashImage.rectTransform.localRotation = Quaternion.identity;
                }

                if (binding.ShineImage != null)
                {
                    Color color = binding.ShineImage.color;
                    color.a = 0f;
                    binding.ShineImage.color = color;
                    binding.ShineImage.rectTransform.localScale = Vector3.one;
                }
            }

            private static void PrepareChrome(WheelOutcomePopupRefs binding)
            {
                if (binding.RewardChromeGroup == null)
                {
                    return;
                }

                binding.RewardChromeGroup.alpha = 0f;
                binding.RewardChromeGroup.transform.localScale = new Vector3(0.94f, 0.94f, 1f);
            }

            private static Vector3 ResolveResponsiveContentScale(WheelOutcomePopupRefs binding)
            {
                RectTransform parent = binding.ContentRoot == null ? null : binding.ContentRoot.parent as RectTransform;
                Bounds visualBounds = WheelOutcomePopupLayout.ResolvePopupVisualBounds(binding.ContentRoot, WheelOutcomePopupLayout.ResolvePopupVisualRoot(binding));
                WheelOutcomePopupPlacementArea placementArea = WheelOutcomePopupLayout.ResolvePlacementArea(binding, parent, visualBounds);
                float scale = WheelOutcomePopupLayout.ResolveRewardPopupScale(visualBounds, placementArea.Size);
                return new Vector3(scale, scale, 1f);
            }

            private static Vector3 MultiplyScale(Vector3 scale, float multiplier)
            {
                return new Vector3(scale.x * multiplier, scale.y * multiplier, 1f);
            }

            private static void ApplyResponsiveContentPlacement(WheelOutcomePopupRefs binding)
            {
                if (binding.ContentRoot == null)
                {
                    return;
                }

                binding.ContentRoot.anchorMin = new Vector2(0.5f, 0.5f);
                binding.ContentRoot.anchorMax = new Vector2(0.5f, 0.5f);
                binding.ContentRoot.pivot = new Vector2(0.5f, 0.5f);
                RectTransform parent = binding.ContentRoot.parent as RectTransform;
                Bounds visualBounds = WheelOutcomePopupLayout.ResolvePopupVisualBounds(binding.ContentRoot, WheelOutcomePopupLayout.ResolvePopupVisualRoot(binding));
                WheelOutcomePopupPlacementArea placementArea = WheelOutcomePopupLayout.ResolvePlacementArea(binding, parent, visualBounds);
                float scale = WheelOutcomePopupLayout.ResolveRewardPopupScale(visualBounds, placementArea.Size);
                Vector2 target = WheelOutcomePopupLayout.ResolvePopupTarget(binding.PopupCenterAnchor, placementArea, parent);
                binding.ContentRoot.anchoredPosition = WheelOutcomePopupLayout.ResolveRewardPopupAnchoredPosition(visualBounds, scale, target);
            }


            private static Tween PlayChromeReveal(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
            {
                if (binding.RewardChromeGroup == null)
                {
                    return DOVirtual.DelayedCall(0f, () => { }, false);
                }

                ConfigureChromeForPhase(binding, snapshot.Phase);

                return DOTween.Sequence()
                    .SetUpdate(true)
                    .Append(binding.RewardChromeGroup.DOFade(1f, 0.14f))
                    .Join(binding.RewardChromeGroup.transform.DOScale(new Vector3(1.02f, 1.02f, 1f), 0.22f).SetEase(Ease.OutCubic))
                    .Append(binding.RewardChromeGroup.transform.DOScale(Vector3.one, 0.14f).SetEase(Ease.OutQuad));
            }

            private static void ConfigureChromeForPhase(WheelOutcomePopupRefs binding, WheelGamePhase phase)
            {
                bool isBombed = phase == WheelGamePhase.Bombed;
                SetChromeChildActive(binding.BombPopupBackground, isBombed);
                SetChromeChildActive(binding.RewardPopupBackground, !isBombed);
                SetChromeChildActive(binding.BombCardShadow, isBombed);
                SetChromeChildActive(binding.BombOuterStroke, isBombed);
                SetChromeChildActive(binding.BombWarmTopGlow, isBombed);
                SetChromeChildActive(binding.BombHalo, isBombed);
                SetChromeChildActive(binding.OutcomeRetryButton, isBombed);
                SetChromeChildActive(binding.OutcomeExitButton, false);
            }

            private static void SetChromeChildActive(GameObject target, bool isActive)
            {
                if (target != null)
                {
                    target.SetActive(isActive);
                }
            }

            private static void PrepareRewardReveal(
                WheelOutcomePopupRefs binding,
                WheelOutcomeSnapshot snapshot,
                Sprite icon,
                Vector2 revealIconPosition)
            {
                RectTransform iconRect = binding.IconImage.rectTransform;
                iconRect.anchoredPosition = revealIconPosition;
                iconRect.localScale = new Vector3(0.58f, 0.58f, 1f);
                iconRect.localRotation = Quaternion.identity;
                binding.IconImage.sprite = icon;
                binding.IconImage.enabled = icon != null;
                binding.IconImage.color = WheelOutcomePopupColors.WithAlpha(ResolvePresentationIconColor(snapshot), 0f);
                binding.IconImage.preserveAspect = true;
                MoveRewardVfx(binding, revealIconPosition);
            }

            private static Vector2 ResolveRewardRevealPosition(WheelOutcomePopupRefs binding)
            {
                return binding.IconHomeAnchoredPosition;
            }

            private static void ApplyRewardRevealLayout(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot, Vector2 revealIconPosition)
            {
                bool isBombed = snapshot.Phase == WheelGamePhase.Bombed;
                SetAnchoredY(binding.TitleText.rectTransform, revealIconPosition.y + (isBombed ? 102f : 150f));
                SetAnchoredY(binding.ResultText.rectTransform, revealIconPosition.y - (isBombed ? 140f : 210f));
                if (binding.SummaryText != null)
                {
                    SetAnchoredY(binding.SummaryText.rectTransform, revealIconPosition.y - (isBombed ? 202f : 242f));
                }
            }

            private static void SetAnchoredY(RectTransform rect, float y)
            {
                Vector2 position = rect.anchoredPosition;
                position.y = y;
                rect.anchoredPosition = position;
            }

            private static void MoveRewardVfx(WheelOutcomePopupRefs binding, Vector2 revealIconPosition)
            {
                if (binding.FlashImage != null)
                {
                    binding.FlashImage.rectTransform.anchoredPosition = revealIconPosition;
                }

                if (binding.ShineImage != null)
                {
                    binding.ShineImage.rectTransform.anchoredPosition = revealIconPosition;
                }
            }

            private static Tween PlayFlash(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
            {
                if (binding.FlashImage == null)
                {
                    return DOVirtual.DelayedCall(0f, () => { }, false);
                }

                RectTransform rect = binding.FlashImage.rectTransform;
                binding.FlashImage.color = snapshot.Phase == WheelGamePhase.Bombed
                    ? WheelOutcomePopupColors.WithAlpha(new Color(1f, 0.22f, 0.04f, 1f), 0f)
                    : WheelOutcomePopupColors.WithAlpha(new Color(1f, 0.8f, 0.28f, 1f), 0f);
                bool isBombed = snapshot.Phase == WheelGamePhase.Bombed;
                if (!isBombed)
                {
                    return DOTween.Sequence()
                        .SetUpdate(true)
                        .Append(binding.FlashImage.DOFade(0.34f, 0.16f))
                        .Join(rect.DOScale(new Vector3(1.02f, 1.02f, 1f), 0.34f).SetEase(Ease.OutCubic))
                        .Join(rect.DORotate(new Vector3(0f, 0f, 168f), 1.18f, RotateMode.FastBeyond360).SetEase(Ease.OutCubic))
                        .Append(binding.FlashImage.DOFade(0.10f, 0.74f));
                }

                return DOTween.Sequence()
                    .SetUpdate(true)
                    .Append(binding.FlashImage.DOFade(0.22f, 0.12f))
                    .Join(rect.DOScale(new Vector3(1.06f, 1.06f, 1f), 0.34f).SetEase(Ease.OutCubic))
                    .Join(rect.DORotate(new Vector3(0f, 0f, 32f), 0.30f, RotateMode.Fast))
                    .Append(binding.FlashImage.DOFade(0.015f, 0.34f));
            }

            private static Tween PlayShine(WheelOutcomePopupRefs binding)
            {
                if (binding.ShineImage == null)
                {
                    return DOVirtual.DelayedCall(0f, () => { }, false);
                }

                RectTransform rect = binding.ShineImage.rectTransform;
                Vector2 home = rect.anchoredPosition;
                rect.anchoredPosition = home + new Vector2(-150f, 6f);
                return DOTween.Sequence()
                    .SetUpdate(true)
                    .Append(binding.ShineImage.DOFade(0.64f, 0.12f))
                    .Join(rect.DOAnchorPos(home + new Vector2(170f, 6f), 0.58f).SetEase(Ease.InOutSine))
                    .Append(binding.ShineImage.DOFade(0f, 0.18f))
                    .OnComplete(() => rect.anchoredPosition = home);
            }

            private static void PlayRewardFlight(WheelOutcomePopupRefs binding, Object tweenTarget, bool hasOutcome)
            {
                if (!hasOutcome || binding.RewardPanelView == null)
                {
                    CompletePresentation(binding);
                    return;
                }

                WheelOutcomeSnapshot snapshot = default(WheelOutcomeSnapshot);
                if (binding.GetCurrentSnapshot == null)
                {
                    CompletePresentation(binding);
                    return;
                }

                snapshot = binding.GetCurrentSnapshot();
                if (snapshot.Phase == WheelGamePhase.Bombed)
                {
                    return;
                }

                if (snapshot.Phase != WheelGamePhase.Won || snapshot.RewardAmount <= 0)
                {
                    CompletePresentation(binding);
                    return;
                }

                Sprite icon = ResolvePresentationIcon(snapshot);
                if (icon == null)
                {
                    CompletePresentation(binding);
                    return;
                }

                const float delay = 0.07f;
                const float duration = 0.72f;
                binding.RewardPanelView.HoldPendingRewardsForArrival();
                PlayPopupIconFlight(binding, snapshot, icon, tweenTarget, delay, duration);
            }

            private static void CompletePresentation(WheelOutcomePopupRefs binding)
            {
                RestoreSourceSliceRewardVisual();
                StopRewardBurst(binding);
                ClearMainIcon(binding);
                binding.Root.SetActive(false);
                binding.MarkPresentationComplete?.Invoke();
            }

            private static void PlayRewardBurst(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot, Vector2 burstCenter)
            {
                if (snapshot.Phase != WheelGamePhase.Won || snapshot.Icon == null || binding.RewardBurstParticle == null)
                {
                    StopRewardBurst(binding);
                    return;
                }

                if (binding.RewardBurstCamera != null)
                {
                    binding.RewardBurstCamera.enabled = true;
                }

                if (binding.RewardBurstDisplay != null)
                {
                    binding.RewardBurstDisplay.enabled = true;
                    binding.RewardBurstDisplay.color = new Color(1f, 0.78f, 0.32f, 0.58f);
                    RectTransform displayRect = binding.RewardBurstDisplay.rectTransform;
                    RectTransform iconRect = binding.IconImage.rectTransform;
                    RectTransform displayParent = displayRect.parent as RectTransform;
                    displayRect.anchorMin = new Vector2(0.5f, 0.5f);
                    displayRect.anchorMax = new Vector2(0.5f, 0.5f);
                    displayRect.pivot = new Vector2(0.5f, 0.5f);
                    displayRect.anchoredPosition = ResolveBurstDisplayPosition(iconRect, displayRect, displayParent, burstCenter);
                    displayRect.sizeDelta = ResolveBurstDisplaySize(iconRect);
                    displayRect.localRotation = Quaternion.identity;
                    displayRect.localScale = Vector3.one;
                }

                ParticleSystem.TextureSheetAnimationModule textureSheet = binding.RewardBurstParticle.textureSheetAnimation;
                textureSheet.enabled = false;

                if (binding.RewardBurstCamera != null)
                {
                    Transform cameraTransform = binding.RewardBurstCamera.transform;
                    cameraTransform.localPosition = new Vector3(0f, 0f, cameraTransform.localPosition.z);
                    cameraTransform.localRotation = Quaternion.identity;
                }

                Transform particleTransform = binding.RewardBurstParticle.transform;
                particleTransform.localPosition = Vector3.zero;
                particleTransform.localRotation = Quaternion.identity;
                particleTransform.localScale = Vector3.one;
                binding.RewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                binding.RewardBurstParticle.Play(true);
#if UNITY_EDITOR
                // Debug capture is intentionally disabled; enable only while tuning UI particle placement.
                // if (binding.Owner != null)
                // {
                //     binding.Owner.CaptureRewardBurstDebug(binding, snapshot);
                // }
#endif
            }

            private static Vector2 ResolveBurstDisplayPosition(
                RectTransform iconRect,
                RectTransform displayRect,
                RectTransform displayParent,
                Vector2 fallback)
            {
                if (iconRect == null || displayParent == null)
                {
                    return fallback;
                }

                Vector2 iconCenter = WheelOutcomePopupLayout.ResolveRectCenterInParent(iconRect, displayParent);
                return iconCenter - WheelOutcomePopupLayout.ResolveAnchorReferencePoint(displayParent, displayRect);
            }

            private static Vector2 ResolveBurstDisplaySize(RectTransform iconRect)
            {
                float baseSize = Mathf.Max(iconRect.rect.width, iconRect.rect.height);
                float size = Mathf.Clamp(baseSize * 2.35f, 520f, 620f);
                return new Vector2(size, size);
            }

            private static void StopRewardBurst(WheelOutcomePopupRefs binding)
            {
                if (binding.RewardBurstParticle != null)
                {
                    binding.RewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                if (binding.RewardBurstDisplay != null)
                {
                    binding.RewardBurstDisplay.enabled = false;
                }

                if (binding.RewardBurstCamera != null)
                {
                    binding.RewardBurstCamera.enabled = false;
                }
            }

            private static void PlayPopupIconFlight(
                WheelOutcomePopupRefs binding,
                WheelOutcomeSnapshot snapshot,
                Sprite icon,
                Object tweenTarget,
                float delay,
                float duration)
            {
                RectTransform rect = binding.IconImage.rectTransform;
                rect.DOKill();
                binding.IconImage.DOKill();
                binding.IconImage.sprite = icon;
                binding.IconImage.enabled = true;
                binding.IconImage.color = ResolvePresentationIconColor(snapshot);
                binding.IconImage.preserveAspect = true;

                Vector3 target = binding.RewardPanelView.ResolveLandingWorldPosition(snapshot.RewardId, 0, 1);
                DOTween.Sequence()
                    .SetTarget(tweenTarget)
                    .SetUpdate(true)
                    .AppendInterval(delay)
                    .Append(rect.DOMove(target, duration).SetEase(Ease.InOutQuad))
                    .Join(rect.DOScale(new Vector3(0.42f, 0.42f, 1f), duration).SetEase(Ease.InOutQuad))
                    .Join(rect.DOLocalRotate(new Vector3(0f, 0f, 10f), duration * 0.48f, RotateMode.Fast).SetEase(Ease.OutSine))
                    .Join(binding.IconImage.DOFade(1f, duration * 0.68f))
                    .AppendCallback(() => binding.RewardPanelView.CommitPendingRewardsNow())
                    .Append(binding.IconImage.DOFade(0f, 0.08f))
                    .Append(binding.CanvasGroup.DOFade(0f, 0.12f))
                    .AppendCallback(() => CompletePresentation(binding));
            }

            public static Sprite ResolvePresentationIcon(WheelOutcomeSnapshot snapshot)
            {
                if (snapshot.Icon != null)
                {
                    return snapshot.Icon;
                }

                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView == null || snapshot.SourceSliceIndex < 0)
                {
                    return null;
                }

                RectTransform parent = null;
                if (wheelView.TryResolveSliceIconPresentation(
                    snapshot.SourceSliceIndex,
                    parent,
                    out Vector2 _,
                    out Sprite sprite,
                    out Color _))
                {
                    return sprite;
                }

                return null;
            }

            public static Color ResolvePresentationIconColor(WheelOutcomeSnapshot snapshot)
            {
                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView != null
                    && snapshot.SourceSliceIndex >= 0
                    && wheelView.TryResolveSliceIconPresentation(
                        snapshot.SourceSliceIndex,
                        null,
                        out Vector2 _,
                        out Sprite sprite,
                        out Color color)
                    && sprite != null)
                {
                    return WheelOutcomePopupColors.ResolveVisibleIconColor(color);
                }

                return WheelOutcomePopupColors.ResolveVisibleIconColor(Color.white);
            }

            private static void SuppressSourceSliceRewardVisual(WheelOutcomeSnapshot snapshot)
            {
                if (snapshot.Phase != WheelGamePhase.Won || snapshot.SourceSliceIndex < 0)
                {
                    return;
                }

                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView != null)
                {
                    wheelView.SuppressAllRewardVisuals();
                }
            }

            private static void RestoreSourceSliceRewardVisual()
            {
                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView != null)
                {
                    wheelView.RestoreAllRewardVisuals();
                    wheelView.RestoreSuppressedSliceRewardVisual();
                }
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

                    image.DOKill();
                    image.rectTransform.DOKill();
                    image.gameObject.SetActive(false);
                }
            }

            private static Vector2 ResolveIconStartPosition(
                WheelOutcomePopupRefs binding,
                WheelOutcomeSnapshot snapshot,
                RectTransform iconRect)
            {
                RectTransform parent = iconRect.parent as RectTransform;
                if (snapshot.SourceSliceIndex >= 0
                    && parent != null
                    && WheelRuntimeLocator.WheelView != null
                    && WheelRuntimeLocator.WheelView.TryResolveSliceIconAnchoredPosition(
                        snapshot.SourceSliceIndex,
                        parent,
                        out Vector2 anchoredPosition))
                {
                    return anchoredPosition;
                }

                return binding.IconHomeAnchoredPosition;
            }

            private static void SetTextAlpha(TextMeshProUGUI text, float alpha)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }

            private static void SetOptionalTextAlpha(TextMeshProUGUI text, float alpha)
            {
                if (text != null)
                {
                    SetTextAlpha(text, alpha);
                }
            }
    }
}
