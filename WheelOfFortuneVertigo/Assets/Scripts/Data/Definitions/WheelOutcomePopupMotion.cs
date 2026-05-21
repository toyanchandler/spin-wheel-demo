using System;
using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelOutcomePopupMotion
    {
        public float fadeDuration;
        public float scaleDuration;
        public float startScale;
        public Ease scaleEase;

        public static WheelOutcomePopupMotion Default()
        {
            return new WheelOutcomePopupMotion
            {
                fadeDuration = 0.18f,
                scaleDuration = 0.26f,
                startScale = 0.92f,
                scaleEase = Ease.OutBack
            };
        }
    }
}
