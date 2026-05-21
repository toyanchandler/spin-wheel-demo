using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelGameSettings", menuName = "Vertigo/Wheel Game Settings")]
    public sealed class WheelGameSettings : ScriptableObject, IWheelContentProvider
    {
        [Header("Zone Rules")]
        [SerializeField, Min(4)] private int _sliceCount = 8;
        [SerializeField, Min(1)] private int _safeZoneInterval = 5;
        [SerializeField, Min(1)] private int _superZoneInterval = 30;

        [Header("Rewards")]
        [SerializeField] private RewardDefinition _bombReward = new RewardDefinition();
        [SerializeField] private List<RewardDefinition> _standardRewards = new List<RewardDefinition>();
        [SerializeField] private List<RewardDefinition> _safeRewards = new List<RewardDefinition>();
        [SerializeField] private List<RewardDefinition> _superRewards = new List<RewardDefinition>();

        private List<RewardDefinition>[] _rewardPoolsByZone;
        private WheelZoneIntervalRule[] _zoneIntervalRules;

        [Header("Zone Gameplay")]
        [SerializeField] private WheelZoneGameplayProfile[] _zoneGameplayProfiles = CreateDefaultZoneGameplayProfiles();

        [Header("Spin Motion")]
        [SerializeField, Min(0.5f)] private float _spinDuration = 2.2f;
        [SerializeField, Min(1)] private int _minimumSpinRounds = 4;

        [Header("Presentation")]
        [SerializeField] private WheelUiCopyCatalog _uiCopy;
        [SerializeField] private WheelSkinCatalog _skinCatalog;
        [SerializeField] private WheelSliceLayoutCatalog _sliceLayoutCatalog;
        [SerializeField] private WheelOutcomePopupMotionCatalog _outcomePopupMotionCatalog;
        [SerializeField] private WheelSpinResolveCatalog _spinResolveCatalog;
        [SerializeField] private WheelLayoutSettings _layout = WheelLayoutSettings.Default();
        [SerializeField] private WheelThemeSettings _theme = WheelThemeSettings.Default();

        public WheelUiCopyCatalog UiCopy { get { return _uiCopy; } }
        public WheelSkinCatalog SkinCatalog { get { return _skinCatalog; } }
        public WheelSliceLayoutCatalog SliceLayoutCatalog { get { return _sliceLayoutCatalog; } }
        public WheelOutcomePopupMotionCatalog OutcomePopupMotionCatalog { get { return _outcomePopupMotionCatalog; } }
        public WheelSpinResolveCatalog SpinResolveCatalog { get { return _spinResolveCatalog; } }
        public float SpinDuration { get { return _spinDuration; } }
        public int MinimumSpinRounds { get { return _minimumSpinRounds; } }
        public WheelLayoutSettings Layout { get { return _layout; } }
        public WheelThemeSettings Theme { get { return _theme; } }

        public WheelZoneGameplayProfile GetZoneGameplay(ZoneType zoneType)
        {
            return _zoneGameplayProfiles[(int)zoneType];
        }

        public List<RewardDefinition> GetRewardPool(ZoneType zoneType)
        {
            return _rewardPoolsByZone[(int)zoneType];
        }

        public int SliceCount { get { return Mathf.Max(4, _sliceCount); } }
        public int SafeZoneInterval { get { return Mathf.Max(1, _safeZoneInterval); } }
        public int SuperZoneInterval { get { return Mathf.Max(1, _superZoneInterval); } }
        public RewardDefinition BombReward { get { return _bombReward; } }

        public void SetBombReward(RewardDefinition bombReward)
        {
            _bombReward = bombReward;
        }
        public List<RewardDefinition> StandardRewards { get { return _standardRewards; } }
        public List<RewardDefinition> SafeRewards { get { return _safeRewards; } }
        public List<RewardDefinition> SuperRewards { get { return _superRewards; } }

        public ZoneType GetZoneType(int zone)
        {
            return WheelZoneTypeTable.Resolve(zone, _zoneIntervalRules, ZoneType.Standard);
        }

        public void ConfigureSkinCatalog(WheelSkinCatalog skinCatalog)
        {
            _skinCatalog = skinCatalog;
        }

        public void ConfigureSliceLayoutCatalog(WheelSliceLayoutCatalog sliceLayoutCatalog)
        {
            _sliceLayoutCatalog = sliceLayoutCatalog;
        }

        public void ConfigureOutcomePopupMotionCatalog(WheelOutcomePopupMotionCatalog outcomePopupMotionCatalog)
        {
            _outcomePopupMotionCatalog = outcomePopupMotionCatalog;
        }

        public void ConfigureSpinResolveCatalog(WheelSpinResolveCatalog spinResolveCatalog)
        {
            _spinResolveCatalog = spinResolveCatalog;
        }

        public void InitializeRuntime()
        {
            _zoneIntervalRules = CreateZoneIntervalRules();
            _rewardPoolsByZone = new[] { _standardRewards, _safeRewards, _superRewards };
            _theme.SyncUiColorPalette();
            ValidateRuntimeConfig();
            RequireUiCopy();
            RequireSliceLayoutCatalog();
            RequireOutcomePopupMotionCatalog();
            RequireSpinResolveCatalog();
            string winLabelFormat = _uiCopy.WinLabelFormat;
            CacheRewardText(_bombReward, winLabelFormat);
            CacheRewardText(_standardRewards, winLabelFormat);
            CacheRewardText(_safeRewards, winLabelFormat);
            CacheRewardText(_superRewards, winLabelFormat);
        }

        public void ConfigureUiCopy(WheelUiCopyCatalog uiCopy)
        {
            _uiCopy = uiCopy;
        }

        public int FillSlicesForZone(int zone, WheelSliceDefinition[] buffer)
        {
            return WheelSliceGenerator.FillSlicesForZone(this, zone, buffer);
        }

        public void ResetToDefaults()
        {
            _sliceCount = 8;
            _safeZoneInterval = 5;
            _superZoneInterval = 30;
            _zoneGameplayProfiles = CreateDefaultZoneGameplayProfiles();
            _layout = WheelLayoutSettings.Default();
            _theme = WheelThemeSettings.Default();
        }

        private WheelZoneIntervalRule[] CreateZoneIntervalRules()
        {
            return new[]
            {
                WheelZoneIntervalRule.Create(_superZoneInterval, ZoneType.Super),
                WheelZoneIntervalRule.Create(_safeZoneInterval, ZoneType.Safe)
            };
        }

        private static WheelZoneGameplayProfile[] CreateDefaultZoneGameplayProfiles()
        {
            return new[]
            {
                WheelZoneGameplayProfile.Create(includesBombSlot: true, allowLeave: false),
                WheelZoneGameplayProfile.Create(includesBombSlot: false, allowLeave: true),
                WheelZoneGameplayProfile.Create(includesBombSlot: false, allowLeave: true)
            };
        }

        private void ValidateRuntimeConfig()
        {
            RequireReward(_bombReward, ZoneType.Standard);
            RequireRewards(_standardRewards, ZoneType.Standard);
            RequireRewards(_safeRewards, ZoneType.Safe);
            RequireRewards(_superRewards, ZoneType.Super);
        }

        private static void RequireRewards(List<RewardDefinition> rewards, ZoneType zoneType)
        {
            if (rewards == null || rewards.Count == 0)
            {
                throw new InvalidOperationException("WheelGameSettings requires at least one reward for " + zoneType + " zones.");
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                RequireReward(rewards[i], zoneType);
            }
        }

        private static void RequireReward(RewardDefinition reward, ZoneType zoneType)
        {
            if (reward == null)
            {
                throw new InvalidOperationException("WheelGameSettings has a null reward in " + zoneType + " configuration.");
            }

            if (string.IsNullOrEmpty(reward.Id))
            {
                throw new InvalidOperationException("WheelGameSettings reward in " + zoneType + " configuration requires a stable id.");
            }

            if (string.IsNullOrEmpty(reward.DisplayName))
            {
                throw new InvalidOperationException("WheelGameSettings reward '" + reward.Id + "' in " + zoneType + " configuration requires a display name.");
            }

            if (reward.Icon == null)
            {
                throw new InvalidOperationException("WheelGameSettings reward '" + reward.Id + "' in " + zoneType + " configuration requires an icon.");
            }
        }

        private void RequireUiCopy()
        {
            if (_uiCopy == null)
            {
                throw new InvalidOperationException("WheelGameSettings requires a WheelUiCopyCatalog asset.");
            }
        }

        private void RequireSliceLayoutCatalog()
        {
            if (_sliceLayoutCatalog == null)
            {
                throw new InvalidOperationException("WheelGameSettings requires a WheelSliceLayoutCatalog asset.");
            }

            _sliceLayoutCatalog.ValidateRuntime();
        }

        private void RequireOutcomePopupMotionCatalog()
        {
            if (_outcomePopupMotionCatalog == null)
            {
                throw new InvalidOperationException("WheelGameSettings requires a WheelOutcomePopupMotionCatalog asset.");
            }
        }

        private void RequireSpinResolveCatalog()
        {
            if (_spinResolveCatalog == null)
            {
                throw new InvalidOperationException("WheelGameSettings requires a WheelSpinResolveCatalog asset.");
            }
        }

        private static void CacheRewardText(List<RewardDefinition> rewards, string winLabelFormat)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                CacheRewardText(rewards[i], winLabelFormat);
            }
        }

        private static void CacheRewardText(RewardDefinition reward, string winLabelFormat)
        {
            reward.CacheRuntimeText(winLabelFormat);
        }
    }
}
