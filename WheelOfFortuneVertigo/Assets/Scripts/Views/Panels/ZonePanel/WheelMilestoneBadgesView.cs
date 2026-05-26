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
            SetIntervalText(_superBadgeNumberText, snapshot.SuperZoneInterval, ref _lastSuperZoneInterval);
            SetIntervalText(_safeBadgeNumberText, snapshot.SafeZoneInterval, ref _lastSafeZoneInterval);
        }

        private static void SetIntervalText(TextMeshProUGUI text, int interval, ref int lastInterval)
        {
            int safeInterval = Mathf.Max(1, interval);
            if (text == null || safeInterval == lastInterval)
            {
                return;
            }

            text.SetText("{0}", safeInterval);
            lastInterval = safeInterval;
        }
    }
}
