using System;
using System.Collections.Generic;

namespace Vertigo.Wheel.Data
{
    /// <summary>
    /// Maps <see cref="ZoneType"/> to gameplay profiles and reward pools without relying on enum ordinal order.
    /// </summary>
    public sealed class WheelZoneTypeCatalog
    {
        private readonly WheelZoneGameplayProfile[] _gameplayByZone;
        private readonly List<RewardDefinition>[] _rewardPoolsByZone;

        private WheelZoneTypeCatalog(
            WheelZoneGameplayProfile[] gameplayByZone,
            List<RewardDefinition>[] rewardPoolsByZone)
        {
            _gameplayByZone = gameplayByZone;
            _rewardPoolsByZone = rewardPoolsByZone;
        }

        public static WheelZoneTypeCatalog Create(
            WheelZoneGameplayProfile standardGameplay,
            WheelZoneGameplayProfile safeGameplay,
            WheelZoneGameplayProfile superGameplay,
            List<RewardDefinition> standardRewards,
            List<RewardDefinition> safeRewards,
            List<RewardDefinition> superRewards)
        {
            int zoneTypeCount = Enum.GetValues(typeof(ZoneType)).Length;
            var gameplayByZone = new WheelZoneGameplayProfile[zoneTypeCount];
            var rewardPoolsByZone = new List<RewardDefinition>[zoneTypeCount];

            AssignZone(ZoneType.Standard, standardGameplay, standardRewards, gameplayByZone, rewardPoolsByZone);
            AssignZone(ZoneType.Safe, safeGameplay, safeRewards, gameplayByZone, rewardPoolsByZone);
            AssignZone(ZoneType.Super, superGameplay, superRewards, gameplayByZone, rewardPoolsByZone);

            return new WheelZoneTypeCatalog(gameplayByZone, rewardPoolsByZone);
        }

        public static WheelZoneTypeCatalog FromSerializedProfiles(
            WheelZoneGameplayProfile[] zoneGameplayProfiles,
            List<RewardDefinition> standardRewards,
            List<RewardDefinition> safeRewards,
            List<RewardDefinition> superRewards)
        {
            if (zoneGameplayProfiles == null || zoneGameplayProfiles.Length < 3)
            {
                throw new InvalidOperationException(
                    "WheelGameSettings requires zone gameplay profiles for Standard, Safe, and Super zones.");
            }

            return Create(
                zoneGameplayProfiles[0],
                zoneGameplayProfiles[1],
                zoneGameplayProfiles[2],
                standardRewards,
                safeRewards,
                superRewards);
        }

        public WheelZoneGameplayProfile GetZoneGameplay(ZoneType zoneType)
        {
            int index = ToZoneIndex(zoneType);
            return _gameplayByZone[index];
        }

        public IReadOnlyList<RewardDefinition> GetRewardPool(ZoneType zoneType)
        {
            int index = ToZoneIndex(zoneType);
            List<RewardDefinition> pool = _rewardPoolsByZone[index];
            if (pool == null)
            {
                throw new InvalidOperationException("WheelGameSettings has no reward pool for " + zoneType + ".");
            }

            return pool;
        }

        public List<RewardDefinition> GetMutableRewardPool(ZoneType zoneType)
        {
            int index = ToZoneIndex(zoneType);
            List<RewardDefinition> pool = _rewardPoolsByZone[index];
            if (pool == null)
            {
                throw new InvalidOperationException("WheelGameSettings has no reward pool for " + zoneType + ".");
            }

            return pool;
        }

        private static void AssignZone(
            ZoneType zoneType,
            WheelZoneGameplayProfile gameplay,
            List<RewardDefinition> rewards,
            WheelZoneGameplayProfile[] gameplayByZone,
            List<RewardDefinition>[] rewardPoolsByZone)
        {
            int index = ToZoneIndex(zoneType);
            gameplayByZone[index] = gameplay;
            rewardPoolsByZone[index] = rewards;
        }

        private static int ToZoneIndex(ZoneType zoneType)
        {
            if (!Enum.IsDefined(typeof(ZoneType), zoneType))
            {
                throw new ArgumentOutOfRangeException(nameof(zoneType), zoneType, "Unsupported zone type.");
            }

            return (int)zoneType;
        }
    }
}
