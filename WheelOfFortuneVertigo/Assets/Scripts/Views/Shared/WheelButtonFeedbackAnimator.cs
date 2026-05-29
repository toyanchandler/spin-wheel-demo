using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal static class WheelButtonFeedbackAnimator
    {
        public static void PlayClickFeedback(Transform transform)
        {
            DOTween.Kill(transform);
            transform.localScale = Vector3.one;
            DOTween.Sequence()
                .SetTarget(transform)
                .SetUpdate(true)
                .Append(transform.DOScale(WheelButtonMotion.PressInScale, WheelButtonMotion.PressInDuration).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(WheelButtonMotion.PressOutScale, WheelButtonMotion.PressOutDuration).SetEase(Ease.OutCubic))
                .Append(transform.DOScale(Vector3.one, WheelButtonMotion.PressSettleDuration).SetEase(Ease.OutQuad));
        }

        public static void PlayIdlePulse(Transform transform)
        {
            transform.DOScale(WheelButtonMotion.IdlePulseScale, WheelButtonMotion.IdlePulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetTarget(transform)
                .SetUpdate(true);
        }

        public static void PlayImageGlow(Image image, Object linkTarget)
        {
            DOTween.Kill(image);
            Color baseColor = image.color;
            DOTween.Sequence()
                .SetTarget(image)
                .SetUpdate(true)
                .SetLink(ResolveLinkTarget(linkTarget, image.gameObject), LinkBehaviour.KillOnDisable)
                .Append(image.DOColor(new Color(0.88f, 1f, 1f, 1f), 0.11f).SetEase(Ease.OutQuad))
                .Append(image.DOColor(baseColor, 0.18f).SetEase(Ease.OutCubic));
        }

        private static GameObject ResolveLinkTarget(Object linkTarget, GameObject fallback)
        {
            if (linkTarget is GameObject gameObject) return gameObject;
            if (linkTarget is Component component) return component.gameObject;
            return fallback;
        }
    }
}
