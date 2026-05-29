# Loot panel

| File | Responsibility |
|------|----------------|
| `WheelRewardPanelView` | Scene wiring, `HudStateChanged`, API for outcome popup flight |
| `WheelRewardPanelHudCoordinator` | Defer vs immediate inventory commit |
| `WheelRewardPanelInventoryState` | Rendered/pending buffers |
| `WheelRewardPanelRenderer` | Card visuals |
| `WheelRewardPanelLayout` | Scroll + reserved slots |
| `WheelRewardPanelLandingResolver` | World position for flying icon |

Journey: see `ARCHITECTURE_AND_LOGIC.md` §14.
