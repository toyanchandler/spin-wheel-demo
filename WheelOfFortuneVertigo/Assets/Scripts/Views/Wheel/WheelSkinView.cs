using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public enum WheelSkinSlot
    {
        WheelBase,
        Indicator
    }

    [RequireComponent(typeof(Image))]
    public sealed class WheelSkinView : MonoBehaviour, IWheelRuntimeComponent
    {
        private delegate Sprite SkinSpriteResolver(WheelSkinCatalog catalog, WheelSkinTier tier);

        private static readonly SkinSpriteResolver[] SpriteResolvers =
        {
            (catalog, tier) => catalog.GetWheelBase(tier),
            (catalog, tier) => catalog.GetIndicator(tier)
        };

        [SerializeField] private WheelSkinCatalog _catalog;
        [SerializeField] private WheelSkinSlot _slot;
        [SerializeField] private Image _image;

        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.ZoneChanged += OnZoneChanged;
        }

        public void Dispose()
        {
            _eventBus.ZoneChanged -= OnZoneChanged;
            _eventBus = null;
        }

        private void OnZoneChanged(WheelZoneSnapshot snapshot)
        {
            _image.sprite = SpriteResolvers[(int)_slot](_catalog, snapshot.SkinTier);
        }
    }
}
