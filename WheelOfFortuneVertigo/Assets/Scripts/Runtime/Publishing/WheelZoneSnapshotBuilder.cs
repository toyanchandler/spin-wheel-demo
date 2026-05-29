using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Builds <see cref="WheelZoneSnapshot"/> including slice copies and resolved skin sprites.</summary>
    public static class WheelZoneSnapshotBuilder
    {
        public static WheelZoneSnapshot Create(WheelGameState state)
        {
            WheelGameSettings settings = state.Settings;
            WheelZoneUiCopy zoneCopy = settings.UiCopy.GetZoneCopy(state.ZoneType);
            WheelSkinCatalog skinCatalog = settings.SkinCatalog;
            WheelSkinTier skinTier = zoneCopy.SkinTier;

            WheelSliceDefinition[] slices = state.CreateSliceSnapshot();
            int sliceCount = state.SliceCount;
            return new WheelZoneSnapshot(
                WheelSlicePresentationBuilder.Build(slices, sliceCount),
                sliceCount,
                skinTier,
                skinCatalog.GetWheelBase(skinTier),
                skinCatalog.GetIndicator(skinTier),
                settings.Theme.BackgroundColor);
        }
    }
}
