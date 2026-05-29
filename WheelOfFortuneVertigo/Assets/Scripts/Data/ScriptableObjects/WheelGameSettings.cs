using System;
using System.Collections.Generic;
using DG.Tweening;
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

        private WheelZoneTypeCatalog _zoneCatalog;
        private WheelZoneIntervalRule[] _zoneIntervalRules;

        [Header("Zone Gameplay")]
        [SerializeField] private WheelZoneGameplayProfile[] _zoneGameplayProfiles = CreateDefaultZoneGameplayProfiles();

        [Header("Spin Motion")]
        [SerializeField, Min(0.5f)] private float _spinDuration = 3.4f;
        [SerializeField, Min(1)] private int _minimumSpinRounds = 5;
        [SerializeField] private Ease _spinEase = Ease.InOutQuart;

        [Header("Presentation")]
        [SerializeField] private WheelUiCopyCatalog _uiCopy;
        [SerializeField] private WheelSkinCatalog _skinCatalog;
        [SerializeField] private WheelOutcomePopupMotionCatalog _outcomePopupMotionCatalog;
        [SerializeField] private WheelSpinResolveCatalog _spinResolveCatalog;
        [SerializeField] private WheelLayoutSettings _layout = WheelLayoutSettings.Default();
        [SerializeField] private WheelThemeSettings _theme = WheelThemeSettings.Default();

        public WheelUiCopyCatalog UiCopy => _uiCopy;
        public WheelSkinCatalog SkinCatalog => _skinCatalog;
        public WheelOutcomePopupMotionCatalog OutcomePopupMotionCatalog => _outcomePopupMotionCatalog;
        public WheelSpinResolveCatalog SpinResolveCatalog => _spinResolveCatalog;
        public float SpinDuration => _spinDuration;
        public int MinimumSpinRounds => _minimumSpinRounds;
        public Ease SpinEase => _spinEase;
        public WheelLayoutSettings Layout => _layout;
        public WheelThemeSettings Theme => _theme;

        public WheelZoneGameplayProfile GetZoneGameplay(ZoneType zoneType) => RequireZoneCatalog().GetZoneGameplay(zoneType);

        public IReadOnlyList<RewardDefinition> GetRewardPool(ZoneType zoneType) => ResolveRewardPool(zoneType);

        public int SliceCount => Mathf.Max(4, _sliceCount);
        public int SafeZoneInterval => Mathf.Max(1, _safeZoneInterval);
        public int SuperZoneInterval => Mathf.Max(1, _superZoneInterval);
        public RewardDefinition BombReward => _bombReward;

        public void SetBombReward(RewardDefinition bombReward)
        {
            _bombReward = bombReward;
        }
        public void ReplaceRewardPool(ZoneType zoneType, RewardDefinition reward)
        {
            List<RewardDefinition> pool = ResolveMutableRewardPool(zoneType);
            pool.Clear();
            if (reward != null) pool.Add(reward);
        }

        public ZoneType GetZoneType(int zone) => WheelZoneTypeTable.Resolve(zone, _zoneIntervalRules, ZoneType.Standard);

#if UNITY_EDITOR
        public void ConfigureSkinCatalog(WheelSkinCatalog skinCatalog)
        {
            _skinCatalog = skinCatalog;
        }

        public void ConfigureOutcomePopupMotionCatalog(WheelOutcomePopupMotionCatalog outcomePopupMotionCatalog)
        {
            _outcomePopupMotionCatalog = outcomePopupMotionCatalog;
        }

        public void ConfigureSpinResolveCatalog(WheelSpinResolveCatalog spinResolveCatalog)
        {
            _spinResolveCatalog = spinResolveCatalog;
        }
#endif

        public void InitializeRuntime()
        {
            _zoneIntervalRules = CreateZoneIntervalRules();
            _zoneCatalog = WheelZoneTypeCatalog.FromSerializedProfiles(
                _zoneGameplayProfiles,
                _standardRewards,
                _safeRewards,
                _superRewards);
            ValidateRuntimeConfig();
            RequireUiCopy();
            RequireOutcomePopupMotionCatalog();
            RequireSpinResolveCatalog();
            string winLabelFormat = _uiCopy.WinLabelFormat;
            CacheRewardText(_bombReward, winLabelFormat);
            CacheRewardText(_standardRewards, winLabelFormat);
            CacheRewardText(_safeRewards, winLabelFormat);
            CacheRewardText(_superRewards, winLabelFormat);
        }

#if UNITY_EDITOR
        public void ConfigureUiCopy(WheelUiCopyCatalog uiCopy)
        {
            _uiCopy = uiCopy;
        }
#endif

        public int FillSlicesForZone(int zone, WheelSliceDefinition[] buffer) => WheelSliceGenerator.FillSlicesForZone(this, zone, buffer);

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
            if (rewards == null || rewards.Count == 0) throw new InvalidOperationException("WheelGameSettings requires at least one reward for " + zoneType + " zones.");
            for (int i = 0; i < rewards.Count; i++) RequireReward(rewards[i], zoneType);
        }

        private static void RequireReward(RewardDefinition reward, ZoneType zoneType)
        {
            if (reward == null) throw new InvalidOperationException("WheelGameSettings has a null reward in " + zoneType + " configuration.");
            if (string.IsNullOrEmpty(reward.Id)) throw new InvalidOperationException("WheelGameSettings reward in " + zoneType + " configuration requires a stable id.");
            if (string.IsNullOrEmpty(reward.DisplayName)) throw new InvalidOperationException("WheelGameSettings reward '" + reward.Id + "' in " + zoneType + " configuration requires a display name.");
            if (reward.Icon == null) throw new InvalidOperationException("WheelGameSettings reward '" + reward.Id + "' in " + zoneType + " configuration requires an icon.");
        }

        private void RequireUiCopy()
        {
            if (_uiCopy == null) throw new InvalidOperationException("WheelGameSettings requires a WheelUiCopyCatalog asset.");
        }

        private void RequireOutcomePopupMotionCatalog()
        {
            if (_outcomePopupMotionCatalog == null) throw new InvalidOperationException("WheelGameSettings requires a WheelOutcomePopupMotionCatalog asset.");
        }

        private void RequireSpinResolveCatalog()
        {
            if (_spinResolveCatalog == null) throw new InvalidOperationException("WheelGameSettings requires a WheelSpinResolveCatalog asset.");
        }

        private static void CacheRewardText(List<RewardDefinition> rewards, string winLabelFormat)
        {
            for (int i = 0; i < rewards.Count; i++) CacheRewardText(rewards[i], winLabelFormat);
        }

        private static void CacheRewardText(RewardDefinition reward, string winLabelFormat)
        {
            reward.CacheRuntimeText(winLabelFormat);
        }

        private IReadOnlyList<RewardDefinition> ResolveRewardPool(ZoneType zoneType)
        {
            return RequireZoneCatalog().GetRewardPool(zoneType);
        }

        private List<RewardDefinition> ResolveMutableRewardPool(ZoneType zoneType)
        {
            return RequireZoneCatalog().GetMutableRewardPool(zoneType);
        }

        private WheelZoneTypeCatalog RequireZoneCatalog()
        {
            if (_zoneCatalog == null)
            {
                throw new InvalidOperationException("WheelGameSettings requires InitializeRuntime before zone catalog access.");
            }

            return _zoneCatalog;
        }
    }
}
