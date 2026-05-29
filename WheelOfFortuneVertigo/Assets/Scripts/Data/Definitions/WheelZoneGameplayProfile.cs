using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelZoneGameplayProfile
    {
        [SerializeField] private bool _includesBombSlot;
        [SerializeField] private bool _allowLeave;

        public bool IncludesBombSlot => _includesBombSlot;
        public bool AllowLeave => _allowLeave;

        public static WheelZoneGameplayProfile Create(bool includesBombSlot, bool allowLeave)
        {
            return new WheelZoneGameplayProfile
            {
                _includesBombSlot = includesBombSlot,
                _allowLeave = allowLeave
            };
        }
    }
}
