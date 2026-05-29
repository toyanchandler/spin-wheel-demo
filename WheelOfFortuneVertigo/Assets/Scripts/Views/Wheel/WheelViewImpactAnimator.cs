using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelViewImpactAnimator
    {
        private const float BombPunchScale = 0.045f;
        private const float BombPunchDuration = 0.16f;
        private const int BombPunchVibrato = 7;
        private const float BombPunchElasticity = 0.42f;

        private Vector3 _homeScale = Vector3.one;
        private bool _hasHomeScale;
        private readonly WheelSpinPresentationChannel _spinPresentation;
        private readonly GameObject _tweenOwner;

        public WheelViewImpactAnimator(WheelSpinPresentationChannel spinPresentation, Object tweenOwner = null)
        {
            _spinPresentation = spinPresentation;
            _tweenOwner = ResolveTweenOwner(tweenOwner);
        }

        public void PlayBombShake()
        {
            RectTransform wheelTransform = _spinPresentation?.WheelTransform;
            if (wheelTransform == null) return;
            if (!_hasHomeScale)
            {
                _homeScale = wheelTransform.localScale;
                _hasHomeScale = true;
            }

            DOTween.Kill(wheelTransform);
            wheelTransform.localScale = _homeScale;
            wheelTransform
                .DOPunchScale(new Vector3(BombPunchScale, BombPunchScale, 0f), BombPunchDuration, BombPunchVibrato, BombPunchElasticity)
                .SetTarget(wheelTransform)
                .SetUpdate(true)
                .SetLink(_tweenOwner != null ? _tweenOwner : wheelTransform.gameObject, LinkBehaviour.KillOnDisable);
        }

        private static GameObject ResolveTweenOwner(Object tweenOwner)
        {
            if (tweenOwner is GameObject gameObject)
            {
                return gameObject;
            }

            if (tweenOwner is Component component)
            {
                return component.gameObject;
            }

            return null;
        }
    }
}
