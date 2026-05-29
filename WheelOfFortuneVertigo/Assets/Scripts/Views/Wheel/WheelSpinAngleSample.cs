namespace Vertigo.Wheel.Views
{
    internal readonly struct WheelSpinAngleSample
    {
        public readonly bool IsValid;
        public readonly float WheelAngle;
        public readonly int SliceCount;

        public WheelSpinAngleSample(float wheelAngle, int sliceCount)
        {
            IsValid = true;
            WheelAngle = wheelAngle;
            SliceCount = sliceCount;
        }

        public static WheelSpinAngleSample Invalid => default;
    }
}
