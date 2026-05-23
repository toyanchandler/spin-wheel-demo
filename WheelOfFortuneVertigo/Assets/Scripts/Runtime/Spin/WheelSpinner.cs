using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelSpinner : MonoBehaviour
    {
        public event Action<WheelSpinResult> SpinCompleted;

        private RectTransform _wheelTransform;

        private WheelSliceDefinition[] _currentSlices;
        private Tween _activeTween;
        private TweenCallback _completeSpinCallback;
        private WheelSpinResult _pendingResult;
        private int _currentSliceCount;
        private bool _spinning;

        public bool IsSpinning { get { return _spinning; } }
        public int CurrentSliceCount { get { return _currentSliceCount; } }

        public RectTransform WheelTransform { get { return _wheelTransform; } }

        private void Awake()
        {
            _wheelTransform = GetComponent<RectTransform>();
            WheelRuntimeLocator.RegisterSpinner(this);
        }

        private void LateUpdate()
        {
            WheelView wheelView = WheelRuntimeLocator.WheelView;
            if (wheelView == null)
            {
                return;
            }

            wheelView.ApplyUprightSlicePresentations();
        }

        public void Bind(WheelEventBus eventBus)
        {
            _completeSpinCallback = CompleteSpin;
        }

        public void Unbind()
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

            float startAngle = _wheelTransform.eulerAngles.z;
            WheelGameSettings settings = WheelRuntimeLocator.Settings;
            Vector2[] slicePositions = WheelSliceLayoutResolver.BuildPositions(
                settings.SliceLayoutCatalog,
                settings.Layout,
                _currentSliceCount);
            float selectedIconAngle = WheelSliceLayoutResolver.ResolvePointerAngle(slicePositions[selectedIndex]);
            float targetDelta = 360f * settings.MinimumSpinRounds + Mathf.DeltaAngle(startAngle, selectedIconAngle);
            float targetAngle = startAngle + targetDelta;
            float spinDuration = settings.SpinDuration;

            _activeTween = _wheelTransform
                .DORotate(new Vector3(0f, 0f, targetAngle), spinDuration, RotateMode.FastBeyond360)
                .SetEase(settings.SpinEase)
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
