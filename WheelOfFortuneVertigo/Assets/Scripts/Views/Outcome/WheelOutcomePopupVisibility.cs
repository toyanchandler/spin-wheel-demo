using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupVisibility
    {
        public static void Apply(
            WheelOutcomePopupRefs binding,
            WheelOutcomePopupMotion motion,
            Object tweenTarget,
            WheelOutcomeSnapshot snapshot,
            int show,
            bool hasOutcome,
            ref Sequence sequence)
        {
            if (show == 0)
            {
                WheelOutcomePopupAnimator.Hide(binding, ref sequence);
                return;
            }

            WheelOutcomePopupAnimator.Show(binding, motion, tweenTarget, snapshot, hasOutcome, ref sequence);
        }
    }
}
