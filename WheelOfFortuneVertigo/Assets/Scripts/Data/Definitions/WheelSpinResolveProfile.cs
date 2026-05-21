using System;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelSpinResolveProfile
    {
        public WheelGamePhase targetPhase;
        public bool clearInventory;
        public bool advanceZone;
        public bool markSlicesDirty;
        public bool addResultToInventory;
    }
}
