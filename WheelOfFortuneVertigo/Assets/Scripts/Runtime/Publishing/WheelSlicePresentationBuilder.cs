using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelSlicePresentationBuilder
    {
        public static WheelSlicePresentation[] Build(WheelSliceDefinition[] slices, int sliceCount)
        {
            if (sliceCount <= 0) return System.Array.Empty<WheelSlicePresentation>();

            var presentations = new WheelSlicePresentation[sliceCount];
            for (int i = 0; i < sliceCount; i++)
            {
                WheelSliceDefinition slice = slices[i];
                presentations[i] = new WheelSlicePresentation(
                    slice.DisplayIcon,
                    slice.DisplayColor,
                    slice.ShowAmountLabel,
                    FormatAmountText(slice.DisplayAmount, slice.ShowAmountLabel));
            }

            return presentations;
        }

        private static string FormatAmountText(int amount, bool showAmountLabel)
        {
            return showAmountLabel ? amount.ToString() : string.Empty;
        }
    }
}
