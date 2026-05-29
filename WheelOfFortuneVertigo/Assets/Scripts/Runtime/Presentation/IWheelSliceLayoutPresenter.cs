namespace Vertigo.Wheel.Runtime
{
    /// <summary>Wheel slice layout queries used by the spin driver. Implemented by <c>WheelView</c>.</summary>
    public interface IWheelSliceLayoutPresenter
    {
        void ApplyUprightSlicePresentations(float wheelLocalEulerZ);
        WheelSlicePointerAnglesCopy CopySlicePointerAngles(int sliceCount, float[] pointerAngles);
    }
}
