namespace Vertigo.Wheel.Data
{
    public static class WheelSliceCopyUtility
    {
        public static void CopyInto(WheelSliceDefinition source, WheelSliceDefinition destination)
        {
            destination.ApplySlot(
                source.IsBomb,
                source.Reward,
                source.DisplayIcon,
                source.DisplayAmount,
                source.DisplayColor,
                source.DisplayLabel,
                source.ShowAmountLabel);
        }

        public static WheelSliceDefinition CreateCopy(WheelSliceDefinition source)
        {
            var copy = new WheelSliceDefinition();
            CopyInto(source, copy);
            return copy;
        }
    }
}
