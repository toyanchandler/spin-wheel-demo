using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelSliceSlotProfile
    {
        public string label;
        public int displayAmount;
        public Color displayColor;
        public bool showAmountLabel;
    }
}
