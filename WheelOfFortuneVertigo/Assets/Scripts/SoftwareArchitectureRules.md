# Software Architecture Rules

**Vertigo Wheel Case Study — project constitution**

This document is the single source of truth for how we structure code, scene hierarchy, data, events, and editor tooling. If a change conflicts with a rule here, the change is wrong unless this document is updated first.

Related deep dives:

- `HIERARCHY_WIRING.md` — scene serialization detail
- `Data/DATA_DEFINITIONS.md` — DTO encapsulation detail
- `Collections/README.md` — `[CollectChildren]` tooling detail
- `README.md` — folder map

---

## 1. Principles (non‑negotiable)

1. **Data drives presentation** — gameplay and copy live in ScriptableObjects + rules; UI only renders **snapshots**.
2. **Thin views** — `MonoBehaviour` views do not contain game rules, string formatting policy, or cross-canvas wiring.
3. **Editor collects, runtime reads** — large child pools are filled in the editor; play mode never searches the hierarchy.
4. **Encapsulation everywhere** — no public mutable fields on config DTOs; SO roots expose get-only APIs.
5. **Explicit boundaries** — cross-subtree communication uses locator, event bus, or shared SOs only.
6. **One composition root** — `game_wheel_runtime` bootstraps session; no scattered `FindObjectOfType` gameplay graphs.
7. **Fail loud** — missing wiring throws `InvalidOperationException` with a clear message.

---

## 2. Layer map

```
Assets/Config/          → ScriptableObject assets (designer data)
Assets/Scripts/Data/    → SO classes, Definitions, Rules (no .asset files here)
Assets/Scripts/Runtime/ → Play mode gameplay + bootstrap + publishing
Assets/Scripts/Views/   → Scene UI MonoBehaviours (render snapshots, raise intents)
Assets/Scripts/Editor/  → Menus, inspectors, asset tools (never referenced at runtime)
Assets/Scripts/Collections/ → [CollectChildren] attribute (runtime) + editor collectors
```

| Namespace | Responsibility |
|-----------|----------------|
| `Vertigo.Wheel.Data` | Config, definitions, generators, static rules |
| `Vertigo.Wheel.Runtime` | State, flow, bus, locator, publisher, spinner |
| `Vertigo.Wheel.Views` | HUD / wheel / outcome views and canvas hosts |
| `Vertigo.Wheel.EditorTools` | Scene rebuild, validation, asset pipeline |
| `Vertigo.Collections` | Child-collection attribute (metadata only at runtime) |
| `Vertigo.Collections.Editor` | Collect / reset / preprocess / property drawer |

---

## 3. Config and encapsulation

### 3.1 Two places, two roles

| Location | What it is |
|----------|------------|
| `Assets/Config/*.asset` | Designer-owned Unity assets |
| `Assets/Scripts/Data/` | C# types that describe those assets |

Never treat `Scripts/Data` as the asset folder.

### 3.2 ScriptableObject roots (`WheelGameSettings`, catalogs)

```csharp
[SerializeField] private int _sliceCount = 8;
public int SliceCount => Mathf.Max(4, _sliceCount);
```

- Private `[SerializeField]` backing fields.
- Public **get-only** surface; validation/clamping in getters when needed.
- Lists and nested DTOs are not mutated by random views at runtime.

### 3.3 Definitions (nested DTOs)

`WheelThemeSettings`, `WheelLayoutSettings`, `RewardDefinition`, `WheelZoneUiCopy`, structs in `WheelUiCopyEntries.cs`, etc.

```csharp
[SerializeField] private Color _safeZoneColor;
public Color SafeZoneColor => _safeZoneColor;
```

Rules:

- **No `public` fields** on definitions.
- **No** `set` on properties exposed to gameplay.
- Runtime mutation uses **named methods** (e.g. `WheelSliceDefinition.ApplySlot(...)`).
- Editor/bootstrap mutation uses **factories** (`RewardDefinition.Create(...)`, `WheelZoneUiCopy.Create(...)`) or narrow SO APIs (`SetBombReward`, `BindFrameSprites`).

