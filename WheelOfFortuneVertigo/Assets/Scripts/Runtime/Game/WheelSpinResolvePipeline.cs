using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Single place for post-spin gameplay rules (phase, zone, inventory, slices).
    /// Profiles come from <see cref="WheelSpinResolveCatalog"/> — not hard-coded in views.
    /// </summary>
    public static class WheelSpinResolvePipeline
    {
        /// <summary>
        /// Win path (default catalog): phase Won, zone++, add to inventory, regenerate slices.
        /// Bomb path: phase Bombed, clear inventory.
        /// </summary>
        public static void Apply(WheelGameState state, WheelSpinResult result)
        {
            state.RecordSpinResult(result);

            WheelSpinResolveProfile profile = state.Settings.SpinResolveCatalog.GetProfile(result.IsBomb);
            state.ApplyResolveProfile(result, profile);
        }
    }
}
