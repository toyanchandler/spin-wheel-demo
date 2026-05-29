# Architecture & Game Logic

How the Vertigo Wheel project is structured, who owns what, and how player actions move through the system.

For file naming and folder shortcuts see [`CODE_READABILITY.md`](CODE_READABILITY.md).

---

## 1. Architectural layers (dependency direction)

Data always flows **down** this stack. Lower layers never call Views.

```
┌─────────────────────────────────────────────────────────────┐
│  Views (MonoBehaviour)     Vertigo.Wheel.Views              │
│  Apply snapshots, play tweens, read bus events only         │
└───────────────────────────────┬─────────────────────────────┘
                                │ subscribes
┌───────────────────────────────▼─────────────────────────────┐
│  Presentation events         WheelEventBus (snapshot side)  │
│  ZoneChanged, HudStateChanged, OutcomeResolved, SpinLanded │
└───────────────────────────────┬─────────────────────────────┘
                                │ built by
┌───────────────────────────────▼─────────────────────────────┐
│  Publishing                  WheelStatePublisher            │
│  WheelSnapshotFactory → *SnapshotBuilder                  │
└───────────────────────────────┬─────────────────────────────┘
                                │ reads
┌───────────────────────────────▼─────────────────────────────┐
│  Gameplay orchestration      WheelGameFlowController        │
│  WheelSpinResolvePipeline    IWheelSpinDriver               │
└───────────────────────────────┬─────────────────────────────┘
                                │ mutates
┌───────────────────────────────▼─────────────────────────────┐
│  Session state (mutable)     WheelGameState, RewardInventory │
└───────────────────────────────┬─────────────────────────────┘
                                │ configured by
┌───────────────────────────────▼─────────────────────────────┐
│  Designer data (immutable)   Assets/Config/*.asset          │
│  WheelGameSettings + catalogs + Rules/*                     │
└─────────────────────────────────────────────────────────────┘
```

### Layer rules

| Layer | May do | Must not do |
|-------|--------|-------------|
| **Config / Rules** | Pure logic, defaults, validation | Reference `MonoBehaviour`, bus, or views |
| **WheelGameState** | Zone, phase, slices, inventory | Publish UI events, run tweens |
| **Flow + Spinner** | Orchestrate spin/leave/restart | Format HUD strings, read TMP |
| **Publisher + Builders** | Project state → snapshots | Change zone or inventory |
| **Views** | Render snapshots, fire **intents** | Read `WheelGameSettings`, change zone |

---

## 2. Two channels on the event bus

`WheelEventBus` is intentionally split into two directions. Mixing them is the main source of “spaghetti” confusion.

### Capability interfaces (compile-time direction)

The concrete `WheelEventBus` implements narrow read/write interfaces so views and gameplay only receive what they need:

| Interface | Direction | Typical consumer |
|-----------|-----------|------------------|
| `IWheelUiIntentPublisher` | Views raise intents | Button actions, exit confirmation |
| `IWheelUiIntentSubscriber` | Gameplay listens | `WheelGameFlowController`, exit confirmation |
| `IWheelSnapshotPublisher` | Gameplay raises snapshots | `WheelStatePublisher`, `WheelGameFlowController` (SpinLanded) |
| `IWheelSnapshotSubscriber` | Views listen | HUD views, wheel views, loot panel |

Views that need `Presentation` channels (loot flight, spin layout) still inject `WheelEventBus` for `eventBus.Presentation` only.

### Deterministic randomness

Gameplay randomness uses `IRandomSource` (`UnityRandomSource` in production, `SeededRandomSource` in tests), injected when `WheelGameState` and `WheelSpinner` initialize/bind. Do not call `UnityEngine.Random` directly in spin outcome or reward-slice selection paths.

### UI intents (Views → gameplay)

| Intent | Raised by | Handled by |
|--------|-----------|------------|
| `RequestSpin` | Spin button | `WheelGameFlowController` → `ExecuteSpin` |
| `RequestLeaveConfirmation` | Leave button | `WheelExitConfirmationView` shows overlay |
| `RequestLeave` | Confirm collect | `WheelGameFlowController` → `ExecuteLeave` (cash out) |
| `RequestRestart` | Restart button | `WheelGameFlowController` → `ExecuteRestart` |

