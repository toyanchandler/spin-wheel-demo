using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupRewardBurstParticleBinding : WheelOutcomePopupSceneComponentBinding<ParticleSystem>
    {
        private ParticleSystemRenderer _renderer;

        public ParticleSystem ParticleSystem { get { return RequiredComponent; } }

        public ParticleSystemRenderer Renderer
        {
            get
            {
                if (_renderer == null && !TryGetComponent(out _renderer))
                {
                    throw new System.InvalidOperationException(name + " requires ParticleSystemRenderer.");
                }

                return _renderer;
            }
        }

        public void Clear()
        {
            ParticleSystem.DOKill();
            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void PlayFor(WheelOutcomeSnapshot snapshot)
        {
            if (snapshot.Phase != WheelGamePhase.Won || snapshot.Icon == null)
            {
                Clear();
                return;
            }

            ParticleSystem.DOKill();
            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.Play(true);
            DOVirtual.DelayedCall(
                    WheelOutcomePopupAnimationConfig.RewardBurstEmissionDuration,
                    StopEmission,
                    false)
                .SetUpdate(true)
                .SetTarget(ParticleSystem);
        }

        private void StopEmission()
        {
            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
