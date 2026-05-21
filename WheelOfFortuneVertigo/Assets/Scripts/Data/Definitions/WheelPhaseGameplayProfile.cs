using System;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelPhaseGameplayProfile
    {
        public bool allowSpin;
        public bool allowLeave;
        public bool allowRestart;
        public bool publishAllAfterSpin;
        public bool useWinLabelForStatus;
    }
}
