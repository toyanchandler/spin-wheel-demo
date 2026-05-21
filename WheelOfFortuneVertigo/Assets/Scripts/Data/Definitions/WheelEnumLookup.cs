using System;

namespace Vertigo.Wheel.Data
{
    public static class WheelEnumLookup
    {
        public static T[] Build<T>(T[] source, Func<T, int> indexForEntry, int length)
        {
            var table = new T[length];
            for (int i = 0; i < source.Length; i++)
            {
                int index = indexForEntry(source[i]);
                table[index] = source[i];
            }

            return table;
        }
    }
}
