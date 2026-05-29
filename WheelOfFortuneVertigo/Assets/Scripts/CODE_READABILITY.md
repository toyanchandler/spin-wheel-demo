# Code Readability — Quick Index

**Start here for architecture and game logic:** [`ARCHITECTURE_AND_LOGIC.md`](ARCHITECTURE_AND_LOGIC.md)

That document explains layers, phase model, spin/leave/restart journeys, publish rules, and config → snapshot → view pipeline.

This file is a **short index** for day-to-day navigation.

## Entry points by question

| Question | Read first |
|----------|------------|
| How does a spin work end-to-end? | `ARCHITECTURE_AND_LOGIC.md` §6 |
| Win vs bomb rules | `WheelSpinResolvePipeline.cs` + §5 |
| What can call what? | `ARCHITECTURE_AND_LOGIC.md` §1 |
| UI vs gameplay events | `WheelEventBus.UiIntents.cs` vs `WheelEventBus.SnapshotEvents.cs` |
| Boot order | `WheelRuntimeCompositionRoot.BeginRuntime` + §3 |
| When to PublishAll/Hud/Zone | `WheelStatePublisher` header + §9 |
| Folder / file naming | Below |

## Method size (unit-based)

Prefer **many small methods** over one long procedure:

- One job per method (~5–20 lines); orchestrators only call steps.
- Pure logic → static helper or `*Builder` / `*Resolver` / `*Simulator` classes you can hit with Edit Mode tests.
- MonoBehaviours → wire events and delegate; no 40-line `Update` / `OnGUI` blocks in gameplay code.

Examples: `WheelHudSnapshotPartsBuilder`, `WheelIndicatorSpinTickSimulator`, `WheelRewardCardSnapshotDiff`.

Prefer a small **result struct** (`WheelIndicatorSpinTickResult`, `WheelSliceCrossingResult`, `WheelSliceIconPresentation`, `WheelSlicePointerAnglesCopy`, `WheelRewardPanelCommitResult`) over `bool TryX(..., out Y)` when the caller needs more than success/fail. Each step should be one named unit (`Seed`, `TrackAngle`, `ResolveBoundaryTickOffset`, `ResolveIconSlice`, `CanCopyPointerAngles`).

Shared view helpers: `WheelSliceArrayLookup`, `WheelUiRectProjection`, `WheelSpinAngleReader`, `WheelWiringValidation`.

## Naming patterns

| Suffix | Meaning |
|--------|---------|
| `*View` | Bus lifecycle + apply snapshots |
| `*Bindings` | Scene refs + `Validate()` |
| `*Binding` | One UI widget group |
| `*Presenter` / `*Animator` | Non-MonoBehaviour presentation logic |
| `*Builder` | One snapshot type from state |
| `*Pipeline` | One gameplay rules path (e.g. post-spin resolve) |

## Panel / wheel file maps

| Area | README |
|------|--------|
| Reward popup | `Views/Panels/RewardPopup/README.md` |
| Loot panel | `Views/Panels/LootPanel/README.md` — §14 defer/commit |
| Wheel visuals | `Views/Wheel/README.md` — §15–16 |

Session API: `WheelGameplaySession` (runtime), `WheelRuntimeEditorSession` (EditorTools).

Cross-view wiring: `Runtime/Presentation/README.md` — bus `Presentation.Loot` / `Presentation.Spin`, not view-to-view inject.

Scene UI refs: `Views/SCENE_WIRING.md` — one `_wiring` struct per panel, not 12 child `*Binding` MonoBehaviours.

## Related

- [`SoftwareArchitectureRules.md`](SoftwareArchitectureRules.md) — rules constitution
- [`Runtime/DI/README.md`](Runtime/DI/README.md) — view injection
