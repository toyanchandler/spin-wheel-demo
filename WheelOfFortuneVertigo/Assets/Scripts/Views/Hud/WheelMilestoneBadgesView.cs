using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelMilestoneBadgesView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private Image _superBadgeImage;
        [SerializeField] private Image _safeBadgeImage;
        [SerializeField] private TextMeshProUGUI _superBadgeText;
        [SerializeField] private TextMeshProUGUI _safeBadgeText;

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
            _superBadgeText.SetText("SUPER\nZONE\n{0}", snapshot.NextSuperZone);
            _safeBadgeText.SetText("SAFE\nZONE\n{0}", snapshot.NextSafeZone);
            _superBadgeImage.color = new Color(0.2f, 0.65f, 0.08f, 0.86f);
            _safeBadgeImage.color = new Color(0.2f, 0.74f, 0.92f, 0.78f);
        }
    }
}
