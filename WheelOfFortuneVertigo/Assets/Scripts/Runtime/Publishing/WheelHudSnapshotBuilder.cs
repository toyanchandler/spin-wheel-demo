using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Builds <see cref="WheelHudSnapshot"/> from live state + UI copy catalogs.</summary>
    public static class WheelHudSnapshotBuilder
    {
        public static Color ResolveHudBackgroundColor(WheelThemeSettings theme)
        {
            return theme.ResolveHudBackgroundColor(theme.BackgroundColor);
        }

        public static WheelHudSnapshot Create(WheelGameState state)
        {
            WheelGameSettings settings = state.Settings;
            WheelUiCopyCatalog catalog = settings.UiCopy;
            WheelThemeSettings theme = settings.Theme;

            return new WheelHudSnapshot(
                state.Zone,
                state.Phase,
                ResolveHudBackgroundColor(theme),
                WheelHudSnapshotPartsBuilder.BuildZoneLabels(state, catalog, theme),
                WheelHudSnapshotPartsBuilder.BuildMilestones(state, settings, catalog, theme),
                WheelHudSnapshotPartsBuilder.BuildStatus(state, catalog, theme),
                WheelHudSnapshotPartsBuilder.BuildActions(state, catalog),
                WheelHudSnapshotPartsBuilder.BuildExitConfirmation(catalog),
                WheelHudSnapshotPartsBuilder.BuildRewardCards(state, catalog, settings.Layout));
        }
    }
}
