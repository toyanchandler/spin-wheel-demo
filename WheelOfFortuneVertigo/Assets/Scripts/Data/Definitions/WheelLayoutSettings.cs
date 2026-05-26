using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelLayoutSettings
    {
        [SerializeField] private Sprite _rewardCardFrameSprite;

        public Sprite RewardCardFrameSprite { get { return _rewardCardFrameSprite; } }

        public void BindRewardCardFrameSprite(Sprite rewardCardFrameSprite)
        {
            _rewardCardFrameSprite = rewardCardFrameSprite;
        }

        public static WheelLayoutSettings Default()
        {
            return new WheelLayoutSettings();
        }
    }
}
