using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelHudButtonPresentation
    {
        public static void Apply(
            Transform transform,
            Button button,
            CanvasGroup canvasGroup,
            WheelHudSnapshot snapshot,
            bool visible,
            bool interactable,
            string label,
            TextMeshProUGUI labelText,
            bool playIdlePulse)
        {
            button.interactable = visible && interactable;
            ApplyLabel(labelText, label);
            ApplyCanvasGroup(canvasGroup, visible);
            ApplyScale(transform, visible && button.interactable, playIdlePulse);
        }

        private static void ApplyLabel(TextMeshProUGUI labelText, string label)
        {
            if (labelText == null || string.IsNullOrEmpty(label))
            {
                return;
            }

            labelText.text = label;
        }

        private static void ApplyCanvasGroup(CanvasGroup canvasGroup, bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private static void ApplyScale(Transform transform, bool enabled, bool playIdlePulse)
        {
            DOTween.Kill(transform);
            float targetScale = enabled ? 1f : WheelButtonMotion.DisabledScale;
            Tween settle = transform
                .DOScale(WheelButtonMotion.UniformScale(targetScale), WheelButtonMotion.StateSettleDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);

            if (!enabled || !playIdlePulse)
            {
                return;
            }

            settle.OnComplete(() => WheelButtonFeedbackAnimator.PlayIdlePulse(transform));
        }
    }
}
