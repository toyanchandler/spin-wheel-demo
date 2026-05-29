namespace Vertigo.Wheel.Views
{
    internal static class WheelSliceArrayLookup
    {
        public static WheelSliceView Get(WheelSliceView[] sliceViews, int sliceIndex)
        {
            if (sliceViews == null || sliceIndex < 0 || sliceIndex >= sliceViews.Length) return null;

            return sliceViews[sliceIndex];
        }

        public static WheelSliceView GetActive(WheelSliceView[] sliceViews, int sliceIndex)
        {
            WheelSliceView sliceView = Get(sliceViews, sliceIndex);
            if (sliceView == null || !sliceView.gameObject.activeInHierarchy) return null;

            return sliceView;
        }
    }
}
