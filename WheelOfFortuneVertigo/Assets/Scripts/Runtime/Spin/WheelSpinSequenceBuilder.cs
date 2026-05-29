using System;
using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Assembles the four-phase spin sequence (anticipation → fast spin → suspense → landing)
    /// from a <see cref="WheelSpinPlan"/> by wiring tweens from <see cref="WheelSpinTweenFactory"/>
    /// to the lifecycle callbacks supplied by <see cref="WheelSpinner"/>.
    /// </summary>
    internal static class WheelSpinSequenceBuilder
    {
        internal readonly struct Callbacks
        {
            public readonly Action OnSpinStarted;
            public readonly Action<int> OnSuspenseTick;
            public readonly Action OnLandingStarted;
            public readonly Action OnComplete;
            public readonly Action OnKilled;

            public Callbacks(
                Action onSpinStarted,
                Action<int> onSuspenseTick,
                Action onLandingStarted,
                Action onComplete,
                Action onKilled)
            {
                OnSpinStarted = onSpinStarted;
                OnSuspenseTick = onSuspenseTick;
                OnLandingStarted = onLandingStarted;
                OnComplete = onComplete;
                OnKilled = onKilled;
            }
        }

        public static Sequence Build(
            GameObject linkTarget,
            WheelSpinPlan plan,
            float[] pointerAngles,
            Ease spinEase,
            WheelSpinTweenFactory tweenFactory,
            in Callbacks callbacks)
        {
            if (tweenFactory == null) throw new ArgumentNullException(nameof(tweenFactory));

            Sequence sequence = DOTween.Sequence()
                .SetRecyclable(true)
                .SetLink(linkTarget, LinkBehaviour.KillOnDisable);

            AppendAnticipation(sequence, plan, tweenFactory, callbacks.OnSpinStarted);
            AppendFastSpin(sequence, plan, spinEase, tweenFactory);
            AppendSuspense(sequence, plan, pointerAngles, tweenFactory, callbacks.OnSuspenseTick);
            AppendLanding(sequence, tweenFactory, callbacks.OnLandingStarted);

            Tween activeSequence = sequence;
            Action onComplete = callbacks.OnComplete;
            Action onKilled = callbacks.OnKilled;
            sequence
                .OnComplete(() => onComplete?.Invoke())
                .OnKill(() =>
                {
                    if (activeSequence != null && !activeSequence.IsComplete())
                    {
                        onKilled?.Invoke();
                    }
                });

            return sequence;
        }

        private static void AppendAnticipation(
            Sequence sequence,
            WheelSpinPlan plan,
            WheelSpinTweenFactory tweenFactory,
            Action onSpinStarted)
        {
            sequence
                .AppendCallback(() => onSpinStarted?.Invoke())
                .AppendInterval(WheelSpinTiming.AnticipationDelay)
                .Append(tweenFactory.CreateRotation(
                    plan.StartAngle,
                    plan.StartAngle + WheelSpinTiming.AnticipationPullDegrees,
                    WheelSpinTiming.AnticipationPullDuration,
                    Ease.OutQuad))
                .AppendInterval(WheelSpinTiming.AnticipationChargeDuration)
                .Append(tweenFactory.CreateRotation(
                    plan.StartAngle + WheelSpinTiming.AnticipationPullDegrees,
                    plan.LaunchAngle,
                    WheelSpinTiming.AnticipationReleaseDuration,
                    Ease.InQuad));
        }

        private static void AppendFastSpin(
            Sequence sequence,
            WheelSpinPlan plan,
            Ease spinEase,
            WheelSpinTweenFactory tweenFactory)
        {
            sequence.Append(tweenFactory.CreateRotation(
                plan.LaunchAngle,
                plan.SuspenseStartAngle,
                plan.FastSpinDuration,
                spinEase));
        }

        private static void AppendSuspense(
            Sequence sequence,
            WheelSpinPlan plan,
            float[] pointerAngles,
            WheelSpinTweenFactory tweenFactory,
            Action<int> onSuspenseTick)
        {
            sequence.Append(tweenFactory.CreateSmoothSuspense(
                plan.SuspenseStartAngle,
                plan.TargetAngle,
                plan.SuspenseDuration,
                pointerAngles,
                onSuspenseTick));
        }

        private static void AppendLanding(
            Sequence sequence,
            WheelSpinTweenFactory tweenFactory,
            Action onLandingStarted)
        {
            sequence
                .AppendCallback(() => onLandingStarted?.Invoke())
                .Append(tweenFactory.CreateLandingPunch())
                .AppendInterval(WheelSpinTiming.PostLandingHoldDuration);
        }
    }
}
