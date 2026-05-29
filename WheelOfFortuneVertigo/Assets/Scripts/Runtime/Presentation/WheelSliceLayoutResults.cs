namespace Vertigo.Wheel.Runtime
{
    /// <summary>Result of copying per-slice pointer angles into a caller-owned buffer.</summary>
    public readonly struct WheelSlicePointerAnglesCopy
    {
        public readonly bool Succeeded;

        public WheelSlicePointerAnglesCopy(bool succeeded)
        {
            Succeeded = succeeded;
        }

        public static WheelSlicePointerAnglesCopy Failed => new WheelSlicePointerAnglesCopy(false);
        public static WheelSlicePointerAnglesCopy Ok => new WheelSlicePointerAnglesCopy(true);
    }
}