### 3.4 Defaults vs tuning

| Mechanism | Purpose |
|-----------|---------|
| `static Default()` / `Create(...)` on DTOs | First-time asset creation, reset buttons |
| `.asset` in `Assets/Config/` | Designer tuning (source of truth at runtime) |
| `WheelSnapshotFactory` | Read-only projection for UI |

Code defaults are **bootstrap**, not hidden runtime magic. Example: `WheelOutcomePopupMotion.Default()` seeds `WheelOutcomePopupMotionCatalog.asset`; play mode uses catalog → snapshot → view.

---

## 4. Scene hierarchy and SerializeField

### 4.1 Allowed references

| From | May reference |
|------|----------------|
| View on GO `X` | Components on `X` or **descendants** of `X` |
| Any runtime/UI script | ScriptableObjects under `Assets/Config/` |
| `game_wheel_runtime` | `WheelGameConfigSource` → `WheelGameSettings.asset` only (same GO) |

### 4.2 Forbidden references

- Dragging HUD widgets into wheel canvas components (or vice versa).
- Serializing “the other branch” slice pool, loot strip, or popup on a distant view.
- `Runtime Components` mega-lists on `game_wheel_runtime`.
- Runtime `Transform.Find` / deep child search for production UI pools.

### 4.3 Canvas hosts (exception, controlled)

`WheelHudUiHost`, `WheelWheelUiHost`, `WheelStaticUiHost` resolve **one** child view per role by **stable GameObject name** in `Awake` (e.g. `ui_zone_milestone_badges_root`). They do **not** serialize cross-branch reference lists.

Hosts only:

1. Subscribe to `WheelRuntimeLocator.RuntimeReady` / `RuntimeStopped`.
2. Call `Bind` / `Unbind` on known child views.

### 4.4 Composition root

`game_wheel_runtime`:

- `WheelRuntimeCompositionRoot` — **zero** serialized fields; uses `GetComponent` for gameplay on same GO.
- `WheelGameConfigSource` — single SO ref to `WheelGameSettings.asset`.
- `WheelGameState`, `WheelStatePublisher`, `WheelGameFlowController` on same GO.

No legacy `game_wheel_settings` scene object.

---

## 5. Child binding — `[CollectChildren]`

### 5.1 Philosophy

We do **not** add extra `*Collection` MonoBehaviours. The view (or panel) owns a serialized array; the editor fills it.

Runtime footprint: **`CollectChildrenAttribute`** only (`PropertyAttribute`, no logic).

### 5.2 Standard pattern

```csharp
[SerializeField] private Transform _slicePoolRoot;

[CollectChildren(nameof(_slicePoolRoot))]
[SerializeField] private WheelSliceView[] _sliceViews = Array.Empty<WheelSliceView>();
```

| Inspector control | Behavior |
|-------------------|----------|
| **Collect Children** | Walk pool root in **sibling order**, assign components |
| **Reset Children** | Clear array; mark scene dirty |
| **Collect On Preprocess Build** | Default on; refresh before player build |
| **(i) How child collection works** | Collapsible English help |

### 5.3 Ordering contract

- Index = `GetChild(0..n-1)` order under pool root (or nested `collectionRoot` transform).
- **GameObject names are not parsed** for matching.
- Each child must have the expected component on the visited transform.

### 5.4 Nested pool root

When pool lives under a child transform (e.g. opening overlay cards row):

```csharp
[SerializeField] private Transform _cardPoolRoot;
[CollectChildren(nameof(_cardPoolRoot))]
[SerializeField] private WheelRewardCardView[] _rewardCards;
```

Editor tooling sets `_cardPoolRoot` then calls `WheelEditorWiring.CollectChildren(...)`.

