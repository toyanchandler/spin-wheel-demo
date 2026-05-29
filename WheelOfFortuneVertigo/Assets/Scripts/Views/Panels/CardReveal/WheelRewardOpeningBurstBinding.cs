using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardOpeningBurstBinding
    {
        private readonly RawImage _display;
        private readonly ParticleSystem _particle;

        public bool IsAvailable => _display != null && _particle != null;

        public WheelRewardOpeningBurstBinding(RawImage display, ParticleSystem particle)
        {
            _display = display;
            _particle = particle;
        }

        public void Prepare()
        {
            if (!IsAvailable) return;
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Emit(int count)
        {
            if (!IsAvailable) return;
            _particle.Emit(count);
        }

        public void Stop()
        {
            _particle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
