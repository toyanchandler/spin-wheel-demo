using System;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Spin animation contract used by <see cref="WheelGameFlowController"/>.
    /// Implementations must raise <see cref="LandingStarted"/> before <see cref="SpinCompleted"/>.
    /// </summary>
    public interface IWheelSpinDriver
    {
        event Action SpinStarted;
        event Action<int> SuspenseTicked;
        event Action<WheelSpinResult> LandingStarted;
        event Action<WheelSpinResult> SpinCompleted;

        bool IsSpinning { get; }
        int CurrentSliceCount { get; }
        RectTransform WheelTransform { get; }

        void Bind(WheelGameSettings settings, WheelSpinPresentationChannel spinPresentation);
        void Unbind();
        void AcceptSlices(WheelSliceDefinition[] slices, int count);
        void Spin(int selectedIndex);
    }
}
