using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelZoneIntervalRule
    {
        [SerializeField] private int _interval;
        [SerializeField] private ZoneType _zoneType;

        public int Interval => _interval;
        public ZoneType ZoneType => _zoneType;

        public static WheelZoneIntervalRule Create(int interval, ZoneType zoneType)
        {
            return new WheelZoneIntervalRule
            {
                _interval = interval,
                _zoneType = zoneType
            };
        }
    }
}
