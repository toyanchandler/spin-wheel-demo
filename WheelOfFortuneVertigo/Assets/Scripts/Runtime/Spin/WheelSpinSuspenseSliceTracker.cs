namespace Vertigo.Wheel.Runtime
{
    internal readonly struct WheelSliceCrossingResult
    {
        public bool DidCross { get; }
        public int SliceIndex { get; }

        public static WheelSliceCrossingResult None => new WheelSliceCrossingResult(false, -1);

        public static WheelSliceCrossingResult Crossed(int sliceIndex)
        {
            return new WheelSliceCrossingResult(true, sliceIndex);
        }

        private WheelSliceCrossingResult(bool didCross, int sliceIndex)
        {
            DidCross = didCross;
            SliceIndex = sliceIndex;
        }
    }

    /// <summary>Detects slice-index crossings during suspense rotation for tick events.</summary>
    internal sealed class WheelSpinSuspenseSliceTracker
    {
        private int _lastSliceIndex;
        private bool _hasInitialized;

        public void Reset(float startAngle, float[] pointerAngles)
        {
            _lastSliceIndex = WheelSpinAnglePlanner.ResolveNearestSliceIndex(startAngle, pointerAngles);
            _hasInitialized = true;
        }

        public WheelSliceCrossingResult ConsumeCrossing(float currentAngle, float[] pointerAngles)
        {
            int currentSliceIndex = WheelSpinAnglePlanner.ResolveNearestSliceIndex(currentAngle, pointerAngles);

            if (!_hasInitialized)
            {
                _lastSliceIndex = currentSliceIndex;
                _hasInitialized = true;
                return WheelSliceCrossingResult.None;
            }

            if (currentSliceIndex == _lastSliceIndex)
            {
                return WheelSliceCrossingResult.None;
            }

            _lastSliceIndex = currentSliceIndex;
            return WheelSliceCrossingResult.Crossed(currentSliceIndex);
        }
    }
}