**Only** `WheelGameFlowController` (and debug tools via `Flow.ForceResolveOutcome`) should change phase/inventory/zone in response to intents.

### Snapshot events (gameplay → Views)

| Event | Payload | Typical subscribers |
|-------|---------|-------------------|
| `ZoneChanged` | `WheelZoneSnapshot` | `WheelView`, `WheelSkinView` |
| `HudStateChanged` | `WheelHudSnapshot` | HUD texts, buttons, loot panel, zone progress |
| `SpinLanded` | `WheelSpinResult` | `WheelView` (slice highlight before resolve) |
| `OutcomeResolved` | `WheelOutcomeSnapshot` | Outcome popup, bomb highlight on wheel |

**Only** `WheelStatePublisher` (or flow calling publisher after resolve) should raise snapshot events.

### Presentation registry (view ↔ view, same session)

Third channel on `WheelEventBus.Presentation` — **not** gameplay. Views **register** handlers in `Connect`; other views **call the channel**, never sibling types.

| Channel | Provider registers | Consumer uses |
|---------|-------------------|---------------|
| `Presentation.Loot` | `WheelRewardPanelView` → `IWheelLootFlightHandler` | Outcome popup flight |
| `Presentation.Spin` | Bootstrap registers `IWheelSpinDriver`; `WheelView` registers `IWheelSliceLayoutPresenter` | `WheelSpinner`, `WheelSkinView` |

No `GetComponentInChildren`, no `[WheelInject]` of another view type. Same-object `Bindings` use `[SerializeField]` or `GetComponent` on self only.

---

## 3. Session lifecycle (play mode boot)

Order is fixed. Views bind when step 4 completes.

```
1. WheelRuntimeCompositionRoot.BeginRuntime()
      LoadGameplayComponentsFromScene()     // state, publisher, flow on same GO
      InitializeGameStateFromSettings()     // WheelGameSettings.asset
      BindGameplaySystems()                 // publisher + flow + spinner
      RegisterSessionAndNotifyViews()       // WheelRuntimeLocator + RuntimeReady
      StartNewRun()                         // Restart() + PublishAll()

2. Each WheelViewScope hears RuntimeReady
      → inject [WheelBind] views
      → [WheelAfterInject] subscribe to bus

3. Player interacts; flow mutates state; publisher pushes snapshots
```

Shutdown reverses bindings: spinner/flow/publisher unbind → `Locator.Clear()` → `EventBus.Clear()`.

---

## 4. Phase model (game logic core)

`WheelGamePhase` is the single authority for “what kind of moment” the player is in.

| Phase | Meaning | Typical CanSpin | Typical CanLeave | Typical CanRestart |
|-------|---------|-----------------|------------------|-------------------|
| `Ready` | Waiting for spin | yes | no* | no |
| `Spinning` | Wheel animating | no | no | no |
| `Won` | Last spin was reward | no | yes | no |
| `Bombed` | Last spin hit bomb | no | no | yes |
| `CashedOut` | Player collected & left | no | no | yes |

\*Leave can be allowed earlier if zone profile allows or inventory has items — see `WheelGameState.CanLeave`.

Phase transitions are **not** scattered in views. They come from:

1. `WheelGameState.BeginSpin()` → `Spinning`
2. `WheelSpinResolvePipeline` after spin → `Won` or `Bombed` (from catalog profile)
3. `WheelGameState.CashOut()` → `CashedOut`
4. `WheelGameState.Restart()` → `Ready`

---

## 5. Post-spin resolve pipeline (win vs bomb)

When the wheel animation finishes, gameplay applies rules in one place:

**`WheelSpinResolvePipeline.Apply(state, result)`**

```
1. RecordSpinResult(result)          // last result for HUD/outcome text
2. profile = SpinResolveCatalog      // index 0 = win, 1 = bomb
3. ApplyResolveProfile(profile):
      - set Phase (Won / Bombed)
      - optional ClearInventory (bomb)
      - optional AddResultToInventory (win)
      - optional AdvanceZone (win)
      - optional MarkSlicesDirty + refill slices (win)
```

