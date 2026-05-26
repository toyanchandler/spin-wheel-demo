using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelSpinner : MonoBehaviour
    {
        public event Action SpinStarted;
        public event Action<int> SuspenseTicked;
        public event Action<WheelSpinResult> LandingStarted;
        public event Action<WheelSpinResult> SpinCompleted;

        private static readonly WheelSliceDefinition[] EmptySlices = new WheelSliceDefinition[0];
        private static readonly float[] EmptyAngles = new float[0];

        private const float AnticipationDelay = 0.03f;
        private const float AnticipationPullDuration = 0.14f;
        private const float AnticipationChargeDuration = 0.08f;
        private const float AnticipationReleaseDuration = 0.07f;
        private const float AnticipationPullDegrees = -14f;
        private const float AnticipationLaunchKickDegrees = 7f;

        private const float MinimumFastSpinDuration = 0.70f;
        private const float MinimumFastRotationBeforeSuspenseDegrees = 540f;

        private const int MinimumSuspenseSlots = 22;
        private const int ExtraRandomSuspenseSlots = 7;
        private const float SuspenseDuration = 3.90f;
        private const float SuspenseEasePower = 1.95f;

        private const float LandingPunchDuration = 0.18f;
        private const float LandingPunchScale = 0.045f;
        private const float PostLandingHoldDuration = 0.04f;

        private RectTransform _wheelTransform;
        private WheelSliceDefinition[] _currentSlices = EmptySlices;
        private Tween _activeTween;
        private WheelSpinResult _pendingResult;
        private Vector3 _baseScale = Vector3.one;

        private int _currentSliceCount;
        private bool _spinning;
        private float _lastTargetAngle;

        public bool IsSpinning
        {
            get { return _spinning; }
        }

        public int CurrentSliceCount
        {
            get { return _currentSliceCount; }
        }

        public RectTransform WheelTransform
        {
            get { return _wheelTransform; }
        }

        private void Awake()
        {
            _wheelTransform = GetComponent<RectTransform>();

            if (_wheelTransform != null)
            {
                _baseScale = _wheelTransform.localScale;
            }

            WheelRuntimeLocator.RegisterSpinner(this);
        }

        private void OnDisable()
        {
            StopActiveTween(false);
            _spinning = false;
            RestoreBaseScale();
        }

        private void LateUpdate()
        {
            WheelView wheelView = WheelRuntimeLocator.WheelView;
            if (wheelView == null || _wheelTransform == null)
            {
                return;
            }

            wheelView.ApplyUprightSlicePresentations(_wheelTransform.localEulerAngles.z);
        }

        public void Unbind()
        {
            StopActiveTween(false);

            _spinning = false;
            _pendingResult = default;
            _lastTargetAngle = 0f;

            SpinStarted = null;
            SuspenseTicked = null;
            LandingStarted = null;
            SpinCompleted = null;

            RestoreBaseScale();
        }

        public void SetSlices(WheelSliceDefinition[] slices, int sliceCount)
        {
            _currentSlices = slices ?? EmptySlices;
            _currentSliceCount = Mathf.Clamp(sliceCount, 0, _currentSlices.Length);
        }

        public void Spin()
        {
            if (!CanSpin())
            {
                return;
            }

            BeginSpin();
        }

#if UNITY_EDITOR
        public void SnapToSelectionForDebug(int selectedIndex)
        {
            if (_wheelTransform == null || selectedIndex < 0 || selectedIndex >= _currentSliceCount)
            {
                return;
            }

            float[] pointerAngles = ResolveSlicePointerAngles();
            if (pointerAngles.Length <= selectedIndex)
            {
                return;
            }

            float startAngle = GetCurrentWheelAngle();
            float targetAngle = startAngle + Mathf.DeltaAngle(startAngle, pointerAngles[selectedIndex]);

            ApplyWheelAngle(targetAngle);
        }
#endif

        private bool CanSpin()
        {
            return !_spinning
                && _wheelTransform != null
                && _currentSlices != null
                && _currentSliceCount > 0
                && _currentSliceCount <= _currentSlices.Length
                && WheelRuntimeLocator.Settings != null;
        }

        private void BeginSpin()
        {
            int selectedIndex = UnityEngine.Random.Range(0, _currentSliceCount);
            PlaySpin(selectedIndex);
        }

        private void PlaySpin(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _currentSliceCount)
            {
                return;
            }

            StopActiveTween(false);
            RestoreBaseScale();

            WheelGameSettings settings = WheelRuntimeLocator.Settings;
            if (settings == null)
            {
                return;
            }

            float[] pointerAngles = ResolveSlicePointerAngles();
            if (pointerAngles.Length <= selectedIndex)
            {
                return;
            }

            _spinning = true;
            _pendingResult = new WheelSpinResult(selectedIndex, _currentSlices[selectedIndex]);

            float startAngle = GetCurrentWheelAngle();
            float launchAngle = startAngle + AnticipationLaunchKickDegrees;
            float slotAngle = 360f / _currentSliceCount;

            int suspenseSlotCount = ResolveSuspenseSlotCount();
            float suspenseDegrees = slotAngle * suspenseSlotCount;

            float targetAngle = ResolveFinalTargetAngle(
                selectedIndex,
                startAngle,
                launchAngle,
                suspenseDegrees,
                pointerAngles,
                settings);

            float suspenseStartAngle = targetAngle - suspenseDegrees;
            float fastSpinDuration = ResolveFastSpinDuration(settings);

            _lastTargetAngle = targetAngle;

            Sequence spinSequence = DOTween.Sequence()
                .SetRecyclable(true)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);

            spinSequence
                .AppendCallback(DispatchSpinStarted)
                .AppendInterval(AnticipationDelay)
                .Append(CreateRotationTween(
                    startAngle,
                    startAngle + AnticipationPullDegrees,
                    AnticipationPullDuration,
                    Ease.OutQuad))
                .AppendInterval(AnticipationChargeDuration)
                .Append(CreateRotationTween(
                    startAngle + AnticipationPullDegrees,
                    launchAngle,
                    AnticipationReleaseDuration,
                    Ease.InQuad))
                .Append(CreateRotationTween(
                    launchAngle,
                    suspenseStartAngle,
                    fastSpinDuration,
                    Ease.Linear))
                .Append(CreateSmoothSuspenseRotation(
                    suspenseStartAngle,
                    targetAngle,
                    SuspenseDuration,
                    pointerAngles))
                .AppendCallback(DispatchLandingStarted)
                .Append(CreateLandingPunchTween())
                .AppendInterval(PostLandingHoldDuration)
                .OnComplete(CompleteSpin)
                .OnKill(HandleActiveTweenKilled);

            _activeTween = spinSequence;
        }

        private int ResolveSuspenseSlotCount()
        {
            int minimumBySliceCount = _currentSliceCount * 2 + 8;
            int minimum = Mathf.Max(MinimumSuspenseSlots, minimumBySliceCount);
            int extra = UnityEngine.Random.Range(0, ExtraRandomSuspenseSlots + 1);

            return minimum + extra;
        }

        private float ResolveFastSpinDuration(WheelGameSettings settings)
        {
            float configuredDuration = Mathf.Max(0f, settings.SpinDuration);

            float anticipationDuration =
                AnticipationDelay
                + AnticipationPullDuration
                + AnticipationChargeDuration
                + AnticipationReleaseDuration;

            float remainingDuration = configuredDuration - anticipationDuration - SuspenseDuration;

            return Mathf.Max(MinimumFastSpinDuration, remainingDuration);
        }

        private float ResolveFinalTargetAngle(
            int selectedIndex,
            float startAngle,
            float launchAngle,
            float suspenseDegrees,
            float[] pointerAngles,
            WheelGameSettings settings)
        {
            float selectedPointerAngle = pointerAngles[selectedIndex];

            float targetDelta =
                360f * Mathf.Max(1, settings.MinimumSpinRounds)
                + Mathf.DeltaAngle(NormalizeAngle(startAngle), NormalizeAngle(selectedPointerAngle));

            float targetAngle = startAngle + targetDelta;

            float requiredDeltaFromLaunch =
                MinimumFastRotationBeforeSuspenseDegrees
                + suspenseDegrees;

            while (targetAngle - launchAngle < requiredDeltaFromLaunch)
            {
                targetAngle += 360f;
            }

            return targetAngle;
        }

        private float[] ResolveSlicePointerAngles()
        {
            WheelView wheelView = WheelRuntimeLocator.WheelView;
            if (wheelView == null || _currentSliceCount <= 0)
            {
                return EmptyAngles;
            }

            float[] pointerAngles = new float[_currentSliceCount];
            return wheelView.TryCopySlicePointerAngles(_currentSliceCount, pointerAngles) ? pointerAngles : EmptyAngles;
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
            int lastSliceIndex = ResolveNearestSliceIndex(fromAngle, pointerAngles);
            bool hasInitializedTickState = false;

            return DOVirtual
                .Float(0f, 1f, duration, normalizedTime =>
                {
                    float easedProgress = EvaluateSuspenseProgress(normalizedTime);
                    float currentAngle = Mathf.LerpUnclamped(fromAngle, toAngle, easedProgress);

                    ApplyWheelAngle(currentAngle);

                    int currentSliceIndex = ResolveNearestSliceIndex(currentAngle, pointerAngles);
                    if (!hasInitializedTickState)
                    {
                        lastSliceIndex = currentSliceIndex;
                        hasInitializedTickState = true;
                        return;
                    }

                    if (currentSliceIndex != lastSliceIndex)
                    {
                        lastSliceIndex = currentSliceIndex;
                        DispatchSuspenseTick(currentSliceIndex);
                    }
                })
                .SetEase(Ease.Linear)
                .SetTarget(_wheelTransform);
        }

        private float EvaluateSuspenseProgress(float normalizedTime)
        {
            float clampedTime = Mathf.Clamp01(normalizedTime);
            float inverse = 1f - clampedTime;

            return 1f - Mathf.Pow(inverse, SuspenseEasePower);
        }

        private int ResolveNearestSliceIndex(float wheelAngle, float[] pointerAngles)
        {
            if (pointerAngles == null || pointerAngles.Length == 0)
            {
                return -1;
            }

            float normalizedWheelAngle = NormalizeAngle(wheelAngle);
            int nearestIndex = 0;
            float nearestDistance = float.MaxValue;

            for (int i = 0; i < pointerAngles.Length; i++)
            {
                float distance = Mathf.Abs(Mathf.DeltaAngle(normalizedWheelAngle, pointerAngles[i]));
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        private Tween CreateLandingPunchTween()
        {
            if (_wheelTransform == null)
            {
                return DOVirtual.DelayedCall(0f, () => { });
            }

            return _wheelTransform
                .DOPunchScale(
                    new Vector3(LandingPunchScale, LandingPunchScale, 0f),
                    LandingPunchDuration,
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

        private void HandleActiveTweenKilled()
        {
            if (_activeTween == null)
            {
                return;
            }

            if (!_activeTween.IsComplete())
            {
                RestoreBaseScale();
            }
        }

        private float GetCurrentWheelAngle()
        {
            if (_wheelTransform == null)
            {
                return 0f;
            }

            return NormalizeAngle(_wheelTransform.eulerAngles.z);
        }

        private void ApplyWheelAngle(float angle)
        {
            if (_wheelTransform == null)
            {
                return;
            }

            _wheelTransform.eulerAngles = new Vector3(0f, 0f, angle);
        }

        private float NormalizeAngle(float angle)
        {
            return Mathf.Repeat(angle, 360f);
        }

        private void RestoreBaseScale()
        {
            if (_wheelTransform == null)
            {
                return;
            }

            _wheelTransform.localScale = _baseScale;
        }

        private void StopActiveTween(bool complete)
        {
            if (_activeTween != null && _activeTween.IsActive())
            {
                _activeTween.Kill(complete);
            }

            _activeTween = null;
        }
    }
}
