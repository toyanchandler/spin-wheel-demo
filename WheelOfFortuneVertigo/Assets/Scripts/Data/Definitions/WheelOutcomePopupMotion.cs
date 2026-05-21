using System;
using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelOutcomePopupMotion
    {
        [SerializeField] private float _fadeDuration;
        [SerializeField] private float _scaleDuration;
        [SerializeField] private float _startScale;
        [SerializeField] private Ease _scaleEase;

        public float FadeDuration { get { return _fadeDuration; } }
        public float ScaleDuration { get { return _scaleDuration; } }
        public float StartScale { get { return _startScale; } }
        public Ease ScaleEase { get { return _scaleEase; } }

        public static WheelOutcomePopupMotion Default()
        {
            return new WheelOutcomePopupMotion
            {
                _fadeDuration = 0.18f,
                _scaleDuration = 0.26f,
                _startScale = 0.92f,
                _scaleEase = Ease.OutBack
            };
        }
    }
}
