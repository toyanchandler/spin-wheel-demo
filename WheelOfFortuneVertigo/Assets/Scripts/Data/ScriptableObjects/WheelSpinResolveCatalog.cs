using UnityEngine;

namespace Vertigo.Wheel.Data
{
    [CreateAssetMenu(fileName = "WheelSpinResolveCatalog", menuName = "Vertigo/Wheel Spin Resolve Catalog")]
    public sealed class WheelSpinResolveCatalog : ScriptableObject
    {
        [SerializeField] private WheelSpinResolveProfile[] _profilesByOutcome = CreateDefaultProfiles();

        public WheelSpinResolveProfile GetProfile(bool isBomb)
        {
            return _profilesByOutcome[System.Convert.ToInt32(isBomb)];
        }

        public void ResetToDefaults()
        {
            _profilesByOutcome = CreateDefaultProfiles();
        }

        private static WheelSpinResolveProfile[] CreateDefaultProfiles()
        {
            return new[]
            {
                new WheelSpinResolveProfile
                {
                    targetPhase = WheelGamePhase.Won,
                    advanceZone = true,
                    markSlicesDirty = true,
                    addResultToInventory = true
                },
                new WheelSpinResolveProfile
                {
                    targetPhase = WheelGamePhase.Bombed,
                    clearInventory = true
                }
            };
        }
    }
}
