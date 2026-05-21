using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelSpinner : MonoBehaviour, IWheelRuntimeComponent
    {
        public event Action<WheelSpinResult> SpinCompleted;

        [SerializeField] private RectTransform _wheelTransform;
        [SerializeField] private float _spinDuration = 2.2f;
        [SerializeField] private int _minimumRounds = 4;

        private WheelSliceDefinition[] _currentSlices;
        private Tween _activeTween;
        private TweenCallback _completeSpinCallback;
        private WheelSpinResult _pendingResult;
        private int _currentSliceCount;
        private bool _spinning;

        public bool IsSpinning { get { return _spinning; } }

        public void Initialize(WheelEventBus eventBus)
        {
            _completeSpinCallback = CompleteSpin;
        }

        public void Dispose()
        {
            StopActiveTween(false);
            _spinning = false;
            SpinCompleted = null;
            _completeSpinCallback = null;
        }

        public void SetSlices(WheelSliceDefinition[] slices, int sliceCount)
        {
            _currentSlices = slices;
            _currentSliceCount = sliceCount;
        }

        public void Spin()
        {
            int canSpin = Convert.ToInt32(!_spinning && _currentSliceCount > 0);
            SpinStartActions[canSpin](this);
        }

        private void BeginSpin()
        {
            int selectedIndex = UnityEngine.Random.Range(0, _currentSliceCount);
            PlaySpin(selectedIndex);
        }

        private void PlaySpin(int selectedIndex)
        {
            _spinning = true;
            StopActiveTween(false);
            _pendingResult = new WheelSpinResult(selectedIndex, _currentSlices[selectedIndex]);

            float sliceAngle = 360f / _currentSliceCount;
            float startAngle = _wheelTransform.eulerAngles.z;
            float selectedIconAngle = selectedIndex * sliceAngle;
            float targetDelta = 360f * _minimumRounds + Mathf.DeltaAngle(startAngle, selectedIconAngle);
            float targetAngle = startAngle + targetDelta;

            _activeTween = _wheelTransform
                .DORotate(new Vector3(0f, 0f, targetAngle), _spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .SetRecyclable(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(_completeSpinCallback);
        }

        private void CompleteSpin()
        {
            _activeTween = null;
            _spinning = false;
            SpinCompleted?.Invoke(_pendingResult);
        }

        private void StopActiveTween(bool complete)
        {
            _activeTween?.Kill(complete);
            _activeTween = null;
        }

        private static readonly Action<WheelSpinner>[] SpinStartActions =
        {
            spinner => { },
            spinner => spinner.BeginSpin()
        };
    }
}
