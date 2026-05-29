using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelIndicatorSpinPresenter
    {
        private RectTransform _rectTransform;
        private float _indicatorBaseRotation;
        private float _indicatorTickOffset;
        private bool _hasIndicatorBaseRotation;
        private WheelIndicatorSpinTickState _tickState;

        public void Bind(RectTransform rectTransform)
        {
            _rectTransform = rectTransform;
            CaptureIndicatorBaseRotationOnce();
        }

        public void ResetTickTracking()
        {
            _tickState.ClearSample();
        }

        public void LateUpdate(WheelSpinPresentationChannel spinChannel)
        {
            WheelSpinAngleSample sample = WheelSpinAngleReader.ReadWhileSpinning(spinChannel);
            if (!sample.IsValid)
            {
                ResetTickTracking();
                EaseTickOffsetTowardZero();
                return;
            }

            ApplySpinTick(sample.WheelAngle, sample.SliceCount);
        }

        public void ApplyLandingTick(WheelSpinPresentationChannel spinChannel)
        {
            WheelSpinAngleSample sample = WheelSpinAngleReader.ReadWheelAngle(spinChannel.WheelTransform);
            if (!sample.IsValid)
            {
                return;
            }

            float direction = WheelIndicatorSpinTickSimulator.ResolveSpinDirection(_tickState.LastWheelAngle, sample.WheelAngle);
            float offset = direction * WheelIndicatorSpinTickSimulator.TickAngleDegrees;
            SetIndicatorTickOffset(offset);
        }

        public void SetIndicatorTickOffset(float offset)
        {
            _indicatorTickOffset = offset;
            if (_rectTransform != null)
            {
                _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _indicatorBaseRotation + _indicatorTickOffset);
            }
        }

        private void ApplySpinTick(float wheelAngle, int sliceCount)
        {
            WheelIndicatorSpinTickResult result =
                WheelIndicatorSpinTickSimulator.UpdateWhileSpinning(wheelAngle, sliceCount, ref _tickState);

            if (result.DidTick)
            {
                SetIndicatorTickOffset(result.OffsetDegrees);
                return;
            }

            EaseTickOffsetTowardZero();
        }

        private void EaseTickOffsetTowardZero()
        {
            float nextOffset = WheelIndicatorSpinTickSimulator.AdvanceReturnTowardZero(_indicatorTickOffset, Time.deltaTime);
            SetIndicatorTickOffset(nextOffset);
        }

        private void CaptureIndicatorBaseRotationOnce()
        {
            if (_rectTransform == null || _hasIndicatorBaseRotation)
            {
                return;
            }

            _indicatorBaseRotation = WheelAngleUtility.NormalizeSignedAngle(_rectTransform.localEulerAngles.z);
            _hasIndicatorBaseRotation = true;
        }
    }
}
