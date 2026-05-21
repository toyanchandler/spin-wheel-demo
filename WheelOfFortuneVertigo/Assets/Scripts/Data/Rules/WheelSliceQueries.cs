namespace Vertigo.Wheel.Data
{
    public static class WheelSliceQueries
    {
        public static int FindFirst(WheelSliceDefinition[] slices, int sliceCount, bool isBomb)
        {
            for (int i = 0; i < sliceCount; i++)
            {
                if (slices[i].isBomb == isBomb)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool ContainsBomb(WheelSliceDefinition[] slices, int sliceCount)
        {
            return FindFirst(slices, sliceCount, true) >= 0;
        }
    }
}
