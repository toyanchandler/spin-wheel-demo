# Outcome popup

| File | Role |
|------|------|
| `WheelOutcomePopupView` | Bus lifecycle |
| `WheelOutcomePopupBindings` | `_wiring` struct (inspector) |
| `WheelOutcomePopupHandles` | Plain C# handles built from wiring |
| `WheelOutcomePopupRefs` | Bundle for presenter / animator |
| `WheelOutcomePopupPresenter` | Snapshot → motion |
| `WheelOutcomePopupRewardFlight` | Uses `Presentation.Loot` (not loot view type) |

Scene wiring: see `Views/SCENE_WIRING.md`. No per-child binding MonoBehaviours.
