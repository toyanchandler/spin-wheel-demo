using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZonePanelView : MonoBehaviour
    {
        [SerializeField] private Image _panelImage;
        [SerializeField] private Image _mapFrameImage;

        private WheelEventBus _eventBus;

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _panelImage.sprite = snapshot.ZonePanelSprite;
            _panelImage.color = Color.clear;
            ApplyMapFrame(snapshot.ZoneMapFrameSprite);
        }

        private void ApplyMapFrame(Sprite mapFrameSprite)
        {
            int showFrame = System.Convert.ToInt32(mapFrameSprite != null);
            MapFrameActions[showFrame](_mapFrameImage, mapFrameSprite);
        }

        private static readonly System.Action<Image, Sprite>[] MapFrameActions =
        {
            (image, sprite) =>
            {
                image.sprite = null;
                image.color = Color.clear;
            },
            (image, sprite) =>
            {
                image.sprite = sprite;
                image.color = Color.clear;
            }
        };
    }
}
