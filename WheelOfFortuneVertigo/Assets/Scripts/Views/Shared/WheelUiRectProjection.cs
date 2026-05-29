using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelUiRectProjection
    {
        public static bool TryProjectToAnchoredPosition(
            RectTransform source,
            RectTransform targetParent,
            out Vector2 anchoredPosition)
        {
            anchoredPosition = default;
            if (source == null || targetParent == null) return false;

            Canvas canvas = targetParent.GetComponentInParent<Canvas>();
            if (canvas == null) return false;

            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, source.position);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetParent,
                screenPoint,
                camera,
                out anchoredPosition);
        }
    }
}
