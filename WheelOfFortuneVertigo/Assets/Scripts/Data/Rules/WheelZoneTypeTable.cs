namespace Vertigo.Wheel.Data
{
    public static class WheelZoneTypeTable
    {
        public static ZoneType Resolve(int zone, WheelZoneIntervalRule[] rules, ZoneType fallback)
        {
            for (int i = 0; i < rules.Length; i++)
            {
                if (zone % rules[i].interval == 0)
                {
                    return rules[i].zoneType;
                }
            }

            return fallback;
        }
    }
}
