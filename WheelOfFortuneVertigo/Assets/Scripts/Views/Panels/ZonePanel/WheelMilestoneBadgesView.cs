using TMPro;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class WheelMilestoneBadgesView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _superBadgeNumberText;
        [SerializeField] private TextMeshProUGUI _safeBadgeNumberText;

        [WheelInject] private WheelEventBus _eventBus;

        private int _lastSuperZoneInterval = -1;
        private int _lastSafeZoneInterval = -1;

        [WheelAfterInject]
        private void Connect()
        {
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            WheelHudMilestoneSnapshot milestones = snapshot.Milestones;
            SetIntervalText(_superBadgeNumberText, milestones.DisplaySuperZoneInterval, ref _lastSuperZoneInterval);
            SetIntervalText(_safeBadgeNumberText, milestones.DisplaySafeZoneInterval, ref _lastSafeZoneInterval);
        }

        private static void SetIntervalText(TextMeshProUGUI text, int interval, ref int lastInterval)
        {
            if (text == null || interval == lastInterval) return;
            text.SetText("{0}", interval);
            lastInterval = interval;
        }
    }
}
