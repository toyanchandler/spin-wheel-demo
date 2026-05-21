using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelZoneGameplayProfile
    {
        [SerializeField] private bool _includesBombSlot;
        [SerializeField] private bool _allowLeave;

        public bool IncludesBombSlot { get { return _includesBombSlot; } }
        public bool AllowLeave { get { return _allowLeave; } }

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
