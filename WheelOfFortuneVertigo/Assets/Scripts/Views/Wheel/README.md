# Wheel visuals

| File | Responsibility |
|------|----------------|
| `WheelView` | Slice pool from `WheelZoneSnapshot`; `SpinLanded` / `OutcomeResolved` highlights |
| `WheelSkinView` | Wheel base + indicator sprites from zone snapshot; indicator tick on spin |
| `WheelSliceVisualState` | Per-slice highlight/bomb styling |
| `WheelSlicePresentationResolver` | Pointer angles for spin; icon projection via `WheelSliceIconPresentation` |
| `WheelSliceArrayLookup` / `WheelSpinAngleReader` | Shared slice/spin read helpers |
| `WheelSliceImpactAnimator` | Bomb/reward hit scale pulses |

Gameplay never lives here — only snapshot + bus reactions. See `ARCHITECTURE_AND_LOGIC.md` §15–16.
