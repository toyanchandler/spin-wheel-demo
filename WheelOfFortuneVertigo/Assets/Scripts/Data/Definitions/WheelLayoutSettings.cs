using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [Serializable]
    public sealed class WheelLayoutSettings
    {
        [SerializeField] private Sprite _rewardCardFrameSprite;

        public Sprite RewardCardFrameSprite => _rewardCardFrameSprite;

        public void BindRewardCardFrameSprite(Sprite rewardCardFrameSprite)
        {
            _rewardCardFrameSprite = rewardCardFrameSprite;
        }

        public static WheelLayoutSettings Default() => new WheelLayoutSettings();
    }
}
