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
        [SerializeField] private Color _bombFlashColor;
        [SerializeField] private Color _rewardFlashColor;
        [SerializeField] private float _bombFlashAlpha;
        [SerializeField] private float _rewardFlashAlpha;

        public float FadeDuration => _fadeDuration;
        public float ScaleDuration => _scaleDuration;
        public float StartScale => _startScale;
        public Ease ScaleEase => _scaleEase;
        public Color BombFlashColor => _bombFlashColor;
        public Color RewardFlashColor => _rewardFlashColor;
        public float BombFlashAlpha => _bombFlashAlpha;
        public float RewardFlashAlpha => _rewardFlashAlpha;

        public Color ResolveFlashColor(WheelGamePhase phase) => phase == WheelGamePhase.Bombed ? _bombFlashColor : _rewardFlashColor;

        public float ResolveFlashAlpha(WheelGamePhase phase) => phase == WheelGamePhase.Bombed ? _bombFlashAlpha : _rewardFlashAlpha;

        public static WheelOutcomePopupMotion Default()
        {
            return new WheelOutcomePopupMotion
            {
                _fadeDuration = 0.09f,
                _scaleDuration = 0.26f,
                _startScale = 0.92f,
                _scaleEase = Ease.OutBack,
                _bombFlashColor = new Color(1f, 0.22f, 0.04f, 1f),
                _rewardFlashColor = new Color(1f, 0.8f, 0.28f, 1f),
                _bombFlashAlpha = 0.22f,
                _rewardFlashAlpha = 0.34f
            };
        }
    }
}
