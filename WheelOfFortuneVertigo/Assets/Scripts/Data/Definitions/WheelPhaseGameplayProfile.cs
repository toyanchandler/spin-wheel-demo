using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelPhaseGameplayProfile
    {
        [SerializeField] private bool _allowSpin;
        [SerializeField] private bool _allowLeave;
        [SerializeField] private bool _allowRestart;
        [SerializeField] private bool _publishAllAfterSpin;
        [SerializeField] private bool _useWinLabelForStatus;

        public bool AllowSpin { get { return _allowSpin; } }
        public bool AllowLeave { get { return _allowLeave; } }
        public bool AllowRestart { get { return _allowRestart; } }
        public bool PublishAllAfterSpin { get { return _publishAllAfterSpin; } }
        public bool UseWinLabelForStatus { get { return _useWinLabelForStatus; } }

        public static WheelPhaseGameplayProfile Create(
            bool allowSpin = false,
            bool allowLeave = false,
            bool allowRestart = false,
            bool publishAllAfterSpin = false,
            bool useWinLabelForStatus = false)
        {
            return new WheelPhaseGameplayProfile
            {
                _allowSpin = allowSpin,
                _allowLeave = allowLeave,
                _allowRestart = allowRestart,
                _publishAllAfterSpin = publishAllAfterSpin,
                _useWinLabelForStatus = useWinLabelForStatus
            };
        }
    }
}
