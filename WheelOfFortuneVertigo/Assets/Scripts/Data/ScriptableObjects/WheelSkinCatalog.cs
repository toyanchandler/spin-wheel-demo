using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelSkinTierSprites
    {
        public Sprite wheelBase;
        public Sprite indicator;
    }

    [CreateAssetMenu(fileName = "WheelSkinCatalog", menuName = "Vertigo/Wheel Skin Catalog")]
    public sealed class WheelSkinCatalog : ScriptableObject
    {
        [SerializeField] private WheelSkinTierSprites[] _spritesByTier = CreateDefaultSprites();

        public Sprite GetWheelBase(WheelSkinTier tier)
        {
            return _spritesByTier[(int)tier].wheelBase;
        }

        public Sprite GetIndicator(WheelSkinTier tier)
        {
            return _spritesByTier[(int)tier].indicator;
        }

        public void Configure(WheelSkinTierSprites bronze, WheelSkinTierSprites silver, WheelSkinTierSprites golden)
        {
            _spritesByTier = new[]
            {
                bronze,
                silver,
                golden
            };
        }

        private static WheelSkinTierSprites[] CreateDefaultSprites()
        {
            return new WheelSkinTierSprites[3];
        }
    }
}
