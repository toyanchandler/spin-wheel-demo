using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZoneProgressView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private Image[] _cellImages;
        [SerializeField] private TextMeshProUGUI[] _cellLabels;
        [SerializeField] private Sprite _standardSprite;
        [SerializeField] private Sprite _currentSprite;
        [SerializeField] private Sprite _safeSprite;
        [SerializeField] private Sprite _superSprite;

        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Dispose()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            int halfCount = _cellLabels.Length / 2;
            int firstZone = Mathf.Max(1, snapshot.Zone - halfCount);

            for (int i = 0; i < _cellLabels.Length; i++)
            {
                int zone = firstZone + i;
                bool isCurrent = zone == snapshot.Zone;
                _cellLabels[i].SetText("{0}", zone);
                _cellLabels[i].color = ResolveTextColor(snapshot, zone, isCurrent);
                _cellImages[i].sprite = ResolveSprite(snapshot, zone, isCurrent);
                _cellImages[i].color = ResolveImageColor(snapshot, zone, isCurrent);
            }
        }

        private Sprite ResolveSprite(WheelHudSnapshot snapshot, int zone, bool isCurrent)
        {
            if (isCurrent)
            {
                return _currentSprite;
            }

            if (IsIntervalZone(zone, snapshot.SuperZoneInterval))
            {
                return _superSprite;
            }

            if (IsIntervalZone(zone, snapshot.SafeZoneInterval))
            {
                return _safeSprite;
            }

            return _standardSprite;
        }

        private static Color ResolveTextColor(WheelHudSnapshot snapshot, int zone, bool isCurrent)
        {
            if (isCurrent)
            {
                return snapshot.ZoneNumberColor;
            }

            if (IsIntervalZone(zone, snapshot.SuperZoneInterval))
            {
                return new Color(0.58f, 1f, 0.28f, 1f);
            }

            if (IsIntervalZone(zone, snapshot.SafeZoneInterval))
            {
                return new Color(0.45f, 0.9f, 1f, 1f);
            }

            return new Color(0.8f, 0.82f, 0.86f, 0.82f);
        }

        private static Color ResolveImageColor(WheelHudSnapshot snapshot, int zone, bool isCurrent)
        {
            if (isCurrent)
            {
                return Color.white;
            }

            if (IsIntervalZone(zone, snapshot.SuperZoneInterval))
            {
                return new Color(0.65f, 1f, 0.25f, 0.72f);
            }

            if (IsIntervalZone(zone, snapshot.SafeZoneInterval))
            {
                return new Color(0.3f, 0.82f, 1f, 0.68f);
            }

            return new Color(1f, 1f, 1f, 0.42f);
        }

        private static bool IsIntervalZone(int zone, int interval)
        {
            return interval > 0 && zone % interval == 0;
        }
    }
}
