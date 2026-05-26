using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public readonly struct WheelOutcomePopupPlacementArea
    {
        public readonly Vector2 Center;
        public readonly Vector2 Size;

        public WheelOutcomePopupPlacementArea(Vector2 center, Vector2 size)
        {
            Center = center;
            Size = size;
        }
    }

    internal static class WheelOutcomePopupLayout
    {
        public static float ResolveRewardPopupScale(Bounds visualBounds, Vector2 fitSize)
        {
            if (visualBounds.size.x <= Mathf.Epsilon || visualBounds.size.y <= Mathf.Epsilon)
            {
                return 1f;
            }

            if (fitSize.x <= Mathf.Epsilon || fitSize.y <= Mathf.Epsilon)
            {
                return 1f;
            }

            return Mathf.Min(1f, fitSize.x / visualBounds.size.x, fitSize.y / visualBounds.size.y);
        }

        public static Vector2 ResolveRewardPopupAnchoredPosition(Bounds visualBounds, float scale, Vector2 target)
        {
            Vector3 center = visualBounds.center;
            return target - new Vector2(center.x * scale, center.y * scale);
        }

        public static Transform ResolvePopupVisualRoot(WheelOutcomePopupRefs refs)
        {
            RectTransform popupRect = ResolveActivePopupRect(refs.RewardPopupBackground, refs.BombPopupBackground);
            if (popupRect != null)
            {
                return popupRect;
            }

            if (refs.RewardChromeGroup != null)
            {
                return refs.RewardChromeGroup.transform;
            }

            return refs.ContentRoot;
        }

        public static RectTransform ResolveActivePopupRect(GameObject rewardPopup, GameObject bombPopup)
        {
            RectTransform rewardRect = rewardPopup != null ? rewardPopup.transform as RectTransform : null;
            RectTransform bombRect = bombPopup != null ? bombPopup.transform as RectTransform : null;
            if (rewardPopup != null && rewardPopup.activeSelf && rewardRect != null)
            {
                return rewardRect;
            }

            if (bombPopup != null && bombPopup.activeSelf && bombRect != null)
            {
                return bombRect;
            }

            return rewardRect != null ? rewardRect : bombRect;
        }

        public static Bounds ResolvePopupVisualBounds(RectTransform contentRoot, Transform visualRoot)
        {
            Bounds bounds = visualRoot != null
                ? RectTransformUtility.CalculateRelativeRectTransformBounds(contentRoot, visualRoot)
                : RectTransformUtility.CalculateRelativeRectTransformBounds(contentRoot);
            if (bounds.size.x > Mathf.Epsilon && bounds.size.y > Mathf.Epsilon)
            {
                return bounds;
            }

            Rect rect = contentRoot.rect;
            return new Bounds(rect.center, rect.size);
        }

        public static WheelOutcomePopupPlacementArea ResolvePlacementArea(
            WheelOutcomePopupRefs refs,
            RectTransform contentParent,
            Bounds visualBounds)
        {
            if (contentParent == null)
            {
                return new WheelOutcomePopupPlacementArea(Vector2.zero, Vector2.zero);
            }

            if (refs.PopupFitArea != null)
            {
                WheelOutcomePopupPlacementArea fitArea = new WheelOutcomePopupPlacementArea(
                    ResolveRectCenterInParent(refs.PopupFitArea, contentParent),
                    refs.PopupFitArea.rect.size);
                return ResolveZoneAwarePlacementArea(contentParent, visualBounds, fitArea, refs.ZoneProgress);
            }

            return ResolveZoneAwarePlacementArea(
                contentParent,
                visualBounds,
                new WheelOutcomePopupPlacementArea(contentParent.rect.center, contentParent.rect.size),
                refs.ZoneProgress);
        }

        public static WheelOutcomePopupPlacementArea ResolveZoneAwarePlacementArea(
            RectTransform contentParent,
            Bounds visualBounds,
            WheelOutcomePopupPlacementArea baseArea,
            WheelZoneProgressView zoneProgress)
        {
            Bounds zoneBounds;
            if (!TryResolveZoneBounds(contentParent, zoneProgress, out zoneBounds))
            {
                return baseArea;
            }

            float baseScale = ResolveRewardPopupScale(visualBounds, baseArea.Size);
            Vector2 basePosition = ResolveRewardPopupAnchoredPosition(visualBounds, baseScale, baseArea.Center);
            float popupTop = basePosition.y + (visualBounds.max.y * baseScale);
            if (popupTop <= zoneBounds.min.y)
            {
                return baseArea;
            }

            Rect parentRect = contentParent.rect;
            float safeTop = Mathf.Min(zoneBounds.min.y, parentRect.yMax);
            float safeHeight = safeTop - parentRect.yMin;
            if (safeHeight <= Mathf.Epsilon)
            {
                return baseArea;
            }

            var safeSize = new Vector2(parentRect.size.x, safeHeight);
            var safeCenter = new Vector2(parentRect.center.x, parentRect.yMin + (safeHeight * 0.5f));
            return new WheelOutcomePopupPlacementArea(safeCenter, safeSize);
        }

        public static Vector2 ResolvePopupTarget(
            RectTransform centerAnchor,
            WheelOutcomePopupPlacementArea placementArea,
            RectTransform contentParent)
        {
            Vector2 target = placementArea.Center;
            if (contentParent == null)
            {
                return target;
            }

            if (centerAnchor == null)
            {
                return target;
            }

            target.x = ResolveRectCenterInParent(centerAnchor, contentParent).x;
            return target;
        }

        public static bool TryResolveZoneBounds(
            RectTransform contentParent,
            WheelZoneProgressView zoneProgress,
            out Bounds zoneBounds)
        {
            if (zoneProgress == null)
            {
                zoneBounds = default;
                return false;
            }

            RectTransform zoneRect = zoneProgress.transform as RectTransform;
            if (zoneRect == null)
            {
                zoneBounds = default;
                return false;
            }

            zoneBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(contentParent, zoneRect);
            return zoneBounds.size.x > Mathf.Epsilon && zoneBounds.size.y > Mathf.Epsilon;
        }

        public static Vector2 ResolveRectCenterInParent(RectTransform target, RectTransform parent)
        {
            Canvas canvas = parent.GetComponentInParent<Canvas>();
            Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Vector3 worldCenter = target.TransformPoint(target.rect.center);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, worldCenter);
            Vector2 localPoint;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, camera, out localPoint)
                ? localPoint
                : Vector2.zero;
        }

        public static Vector2 ResolveAnchorReferencePoint(RectTransform parent, RectTransform subject)
        {
            Rect rect = parent.rect;
            Vector2 anchor = (subject.anchorMin + subject.anchorMax) * 0.5f;
            return new Vector2(
                Mathf.Lerp(rect.xMin, rect.xMax, anchor.x),
                Mathf.Lerp(rect.yMin, rect.yMax, anchor.y));
        }
    }
}
