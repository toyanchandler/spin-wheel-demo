using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Read-only spin state + slice layout for wheel views. Driver registered from bootstrap.</summary>
    public sealed class WheelSpinPresentationChannel
    {
        private IWheelSpinDriver _driver;
        private IWheelSliceLayoutPresenter _sliceLayout;

        public void RegisterDriver(IWheelSpinDriver driver)
        {
            _driver = driver;
        }

        public void RegisterSliceLayout(IWheelSliceLayoutPresenter sliceLayout)
        {
            _sliceLayout = sliceLayout;
        }

        public void Clear()
        {
            _driver = null;
            _sliceLayout = null;
        }

        public bool IsSpinning => _driver != null && _driver.IsSpinning;

        public int CurrentSliceCount => _driver?.CurrentSliceCount ?? 0;

        public RectTransform WheelTransform => _driver?.WheelTransform;

        public void ApplyUprightSlicePresentations(float wheelLocalEulerZ)
        {
            _sliceLayout?.ApplyUprightSlicePresentations(wheelLocalEulerZ);
        }

        public WheelSlicePointerAnglesCopy CopySlicePointerAngles(int sliceCount, float[] pointerAngles)
        {
            return _sliceLayout == null
                ? WheelSlicePointerAnglesCopy.Failed
                : _sliceLayout.CopySlicePointerAngles(sliceCount, pointerAngles);
        }
    }
}
