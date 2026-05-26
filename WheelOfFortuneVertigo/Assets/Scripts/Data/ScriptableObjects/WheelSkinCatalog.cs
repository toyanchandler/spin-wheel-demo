using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public struct WheelSkinTierSprites
    {
        [SerializeField] private Sprite _wheelBase;
        [SerializeField] private Sprite _indicator;

        public Sprite WheelBase { get { return _wheelBase; } }
        public Sprite Indicator { get { return _indicator; } }

        public static WheelSkinTierSprites Create(Sprite wheelBase, Sprite indicator)
        {
            return new WheelSkinTierSprites
            {
                _wheelBase = wheelBase,
                _indicator = indicator
            };
        }
    }

    [CreateAssetMenu(fileName = "WheelSkinCatalog", menuName = "Vertigo/Wheel Skin Catalog")]
    public sealed class WheelSkinCatalog : ScriptableObject
    {
        [SerializeField] private WheelSkinTierSprites[] _spritesByTier = CreateDefaultSprites();

        public Sprite GetWheelBase(WheelSkinTier tier)
        {
            return _spritesByTier[(int)tier].WheelBase;
        }

        public Sprite GetIndicator(WheelSkinTier tier)
        {
            return _spritesByTier[(int)tier].Indicator;
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
