namespace Vertigo.Wheel.Data
{
    public static class WheelSliceQueries
    {
        public static int FindFirst(WheelSliceDefinition[] slices, int sliceCount, bool isBomb)
        {
            for (int i = 0; i < sliceCount; i++)
            {
                if (slices[i].IsBomb == isBomb)
                {
                    return i;
                }
            }

            return -1;
        }

        public static int CountNonBomb(WheelSliceDefinition[] slices, int sliceCount)
        {
            int count = 0;
            for (int i = 0; i < sliceCount; i++)
            {
                if (!slices[i].IsBomb)
                {
                    count++;
                }
            }

            return count;
        }

        public static int SelectRandomRewardIndex(WheelSliceDefinition[] slices, int sliceCount, int randomRewardPick)
        {
            int rewardCount = CountNonBomb(slices, sliceCount);
            if (rewardCount <= 0)
            {
                return -1;
            }

            int selectedReward = randomRewardPick % rewardCount;
            for (int i = 0; i < sliceCount; i++)
            {
                if (slices[i].IsBomb)
                {
                    continue;
                }

                if (selectedReward == 0)
                {
                    return i;
                }

                selectedReward--;
            }

            return -1;
        }

        public static bool ContainsBomb(WheelSliceDefinition[] slices, int sliceCount)
        {
            return FindFirst(slices, sliceCount, true) >= 0;
        }
    }
}
