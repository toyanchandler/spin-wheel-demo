using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelSlicePresentationResolver
    {
        private readonly WheelSliceView[] _sliceViews;

        public WheelSlicePresentationResolver(WheelSliceView[] sliceViews)
        {
            _sliceViews = sliceViews;
        }

        public bool TryResolveIconAnchoredPosition(int sliceIndex, RectTransform targetParent, out Vector2 anchoredPosition)
        {
            anchoredPosition = default(Vector2);
            return TryResolveIconPresentation(sliceIndex, targetParent, out anchoredPosition, out Sprite _, out Color _);
        }

        public bool TryCopyPointerAngles(int sliceCount, float[] pointerAngles)
        {
            if (sliceCount <= 0 || pointerAngles == null || pointerAngles.Length < sliceCount || _sliceViews == null || _sliceViews.Length < sliceCount)
            {
                return false;
            }

            for (int i = 0; i < sliceCount; i++)
            {
                WheelSliceView sliceView = _sliceViews[i];
                if (sliceView == null)
                {
                    return false;
                }

                pointerAngles[i] = NormalizeAngle(sliceView.PointerAngle);
            }

            return true;
        }

        public bool TryResolveIconPresentation(
            int sliceIndex,
            RectTransform targetParent,
            out Vector2 anchoredPosition,
            out Sprite sprite,
            out Color color)
        {
            anchoredPosition = default(Vector2);
            sprite = null;
            color = Color.white;
            if (_sliceViews == null || sliceIndex < 0 || sliceIndex >= _sliceViews.Length)
            {
                return false;
            }

            WheelSliceView sliceView = _sliceViews[sliceIndex];
            if (sliceView == null || !sliceView.gameObject.activeInHierarchy)
            {
                return false;
            }

            RectTransform iconRect = sliceView.IconRect;
            sprite = sliceView.IconSprite;
            color = sliceView.IconColor;
            if (iconRect == null || sprite == null)
            {
                return false;
            }

            if (targetParent == null)
            {
                return true;
            }

            Canvas canvas = targetParent.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return false;
            }

            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, iconRect.position);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screenPoint, camera, out anchoredPosition);
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            return angle < 0f ? angle + 360f : angle;
        }
    }
}
