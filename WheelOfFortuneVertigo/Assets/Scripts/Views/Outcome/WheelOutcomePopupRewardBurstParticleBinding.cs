using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupRewardBurstParticleBinding : WheelOutcomePopupComponentBinding<ParticleSystem>
    {
        private ParticleSystemRenderer _renderer;

        public ParticleSystem ParticleSystem { get { return Component; } }

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
    }
}
