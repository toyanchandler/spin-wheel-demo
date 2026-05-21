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

        public WheelGamePhase TargetPhase { get { return _targetPhase; } }
        public bool ClearInventory { get { return _clearInventory; } }
        public bool AdvanceZone { get { return _advanceZone; } }
        public bool MarkSlicesDirty { get { return _markSlicesDirty; } }
        public bool AddResultToInventory { get { return _addResultToInventory; } }

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
