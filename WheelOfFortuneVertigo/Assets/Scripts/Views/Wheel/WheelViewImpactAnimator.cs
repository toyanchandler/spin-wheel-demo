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
        private readonly WheelSpinner _spinner;

        public WheelViewImpactAnimator(WheelSpinner spinner)
        {
            _spinner = spinner;
        }

        public void PlayBombShake()
        {
            if (_spinner == null || _spinner.WheelTransform == null)
            {
                return;
            }

            RectTransform wheelTransform = _spinner.WheelTransform;
            if (!_hasHomeScale)
            {
                _homeScale = wheelTransform.localScale;
                _hasHomeScale = true;
            }

            wheelTransform.DOKill();
            wheelTransform.localScale = _homeScale;
            wheelTransform
                .DOPunchScale(new Vector3(BombPunchScale, BombPunchScale, 0f), BombPunchDuration, BombPunchVibrato, BombPunchElasticity)
                .SetTarget(wheelTransform)
                .SetUpdate(true)
                .SetLink(_spinner.gameObject, LinkBehaviour.KillOnDisable);
        }
    }
}
