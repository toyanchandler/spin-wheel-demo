using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardOpeningBurstBinding
    {
        private readonly Camera _camera;
        private readonly RawImage _display;
        private readonly ParticleSystem _particle;

        public RawImage Display { get { return _display; } }
        public bool IsAvailable { get { return _display != null && _particle != null; } }

        public WheelRewardOpeningBurstBinding(Camera camera, RawImage display, ParticleSystem particle)
        {
            _camera = camera;
            _display = display;
            _particle = particle;
        }

        public void Prepare()
        {
            if (!IsAvailable)
            {
                return;
            }

            _display.enabled = true;
            _display.color = WheelUiGraphicUtility.WithAlpha(_display.color, 0f);
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (_camera != null)
            {
                _camera.enabled = true;
            }
        }

        public void Emit(int count)
        {
            if (!IsAvailable)
            {
                return;
            }

            _display.enabled = true;
            if (_camera != null)
            {
                _camera.enabled = true;
            }

            _particle.Emit(count);
        }

        public void Stop()
        {
            if (_particle != null)
            {
                _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_display != null)
            {
                _display.DOKill();
                _display.enabled = false;
            }

            if (_camera != null)
            {
                _camera.enabled = false;
            }
        }
    }
}
