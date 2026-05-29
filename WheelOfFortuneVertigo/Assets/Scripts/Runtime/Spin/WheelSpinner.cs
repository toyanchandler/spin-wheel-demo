using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// MonoBehaviour orchestrator for the wheel spin animation.
    /// Owns the wheel <see cref="RectTransform"/>, the slice/pointer-angle buffers,
    /// and the spin state machine. Plan creation lives in <see cref="WheelSpinAnglePlanner"/>,
    /// tween creation in <see cref="WheelSpinTweenFactory"/>, and DOTween sequence
    /// assembly in <see cref="WheelSpinSequenceBuilder"/>.
    /// </summary>
    public sealed class WheelSpinner : MonoBehaviour, IWheelSpinDriver
    {
        public event Action SpinStarted;
        public event Action<int> SuspenseTicked;
        public event Action<WheelSpinResult> LandingStarted;
        public event Action<WheelSpinResult> SpinCompleted;

        private static readonly WheelSliceDefinition[] EmptySlices = new WheelSliceDefinition[0];
        private static readonly float[] EmptyAngles = new float[0];

        private RectTransform _wheelTransform;
        private WheelGameSettings _settings;
        private WheelSpinPresentationChannel _spinPresentation;
        private IRandomSource _randomSource = UnityRandomSource.Shared;
        private WheelSpinTweenFactory _tweenFactory;
        private WheelSliceDefinition[] _sliceBuffer = EmptySlices;
        private WheelSliceDefinition[] _currentSlices = EmptySlices;
        private float[] _pointerAngleBuffer = EmptyAngles;
        private Tween _activeTween;
        private WheelSpinResult _pendingResult;
        private Vector3 _baseScale = Vector3.one;

        private int _currentSliceCount;
        private bool _spinning;
        private float _lastTargetAngle;

        public bool IsSpinning => _spinning;

        public int CurrentSliceCount => _currentSliceCount;

        public RectTransform WheelTransform => _wheelTransform;

        private void Awake()
        {
            _wheelTransform = GetComponent<RectTransform>();
            _tweenFactory = new WheelSpinTweenFactory(_wheelTransform);

            if (_wheelTransform != null)
            {
                _baseScale = _wheelTransform.localScale;
            }
        }

        private void OnDisable()
        {
            StopActiveTween(false);
            _spinning = false;
            RestoreBaseScale();
        }

        private void LateUpdate()
        {
            if (_spinPresentation == null || _wheelTransform == null) return;

            _spinPresentation.ApplyUprightSlicePresentations(_wheelTransform.localEulerAngles.z);
        }

        public void Bind(WheelGameSettings settings, WheelSpinPresentationChannel spinPresentation)
        {
            Bind(settings, spinPresentation, UnityRandomSource.Shared);
        }

        public void Bind(WheelGameSettings settings, WheelSpinPresentationChannel spinPresentation, IRandomSource randomSource)
        {
            _settings = settings;
            _spinPresentation = spinPresentation;
            _randomSource = randomSource ?? UnityRandomSource.Shared;
        }

        public void Unbind()
        {
            StopActiveTween(false);

            _spinning = false;
            _pendingResult = default;
            _lastTargetAngle = 0f;
            _settings = null;
            _spinPresentation = null;
            _randomSource = UnityRandomSource.Shared;

            SpinStarted = null;
            SuspenseTicked = null;
            LandingStarted = null;
            SpinCompleted = null;

            RestoreBaseScale();
        }

        public void AcceptSlices(WheelSliceDefinition[] slices, int count)
        {
            if (slices == null) throw new ArgumentNullException(nameof(slices));
            if (count < 0 || count > slices.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            EnsureSliceBuffer(count);
            _currentSliceCount = count;
            for (int i = 0; i < count; i++)
            {
                WheelSliceCopyUtility.CopyInto(slices[i], _sliceBuffer[i]);
            }

            _currentSlices = _sliceBuffer;
        }

        public void Spin(int selectedIndex)
        {
            if (!TryBeginSpin(selectedIndex, out string failureReason))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (failureReason != null)
                {
                    Debug.LogWarning($"Cannot spin: {failureReason}");
                }
#endif
                return;
            }

            PlaySpin(selectedIndex);
        }

#if UNITY_EDITOR
        // Editor-only seams. Visible to Assembly-CSharp-Editor (tests + designer tools)
        // via [InternalsVisibleTo("Assembly-CSharp-Editor")] in WheelAssemblyInfo.cs.
        internal void RaiseLandingStarted(WheelSpinResult result)
        {
            LandingStarted?.Invoke(result);
        }

        internal void RaiseSpinCompleted(WheelSpinResult result)
        {
            SpinCompleted?.Invoke(result);
        }

        internal void SnapToSelection(int selectedIndex)
        {
            if (_wheelTransform == null || selectedIndex < 0 || selectedIndex >= _currentSliceCount) return;
            if (!TryResolveSlicePointerAngles(out int pointerAngleCount)) return;
            if (pointerAngleCount <= selectedIndex) return;

            float startAngle = GetCurrentWheelAngle();
            float targetAngle = startAngle + Mathf.DeltaAngle(startAngle, _pointerAngleBuffer[selectedIndex]);
            _tweenFactory.ApplyAngle(targetAngle);
        }
#endif

        private bool TryBeginSpin(int selectedIndex, out string failureReason)
        {
            if (_spinning)
            {
                failureReason = "already spinning";
                return false;
            }

            if (_wheelTransform == null)
            {
                failureReason = "missing wheel transform";
                return false;
            }

            if (_currentSlices == null || _currentSliceCount <= 0)
            {
                failureReason = "no slices accepted";
                return false;
            }

            if (selectedIndex < 0 || selectedIndex >= _currentSliceCount)
            {
                failureReason = "invalid selectedIndex";
                return false;
            }

            if (_currentSliceCount > _currentSlices.Length)
            {
                failureReason = "slice buffer mismatch";
                return false;
            }

            if (_settings == null)
            {
                failureReason = "missing settings";
                return false;
            }

            failureReason = null;
            return true;
        }

        private void PlaySpin(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _currentSliceCount) return;

            StopActiveTween(false);
            RestoreBaseScale();

            if (_settings == null) return;
            if (!TryResolveSlicePointerAngles(out int pointerAngleCount)) return;
            if (pointerAngleCount <= selectedIndex) return;

            _spinning = true;
            _pendingResult = new WheelSpinResult(selectedIndex, _currentSlices[selectedIndex]);

            WheelSpinPlan plan = CreateSpinPlan(selectedIndex, _pointerAngleBuffer, _settings);
            _lastTargetAngle = plan.TargetAngle;

            var callbacks = new WheelSpinSequenceBuilder.Callbacks(
                onSpinStarted: DispatchSpinStarted,
                onSuspenseTick: DispatchSuspenseTick,
                onLandingStarted: DispatchLandingStarted,
                onComplete: CompleteSpin,
                onKilled: RestoreBaseScale);

            _activeTween = WheelSpinSequenceBuilder.Build(
                gameObject,
                plan,
                _pointerAngleBuffer,
                _settings.SpinEase,
                _tweenFactory,
                callbacks);
        }

        private WheelSpinPlan CreateSpinPlan(int selectedIndex, float[] pointerAngles, WheelGameSettings settings)
        {
            float startAngle = GetCurrentWheelAngle();
            int suspenseSlotCount = WheelSpinAnglePlanner.ResolveSuspenseSlotCount(
                _currentSliceCount,
                _randomSource.Range(0, WheelSpinTiming.ExtraRandomSuspenseSlots + 1));

            return WheelSpinAnglePlanner.PlanSpin(
                selectedIndex,
                _currentSliceCount,
                startAngle,
                pointerAngles,
                settings,
                suspenseSlotCount);
        }

        private bool TryResolveSlicePointerAngles(out int count)
        {
            count = 0;
            if (_spinPresentation == null || _currentSliceCount <= 0) return false;

            EnsurePointerAngleBuffer(_currentSliceCount);
            if (!_spinPresentation.CopySlicePointerAngles(_currentSliceCount, _pointerAngleBuffer).Succeeded)
            {
                return false;
            }

            count = _currentSliceCount;
            return true;
        }

        private void DispatchSpinStarted()
        {
            SpinStarted?.Invoke();
        }

        private void DispatchSuspenseTick(int sliceIndex)
        {
            SuspenseTicked?.Invoke(sliceIndex);
        }

        private void DispatchLandingStarted()
        {
            _tweenFactory.ApplyAngle(_lastTargetAngle);
            LandingStarted?.Invoke(_pendingResult);
        }

        private void CompleteSpin()
        {
            _activeTween = null;

            _tweenFactory.ApplyAngle(_lastTargetAngle);
            RestoreBaseScale();

            _spinning = false;
            SpinCompleted?.Invoke(_pendingResult);
        }

        private float GetCurrentWheelAngle()
        {
            if (_wheelTransform == null) return 0f;

            return WheelSpinAnglePlanner.NormalizeAngle(_wheelTransform.eulerAngles.z);
        }

        private void RestoreBaseScale()
        {
            if (_wheelTransform == null) return;

            _wheelTransform.localScale = _baseScale;
        }

        private void StopActiveTween(bool complete)
        {
            if (_activeTween != null && _activeTween.IsActive()) _activeTween.Kill(complete);

            _activeTween = null;
        }

        private void EnsureSliceBuffer(int capacity)
        {
            if (_sliceBuffer.Length == capacity && _sliceBuffer.Length > 0 && _sliceBuffer[0] != null) return;

            _sliceBuffer = new WheelSliceDefinition[capacity];
            for (int i = 0; i < capacity; i++)
            {
                _sliceBuffer[i] = new WheelSliceDefinition();
            }
        }

        private void EnsurePointerAngleBuffer(int capacity)
        {
            if (_pointerAngleBuffer.Length >= capacity) return;

            _pointerAngleBuffer = new float[capacity];
        }
    }
}
