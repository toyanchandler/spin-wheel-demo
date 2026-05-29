using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelSpinAngleReader
    {
        public static WheelSpinAngleSample ReadWhileSpinning(WheelSpinPresentationChannel spin)
        {
            if (spin == null || !spin.IsSpinning || spin.WheelTransform == null)
            {
                return WheelSpinAngleSample.Invalid;
            }

            float wheelAngle = Mathf.Repeat(spin.WheelTransform.eulerAngles.z, 360f);
            return new WheelSpinAngleSample(wheelAngle, spin.CurrentSliceCount);
        }

        public static WheelSpinAngleSample ReadWheelAngle(RectTransform wheelTransform)
        {
            if (wheelTransform == null) return WheelSpinAngleSample.Invalid;

            float wheelAngle = Mathf.Repeat(wheelTransform.eulerAngles.z, 360f);
            return new WheelSpinAngleSample(wheelAngle, 0);
        }
    }
}