Default catalog (see `WheelSpinResolveCatalog`):

| Outcome | New phase | Zone | Inventory | Slices |
|---------|-----------|------|-----------|--------|
| Reward | `Won` | +1 | add spin result | regenerate for new zone |
| Bomb | `Bombed` | — | cleared | unchanged |

Designer tuning: edit profiles on `WheelSpinResolveCatalog.asset`, not C# in views.

---

## 6. Player journey: one full spin

Step-by-step with owning types.

| Step | What happens | Owner |
|------|----------------|-------|
| 1 | Player taps Spin | `WheelSpinButtonAction` → `RequestSpin` |
| 2 | Flow checks `CanSpin`, not `IsSpinning` | `WheelGameFlowController` |
| 3 | `PrepareCurrentZone`, `PublishZone` | state + publisher |
| 4 | Pick random slice index | `WheelSpinOutcomeSelector` |
| 5 | `BeginSpin`, `PublishHud` (spinning UI) | state + publisher |
| 6 | Copy slices to spinner, start tween | `WheelSpinner` |
| 7 | Landing phase fires | `LandingStarted` → flow raises `SpinLanded` |
| 8 | Wheel highlights landed slice | `WheelView.OnSpinLanded` |
| 9 | Animation completes | `SpinCompleted` |
| 10 | `WheelSpinResolvePipeline.Apply` | mutates phase/zone/inventory |
| 11 | `PublishOutcome` + `PublishHud` or `PublishAll` | publisher (per phase profile) |
| 12 | Popup + HUD + loot panel refresh | views on snapshot events |

**Important:** UI reacts twice to a spin — once on `SpinLanded` (visual only), once on snapshots after resolve (rules outcome).

---

## 7. Player journey: leave (cash out)

| Step | Owner |
|------|--------|
| Leave tap → `RequestLeaveConfirmation` | button |
| Overlay shows copy from `HudSnapshot.ExitConfirmation` | `WheelExitConfirmationView` |
| Collect → `RequestLeave` | overlay |
| `CashOut()` → phase `CashedOut` | `WheelGameState` |
| `PublishHud` + empty `PublishOutcome` | `WheelGameFlowController.ExecuteLeave` |
| Card reveal / restart UI | views on `HudStateChanged` |

---

## 8. Player journey: restart

| Step | Owner |
|------|--------|
| Restart tap → `RequestRestart` | button |
| `Restart()` zone=1, phase=Ready, clear inventory | `WheelGameState` |
| `PublishAll` | publisher |
| All HUD + wheel views reset from snapshots | views |

---

## 9. When to call which publish method

| Method | Refreshes | Use when |
|--------|-----------|----------|
| `PublishZone` | Wheel slices, skin tier | Zone changed, start of spin |
| `PublishHud` | Buttons, texts, loot, milestones | Phase/permissions/copy changed |
| `PublishOutcome` | Result popup | After spin resolve or cash out |
| `PublishAll` | Zone + HUD | Restart, debug fill, phases that set `PublishAllAfterSpin` |

`WheelGameFlowController` is the reference for publish order during spins. After resolve:

- If `PhaseGameplay.PublishAllAfterSpin` → `PublishAll` (win path refills zone UI)
- Else → `PublishHud` only (bomb path often keeps zone snapshot)

---

## 10. Config → snapshot → view (data pipeline)

Views never read `WheelGameSettings` directly.

```
WheelGameSettings.asset
    ├── Theme, Layout, UiCopy, SpinResolve, Motion catalogs
    └── FillSlicesForZone / GetZoneType (Rules)

WheelGameState (runtime numbers: zone, phase, inventory, slice buffer)

*SnapshotBuilder (read state + settings once per publish)
    ├── WheelHudSnapshotBuilder      → buttons, labels, reward cards
    ├── WheelZoneSnapshotBuilder     → slice copies for wheel
    └── WheelOutcomeSnapshotBuilder → popup copy/icon

View handler (apply snapshot fields only)
```

Copy formatting example: `string.Format(catalog.SafeMilestoneBadgeFormat, nextSafeZone)` lives in **Hud builder**, not in `WheelMilestoneBadgesView`.

---

