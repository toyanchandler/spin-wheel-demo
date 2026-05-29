using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Plays the wheel spin DOTween sequence. Angle targets come from <see cref="WheelSpinAnglePlanner"/>.
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
        public void RaiseLandingStartedForTests(WheelSpinResult result)
        {
            LandingStarted?.Invoke(result);
        }

        public void RaiseSpinCompletedForTests(WheelSpinResult result)
        {
            SpinCompleted?.Invoke(result);
        }

        public void SnapToSelectionForDebug(int selectedIndex)
        {
            if (_wheelTransform == null || selectedIndex < 0 || selectedIndex >= _currentSliceCount) return;
            if (!TryResolveSlicePointerAngles(out int pointerAngleCount)) return;
            if (pointerAngleCount <= selectedIndex) return;

            float startAngle = GetCurrentWheelAngle();
            float targetAngle = startAngle + Mathf.DeltaAngle(startAngle, _pointerAngleBuffer[selectedIndex]);
            ApplyWheelAngle(targetAngle);
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
            _activeTween = BuildSpinSequence(plan, _pointerAngleBuffer, _settings.SpinEase);
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

        private Sequence BuildSpinSequence(WheelSpinPlan plan, float[] pointerAngles, Ease spinEase)
        {
            Sequence sequence = DOTween.Sequence()
                .SetRecyclable(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

            AppendAnticipationPhase(sequence, plan);
            AppendFastSpinPhase(sequence, plan, spinEase);
            AppendSuspensePhase(sequence, plan, pointerAngles);
            AppendLandingPhase(sequence);

            Tween activeSequence = sequence;
            sequence
                .OnComplete(CompleteSpin)
                .OnKill(() =>
                {
                    if (activeSequence != null && !activeSequence.IsComplete())
                    {
                        RestoreBaseScale();
                    }
                });

            return sequence;
        }

        private void AppendAnticipationPhase(Sequence sequence, WheelSpinPlan plan)
        {
            sequence
                .AppendCallback(DispatchSpinStarted)
                .AppendInterval(WheelSpinTiming.AnticipationDelay)
                .Append(CreateRotationTween(
                    plan.StartAngle,
                    plan.StartAngle + WheelSpinTiming.AnticipationPullDegrees,
                    WheelSpinTiming.AnticipationPullDuration,
                    Ease.OutQuad))
                .AppendInterval(WheelSpinTiming.AnticipationChargeDuration)
                .Append(CreateRotationTween(
                    plan.StartAngle + WheelSpinTiming.AnticipationPullDegrees,
                    plan.LaunchAngle,
                    WheelSpinTiming.AnticipationReleaseDuration,
                    Ease.InQuad));
        }

        private void AppendFastSpinPhase(Sequence sequence, WheelSpinPlan plan, Ease spinEase)
        {
            sequence.Append(CreateRotationTween(
                plan.LaunchAngle,
                plan.SuspenseStartAngle,
                plan.FastSpinDuration,
                spinEase));
        }

        private void AppendSuspensePhase(Sequence sequence, WheelSpinPlan plan, float[] pointerAngles)
        {
            sequence.Append(CreateSmoothSuspenseRotation(
                plan.SuspenseStartAngle,
                plan.TargetAngle,
                plan.SuspenseDuration,
                pointerAngles));
        }

        private void AppendLandingPhase(Sequence sequence)
        {
            sequence
                .AppendCallback(DispatchLandingStarted)
                .Append(CreateLandingPunchTween())
                .AppendInterval(WheelSpinTiming.PostLandingHoldDuration);
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

        private Tween CreateRotationTween(float fromAngle, float toAngle, float duration, Ease ease)
        {
            return DOVirtual
                .Float(fromAngle, toAngle, duration, ApplyWheelAngle)
                .SetEase(ease)
                .SetTarget(_wheelTransform);
        }

        private Tween CreateSmoothSuspenseRotation(
            float fromAngle,
            float toAngle,
            float duration,
            float[] pointerAngles)
        {
            var sliceTracker = new WheelSpinSuspenseSliceTracker();
            sliceTracker.Reset(fromAngle, pointerAngles);

            return DOVirtual
                .Float(0f, 1f, duration, normalizedTime =>
                {
                    float easedProgress = WheelSpinAnglePlanner.EvaluateSuspenseProgress(normalizedTime);
                    float currentAngle = Mathf.LerpUnclamped(fromAngle, toAngle, easedProgress);
                    ApplyWheelAngle(currentAngle);
                    WheelSliceCrossingResult crossing = sliceTracker.ConsumeCrossing(currentAngle, pointerAngles);
                    if (crossing.DidCross)
                    {
                        DispatchSuspenseTick(crossing.SliceIndex);
                    }
                })
                .SetEase(Ease.Linear)
                .SetTarget(_wheelTransform);
        }

        private Tween CreateLandingPunchTween()
        {
            if (_wheelTransform == null) return DOVirtual.DelayedCall(0f, () => { });

            return _wheelTransform
                .DOPunchScale(
                    new Vector3(WheelSpinTiming.LandingPunchScale, WheelSpinTiming.LandingPunchScale, 0f),
                    WheelSpinTiming.LandingPunchDuration,
                    8,
                    0.45f)
                .SetEase(Ease.OutQuad)
                .SetTarget(_wheelTransform);
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
            ApplyWheelAngle(_lastTargetAngle);
            LandingStarted?.Invoke(_pendingResult);
        }

        private void CompleteSpin()
        {
            _activeTween = null;

            ApplyWheelAngle(_lastTargetAngle);
            RestoreBaseScale();

            _spinning = false;
            SpinCompleted?.Invoke(_pendingResult);
        }

        private float GetCurrentWheelAngle()
        {
            if (_wheelTransform == null) return 0f;

            return WheelSpinAnglePlanner.NormalizeAngle(_wheelTransform.eulerAngles.z);
        }

        private void ApplyWheelAngle(float angle)
        {
            if (_wheelTransform == null) return;

            _wheelTransform.eulerAngles = new Vector3(0f, 0f, angle);
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
