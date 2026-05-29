using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupBurstParticleHandle
    {
        private readonly ParticleSystem _particle;

        public WheelOutcomePopupBurstParticleHandle(ParticleSystem particle) => _particle = particle;

        public void Clear()
        {
            DOTween.Kill(_particle);
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Play(float emissionDuration)
        {
            DOTween.Kill(_particle);
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particle.Play(true);
            DOVirtual.DelayedCall(
                    emissionDuration,
                    () => _particle.Stop(true, ParticleSystemStopBehavior.StopEmitting),
                    false)
                .SetUpdate(true)
                .SetTarget(_particle);
        }
    }
}
