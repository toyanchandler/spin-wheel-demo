using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public static class WheelUiTweenUtility
    {
        public static Tween EmptyTween()
        {
            return DOVirtual.DelayedCall(0f, () => { }, false);
        }

        public static void Kill(ref Sequence sequence)
        {
            sequence?.Kill();
            sequence = null;
        }

        public static void KillImageTweens(Image image)
        {
            image?.DOKill();
            image?.rectTransform.DOKill();
        }

        public static Vector3 Scale(Vector3 scale, float multiplier)
        {
            return new Vector3(scale.x * multiplier, scale.y * multiplier, scale.z);
        }
    }
}
