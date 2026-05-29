using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public static class WheelUiTweenUtility
    {
        public static Tween EmptyTween() => DOVirtual.DelayedCall(0f, () => { }, false);

        public static void Kill(ref Sequence sequence)
        {
            sequence?.Kill();
            sequence = null;
        }

        public static void KillImageTweens(Image image)
        {
            if (image != null) DOTween.Kill(image);
            if (image != null) DOTween.Kill(image.rectTransform);
        }

        public static Vector3 Scale(Vector3 scale, float multiplier) => new Vector3(scale.x * multiplier, scale.y * multiplier, scale.z);
    }
}
