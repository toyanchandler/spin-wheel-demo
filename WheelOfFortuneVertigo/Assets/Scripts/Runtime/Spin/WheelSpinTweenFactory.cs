using System;
using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Creates the rotation, suspense, and landing-punch tweens used by the spin
    /// sequence. Owns the <see cref="RectTransform"/> that the spin animates so
    /// <see cref="WheelSpinSequenceBuilder"/> can stay tween-shape agnostic.
    /// </summary>
    internal sealed class WheelSpinTweenFactory
    {
        private readonly RectTransform _wheelTransform;

        public WheelSpinTweenFactory(RectTransform wheelTransform)
        {
            _wheelTransform = wheelTransform;
        }

        public RectTransform WheelTransform => _wheelTransform;

        public void ApplyAngle(float angle)
        {
            if (_wheelTransform == null) return;

            _wheelTransform.eulerAngles = new Vector3(0f, 0f, angle);
        }

        public Tween CreateRotation(float fromAngle, float toAngle, float duration, Ease ease)
        {
            return DOVirtual
                .Float(fromAngle, toAngle, duration, ApplyAngle)
                .SetEase(ease)
                .SetTarget(_wheelTransform);
        }

        public Tween CreateSmoothSuspense(
            float fromAngle,
            float toAngle,
            float duration,
            float[] pointerAngles,
            Action<int> onSliceCrossed)
        {
            var sliceTracker = new WheelSpinSuspenseSliceTracker();
            sliceTracker.Reset(fromAngle, pointerAngles);

            return DOVirtual
                .Float(0f, 1f, duration, normalizedTime =>
                {
                    float easedProgress = WheelSpinAnglePlanner.EvaluateSuspenseProgress(normalizedTime);
                    float currentAngle = Mathf.LerpUnclamped(fromAngle, toAngle, easedProgress);
                    ApplyAngle(currentAngle);
                    WheelSliceCrossingResult crossing = sliceTracker.ConsumeCrossing(currentAngle, pointerAngles);
                    if (crossing.DidCross && onSliceCrossed != null)
                    {
                        onSliceCrossed.Invoke(crossing.SliceIndex);
                    }
                })
                .SetEase(Ease.Linear)
                .SetTarget(_wheelTransform);
        }

        public Tween CreateLandingPunch()
        {
            if (_wheelTransform == null) return DOVirtual.DelayedCall(0f, () => { });

            return _wheelTransform
                .DOPunchScale(
                    new Vector3(WheelSpinTiming.LandingPunchScale, WheelSpinTiming.LandingPunchScale, 0f),
                    WheelSpinTiming.LandingPunchDuration,
                    8,
                    0.45f)
                .SetEase(Ease.OutQuad)
                .SetTarget(_wheelTransform);
        }
    }
}
