using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelSpinResolveProfile
    {
        [SerializeField] private WheelGamePhase _targetPhase;
        [SerializeField] private bool _clearInventory;
        [SerializeField] private bool _advanceZone;
        [SerializeField] private bool _markSlicesDirty;
        [SerializeField] private bool _addResultToInventory;

        public WheelGamePhase TargetPhase => _targetPhase;
        public bool ClearInventory => _clearInventory;
        public bool AdvanceZone => _advanceZone;
        public bool MarkSlicesDirty => _markSlicesDirty;
        public bool AddResultToInventory => _addResultToInventory;

        public static WheelSpinResolveProfile Create(
            WheelGamePhase targetPhase,
            bool clearInventory = false,
            bool advanceZone = false,
            bool markSlicesDirty = false,
            bool addResultToInventory = false)
        {
            return new WheelSpinResolveProfile
            {
                _targetPhase = targetPhase,
                _clearInventory = clearInventory,
                _advanceZone = advanceZone,
                _markSlicesDirty = markSlicesDirty,
                _addResultToInventory = addResultToInventory
            };
        }
    }
}