## 11. View DI vs gameplay (why two mechanisms)

| Mechanism | Purpose |
|-----------|---------|
| `WheelViewScope` + `[WheelInject]` | Find sibling components under same canvas |
| `WheelEventBus` | Cross-component **events** without serialized refs |

Outcome popup uses `eventBus.Presentation.Loot` for flight landing — still no gameplay logic in popup view.

---

## 12. Adding a feature checklist

1. **Gameplay rule?** → state, catalog, or `Data/Rules`
2. **New UI text/color?** → catalog + snapshot builder field
3. **New UI widget?** → view + bindings; subscribe in `AfterInject`
4. **New player action?** → bus intent + flow handler
5. **Publish?** → publisher method; pick zone/hud/outcome/all deliberately

If you need a new global singleton or view reading settings — stop and re-read section 1.

---

## 13. Related code entry points

| Question | Start here |
|----------|------------|
| Boot order | `WheelRuntimeCompositionRoot.BeginRuntime` |
| Spin logic | `WheelGameFlowController.ExecuteSpin` |
| Win/bomb rules | `WheelSpinResolvePipeline` |
| UI refresh | `WheelStatePublisher` |
| Button → game | `WheelButtonAction` subclasses |
| Slice generation | `WheelSliceGenerator.FillSlicesForZone` |
| Typed session API | `WheelGameplaySession.TryGet` |
| Editor play-mode tools | `VertigoWheelDesignerPlayModeCommands` |

---

## 14. Loot panel: defer vs commit (win flight)

When the player wins, HUD inventory updates may be **deferred** until the outcome popup icon flies to the loot strip.

```
HudStateChanged (rewards increased, defer flag set)
    → WheelRewardPanelHudCoordinator.ApplyDeferredGain
        → inventory.StorePending
        → render current cards only
        → reserve landing slot (layout + hide placeholders)
        → schedule fallback commit (motion catalog delay)

Outcome popup flight starts
    → Presentation.Loot.HoldForArrival()  // loot panel handler; cancels fallback timer

Icon lands on strip
    → Presentation.Loot.CommitPendingNow()
        → render merged inventory + pulse new card
```

| Type | Role |
|------|------|
| `WheelRewardPanelView` | Bus wiring, scene refs, public API for popup flight |
| `WheelRewardPanelHudCoordinator` | Defer/immediate/fallback commit policy |
| `WheelRewardPanelInventoryState` | Rendered vs pending card buffers |
| `WheelRewardPanelRenderer` / `Layout` | Visuals and scroll |

Views never read `WheelGameSettings`; frame sprite and titles come from `WheelHudSnapshot.Rewards`.

---

## 15. Zone skin sprites in snapshots

`WheelZoneSnapshotBuilder` resolves `WheelBaseSprite` and `IndicatorSprite` from `WheelSkinCatalog` when publishing.

`WheelSkinView` only assigns `Image.sprite` from the snapshot and runs indicator tick motion during spin — **no catalog field on the view**.

---

## 16. Wheel slice feedback (land → resolve)

```
SpinLanded(result)     → WheelView highlights landing slice
OutcomeResolved(snap)  → bomb/reward pulse via WheelSliceImpactAnimator
```

Slice motion constants live in `WheelSliceMotion`; animator is a static helper to keep `WheelView` readable.

---

## 17. Card reveal (cash out)

```
RequestLeave / cash out
    → state.CashOut()
    → PublishHud + PublishOutcome(empty result)
    → WheelCardRevealView reads OutcomeResolved / Hud rewards snapshot
```

Debug: designer **Open 15 Cards** uses `VertigoWheelDesignerPlayModeCommands` (shared `WheelEditorDebugRewards` filler).

---

## 18. Quality bar (target 10/10)

| Area | Expectation |
|------|-------------|
| **Architecture** | One direction: config → state → publisher → views; bus intents only into flow |
| **Readability** | Views thin; policy in coordinators/builders; journey documented here |
| **Session access** | Runtime: `WheelRuntimeLocator` / `WheelGameplaySession`; editor: `WheelRuntimeEditorSession` |
| **Tests** | Edit Mode: snapshots, flow, resolve, locator, session; Play Mode smoke optional |
