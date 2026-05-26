using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public enum WheelZoneProgressCellKind
    {
        Standard,
        Safe,
        Super
    }

    public sealed class WheelZoneProgressCellView : MonoBehaviour
    {
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _valueText;

        private bool _hasLastZone;
        private int _lastZone;

        public void Apply(in WheelZoneProgressWindow window, int slotIndex)
        {
            int zone = window.ZoneAtSlot(slotIndex);
            bool isValid = zone > 0;
            gameObject.SetActive(isValid);
            if (!isValid)
            {
                return;
            }

            WheelZoneProgressCellKind kind = ResolveKind(window.Snapshot, zone);
            SetTextIfChanged(zone);
            SetFrame(kind, zone == window.Snapshot.Zone, window.Snapshot);
        }

        public bool IsWired()
        {
            return _frameImage != null
                && _valueText != null;
        }

        private static WheelZoneProgressCellKind ResolveKind(WheelHudSnapshot snapshot, int zone)
        {
            if (IsIntervalZone(zone, snapshot.SuperZoneInterval))
            {
                return WheelZoneProgressCellKind.Super;
            }

            return IsIntervalZone(zone, snapshot.SafeZoneInterval)
                ? WheelZoneProgressCellKind.Safe
                : WheelZoneProgressCellKind.Standard;
        }

        private static bool IsIntervalZone(int zone, int interval)
        {
            return interval > 0 && zone % interval == 0;
        }

        private void SetTextIfChanged(int zone)
        {
            if (_hasLastZone && zone == _lastZone)
            {
                return;
            }

            _valueText.SetText("{0}", zone);
            _hasLastZone = true;
            _lastZone = zone;
        }

        private void SetFrame(WheelZoneProgressCellKind kind, bool isCurrentZone, WheelHudSnapshot snapshot)
        {
            bool isFramed = isCurrentZone || kind == WheelZoneProgressCellKind.Safe || kind == WheelZoneProgressCellKind.Super;
            if (_frameImage.gameObject.activeSelf != isFramed)
            {
                _frameImage.gameObject.SetActive(isFramed);
            }

            if (!isFramed)
            {
                return;
            }

            Color frameColor = ResolveFrameColor(kind, isCurrentZone, snapshot);
            if (_frameImage.color != frameColor)
            {
                _frameImage.color = frameColor;
            }
        }

        private static Color ResolveFrameColor(
            WheelZoneProgressCellKind kind,
            bool isCurrentZone,
            WheelHudSnapshot snapshot)
        {
            if (kind == WheelZoneProgressCellKind.Super)
            {
                return snapshot.SuperMilestoneBadgeColor;
            }

            if (kind == WheelZoneProgressCellKind.Safe)
            {
                return snapshot.SafeMilestoneBadgeColor;
            }

            return isCurrentZone
                ? new Color(1f, 1f, 1f, 0.92f)
                : snapshot.SafeMilestoneBadgeColor;
        }
    }
}