### 5.5 Custom bindings (zone progress)

When one child maps to **multiple** references (Image + TMP per cell), use a **view-specific Editor** (`WheelZoneProgressViewEditor`) — still no extra runtime collector component.

### 5.6 After every collect

`ChildCollectionEditorUtility.FinalizeCollect`:

- `EditorUtility.SetDirty(host)`
- `EditorSceneManager.MarkSceneDirty(scene)`

Scene rebuild must also mark scene dirty before save.

---

## 6. Runtime session — locator and bootstrap

### 6.1 `WheelRuntimeLocator` (static session)

Holds for one play session:

- `EventBus`, `Settings`, `State`, `Publisher`, `Spinner`
- `RuntimeReady` / `RuntimeStopped` signals for UI hosts

Registration only from composition root / spinner `Awake`. Cleared on stop.

### 6.2 Lifecycle

```
CompositionRoot.Start
  → Register settings + gameplay
  → State.InitializeRuntime()
  → Publisher.Bind(bus), Flow.Bind(bus), Spinner.Bind(bus)
  → NotifyRuntimeReady()  → UI hosts bind child views
  → State.Restart()
  → Publisher.PublishAll()

CompositionRoot.OnDisable / quit
  → Unbind all
  → Locator.Clear(), EventBus.Clear()
```

### 6.3 No service locator abuse

Locator is **session-scoped**, not a global grab-bag for new features. New systems extend bus + publisher + state, not `Locator.GetRandomThing()`.

---

## 7. Events and snapshots

### 7.1 `WheelEventBus`

| Direction | Events |
|-----------|--------|
| UI → gameplay | `SpinRequested`, `LeaveRequested`, `RestartRequested` |
| Gameplay → UI | `ZoneChanged`, `HudStateChanged`, `OutcomeResolved` |

Views **subscribe in `Bind`**, **unsubscribe in `Unbind`**. No orphaned delegates.

### 7.2 Snapshot types (readonly structs)

- `WheelHudSnapshot` — zone label, phase, buttons flags, loot sprites, milestone text/colors, reward cards, etc.
- `WheelZoneSnapshot` — slices, positions, skin tier, background
- `WheelOutcomeSnapshot` — popup copy, colors, motion, icon

Built only in `WheelSnapshotFactory` from `WheelGameState` + `WheelGameSettings` + catalogs.

### 7.3 View contract

```csharp
public void Bind(WheelEventBus eventBus)
{
    _eventBus = eventBus;
    _eventBus.HudStateChanged += OnHudStateChanged;
}

private void OnHudStateChanged(WheelHudSnapshot snapshot)
{
    _titleText.SetText(snapshot.SuperMilestoneBadgeText);
    _badgeImage.color = snapshot.SuperMilestoneBadgeColor;
}
```

Forbidden in views:

- Reading `WheelGameSettings` for presentation strings/colors
- Formatting `"SAFE\nZONE\n{0}"` locally
- Hard-coded theme colors “just for this label”

### 7.4 Publishing

Only `WheelStatePublisher` (and flow after spin) raises snapshot events. UI never pushes state back through snapshots.

### 7.5 Buttons

`WheelButtonAction` subclasses:

- `Bind` → listen `HudStateChanged`, set `interactable` from snapshot flags
- `Execute` → `EventBus.RequestSpin()` / leave / restart only

---

## 8. Gameplay flow (where rules live)

| Piece | Role |
|-------|------|
| `WheelGameState` | Zone, phase, inventory, slices, CanSpin/Leave/Restart from **phase + zone profiles** |
| `WheelGameFlowController` | Handles bus requests, spin completion, publish order |
| `WheelSpinner` | Animation only; duration/rounds from `Settings.SpinDuration` / `MinimumSpinRounds` |
| `WheelSpinResolveCatalog` | Bomb vs win resolve profiles |
| `WheelUiCopyCatalog` | Phase/zone/outcome copy and gameplay flags per phase |
| Rules under `Data/Rules/` | Slice generation, zone type table, layout resolver |

