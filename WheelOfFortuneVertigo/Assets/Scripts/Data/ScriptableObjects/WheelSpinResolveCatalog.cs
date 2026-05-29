using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelSpinResolveCatalog", menuName = "Vertigo/Wheel Spin Resolve Catalog")]
    public sealed class WheelSpinResolveCatalog : ScriptableObject
    {
        [SerializeField] private WheelSpinResolveProfile[] _profilesByOutcome = CreateDefaultProfiles();

        public WheelSpinResolveProfile GetProfile(bool isBomb) => _profilesByOutcome[isBomb ? 1 : 0];

        public void ResetToDefaults()
        {
            _profilesByOutcome = CreateDefaultProfiles();
        }

        private static WheelSpinResolveProfile[] CreateDefaultProfiles()
        {
            return new[]
            {
                WheelSpinResolveProfile.Create(
                    WheelGamePhase.Won,
                    advanceZone: true,
                    markSlicesDirty: true,
                    addResultToInventory: true),
                WheelSpinResolveProfile.Create(WheelGamePhase.Bombed, clearInventory: true)
            };
        }
    }
}