Gameplay rules stay out of Views and Editor.

---

## 9. Editor vs runtime

| Editor | Runtime |
|--------|---------|
| `WheelEditorWiring`, `CollectChildren` | Never referenced |
| `#if UNITY_EDITOR` custom editors | Stripped or inactive in player |

Menus (Vertigo Case): Android setup, project asset repair, play commands, designer windows.

Editor assembly must not become a second runtime.

---

## 10. Anti-patterns (reject in review)

- Public fields on `Definitions` or SOs “for convenience”
- `FindObjectOfType` / `FindDeepChild` for UI pools at runtime
- Extra `WheelXxxCollection` MonoBehaviours only to fill arrays
- String-based child matching (`ui_slice_0_value`) in collectors
- Cross-canvas `[SerializeField]` refs
- Views reading `WheelGameSettings` or formatting milestone/badge strings
- Gameplay logic in `Editor` scripts calling runtime flow directly
- Duplicate event systems (UnityEvents + bus for same intent)
- Mega inspector lists on runtime root
- Silent nulls in UI when wiring is missing (prefer throw on bind)
- Committing secrets or machine paths in `Assets/Config`

---

## 11. Validation

Before considering a feature done:

1. Play mode: spin, bomb, cash out, restart — HUD/wheel/popup update
2. Inspector: `[CollectChildren]` arrays populated, no missing refs on hosts

---

## 12. Adding a new UI pool (checklist)

1. Create hierarchy under correct canvas; sibling order = index order.
2. Add view with `[SerializeField]` pool root + `[CollectChildren]` array.
3. Bind view from canvas host (or same-subtree ref) — **not** from other canvas.
4. Extend `WheelSnapshotFactory` / catalog if new text or colors needed.
5. Subscribe in view `Bind`; apply only snapshot fields in handler.
6. Editor wiring: wire root, `CollectChildren`, `FinalizeCollect` / mark dirty.

---

## 13. Adding new config field (checklist)

1. Add `[SerializeField] private` + get-only property on definition or SO.
2. When renaming a serialized field, migrate existing asset YAML to the new field name in the same change.
3. Expose via snapshot if UI needs it (do not let view read SO directly).
4. Seed default via `Create`/`Default()` for catalog reset.
5. Document in designer-facing asset if non-obvious.

---

## 14. `[CollectChildren]` attribute reference

```csharp
[CollectChildren(
    rootProperty: nameof(_slicePoolRoot),  // null = host.transform
    scope: ChildCollectionScope.DirectChildren,
    includeInactive: true,
    collectOnPreprocessBuild: true)]
```

| Parameter | Meaning |
|-----------|---------|
| `rootProperty` | Name of `Transform` field on same behaviour; empty = collect from host |
| `scope` | `DirectChildren` (default) or `Descendants` |
| `includeInactive` | Include inactive children when collecting |
| `collectOnPreprocessBuild` | Auto-refresh in `ChildCollectionPreprocessBuildCollector` |

Implementation lives in `Assets/Scripts/Collections/Editor/` — not in play builds.

---

## 15. Glossary

| Term | Meaning |
|------|---------|
| **Snapshot** | Immutable UI-facing read model for one frame of state |
| **Host** | Canvas-level binder between locator ready and child views |
| **Definition** | Serializable DTO nested in SO (not an asset file) |
| **Catalog** | Shared SO listing copy, motion, spin resolve, skins, etc. |
| **Pool root** | Transform whose ordered children become array indices |
| **Bootstrap** | Editor/scene-builder path that creates or resets defaults |

---

*Last updated to match the Vertigo Wheel case study architecture (SO-driven config, snapshot UI, `[CollectChildren]` editor binding, encapsulated definitions, single composition root).*
