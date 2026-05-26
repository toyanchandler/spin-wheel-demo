# Software Architecture Rules

**Vertigo Wheel Case Study — project constitution**

This document is the single source of truth for how we structure code, scene hierarchy, data, events, and editor tooling. If a change conflicts with a rule here, the change is wrong unless this document is updated first.

Related deep dives:

- `AdvancedSoftwareRules.md` — binding lifecycle, attributes, static helpers, aggregate bindings
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
8. **Baked UI stays authored** — runtime may animate, show/hide, and apply snapshot data, but must not overwrite authored component settings such as `maskable`, raycast flags, image type, anchors, pivots, or baked chrome geometry.

---

## 2. Layer map

```
Assets/Config/          → ScriptableObject assets (designer data)
Assets/Scripts/Collections/ → [CollectChildren] runtime attribute + editor collectors
Assets/Scripts/Data/        → SO classes, Definitions, Rules, SceneSettings (no .asset files here)
Assets/Scripts/Diagnostics/ → Debug/performance monitoring only
Assets/Scripts/Editor/      → Menus, inspectors, scene builders, asset tools (never referenced at runtime)
Assets/Scripts/Runtime/     → Play mode gameplay, bootstrap, DI, events, publishing, spin
Assets/Scripts/Views/       → Scene UI components, panel collaborators, wheel presentation
```

### 2.1 Runtime code folders

| Folder | Namespace | Responsibility |
|--------|-----------|----------------|
| `Collections/` | `Vertigo.Collections` | Runtime metadata only: `CollectChildrenAttribute` and collection enums |
| `Collections/Editor/` | `Vertigo.Collections.Editor` | Collect/reset/preprocess/property drawer tooling |
| `Data/Definitions/` | `Vertigo.Wheel.Data` | Serializable DTOs, enums, profiles, content contracts |
| `Data/Rules/` | `Vertigo.Wheel.Data` | Pure decision helpers: slice generation, slot catalog, zone tables, queries |
| `Data/SceneSettings/` | `Vertigo.Wheel.Data` | Scene-authored data contracts when present |
| `Data/ScriptableObjects/` | `Vertigo.Wheel.Data` | SO classes whose assets live under `Assets/Config/` |
| `Diagnostics/` | `Vertigo.Wheel.Diagnostics` | Development/performance overlays and metric snapshots |
| `Runtime/Bootstrap/` | `Vertigo.Wheel.Runtime` | Composition root, config source, runtime locator |
| `Runtime/DI/` | `Vertigo.Wheel.Runtime` | `WheelViewScope`, same-scope container, injection attributes, lifecycle invocation |
| `Runtime/Events/` | `Vertigo.Wheel.Runtime` | `WheelEventBus` and intent/snapshot event surface |
| `Runtime/Flow/` | `Vertigo.Wheel.Runtime` | Spin/leave/restart flow controller |
| `Runtime/Game/` | `Vertigo.Wheel.Runtime` | Mutable session state, reward inventory, spin result |
| `Runtime/Publishing/` | `Vertigo.Wheel.Runtime` | Snapshot structs and `WheelStatePublisher` |
| `Runtime/Spin/` | `Vertigo.Wheel.Runtime` | Wheel spin animation component |

### 2.2 View folders

`Vertigo.Wheel.Views` is organized by authored surface, not by old HUD/outcome buckets.

| Folder | Responsibility |
|--------|----------------|
| `Views/Wheel/` | Wheel root, slice views, skin application, slice presentation |
| `Views/Shared/` | Small shared view bases/utilities such as `WheelButtonAction`, `WheelHudTextView`, tween/graphic helpers |
| `Views/Panels/Background/` | Background tint/presentation view |
| `Views/Panels/CardReveal/` | Cashout/opening overlay, card bindings, deck renderer, scroller, burst animation |
| `Views/Panels/ExitConfirmation/` | Exit confirmation overlay |
| `Views/Panels/LootPanel/` | Current loot panel, loot cards, inventory buffer, renderer, layout, landing resolver |
| `Views/Panels/RewardPopup/` | Outcome popup view, aggregate bindings, presenter, reveal sequence, reward flight, popup-local bindings |
| `Views/Panels/WheelSpinPanel/` | Spin/leave/restart buttons and footer status text |
| `Views/Panels/ZonePanel/` | Zone progress, zone cells, milestone badges, zone progress window |

Legacy `Views/Buttons/`, `Views/Hud/`, `Views/Outcome/`, and `Views/Hosts/` folders are obsolete. Do not recreate them.

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

### 4.3 View scopes

Each UI canvas root owns exactly one `WheelViewScope`.

At `Awake`, the scope builds a `WheelViewContainer` from every `MonoBehaviour` under the same canvas subtree. This makes same-scope components injectable by type.

Separately, `WheelBindDiscovery` selects only `[WheelBind]` components for lifecycle. Only those lifecycle participants receive `[WheelInject]`, `[WheelAfterInject]`, and `[WheelBeforeUnbind]`.

View scopes only:

1. Subscribe to `WheelRuntimeLocator.RuntimeReady` / `RuntimeStopped`.
2. Build the same-scope component registry.
3. Inject lifecycle participants.
4. Call `[WheelAfterInject]` / `[WheelBeforeUnbind]` lifecycle methods.

No legacy `*UiHost` scripts should remain in the scene or project.

### 4.4 Composition root

`game_wheel_runtime`:

- `WheelRuntimeCompositionRoot` — **zero** serialized fields; uses `GetComponent` for gameplay on same GO.
- `WheelGameConfigSource` — single SO ref to `WheelGameSettings.asset`.
- `WheelGameState`, `WheelStatePublisher`, `WheelGameFlowController` on same GO.

No legacy `game_wheel_settings` scene object.

### 4.5 Aggregate bindings vs views

Large authored UI surfaces should split scene references from runtime orchestration.

| Class shape | Responsibility |
|-------------|----------------|
| `*View` | `[WheelBind]` lifecycle, event subscription, snapshot orchestration, public intent/landing APIs |
| `*Bindings` | Serialized authored references, validation, listener hookup, home-state capture, collaborator factories |
| Leaf binding (`*CardBinding`, `*RootBinding`) | Prepare/clear the component's own transient state only |
| `*Animator` | Build DOTween timelines from explicit bindings and named motion values |
| `*Motion` / `*Config` | Named timing, alpha, scale, offset, and fallback values |
| State/renderer/layout collaborators | Buffers, diffing, card rendering, scroll/layout math, landing-position math |

Aggregate `*Bindings` components are injectable because `WheelViewContainer` registers every `MonoBehaviour` under the same `WheelViewScope`. They do **not** need `[WheelBind]` unless they have their own event lifecycle. In the normal case, `[WheelBind]` belongs on the controller `*View`, not on the aggregate binding holder.

Correct pattern:

```csharp
public sealed class WheelRewardOpeningBindings : MonoBehaviour
{
    [SerializeField] private Button _previousButton;
    [SerializeField] private Button _nextButton;

    public void Validate() { /* fail loud */ }
    internal WheelRewardOpeningScroller CreateScroller(Object target) { /* pass explicit refs */ }
}

[WheelBind]
public sealed class WheelRewardOpeningView : MonoBehaviour
{
    [WheelInject] private WheelEventBus _eventBus;
    [WheelInject] private WheelRewardOpeningBindings _bindings;
}
```

Do not mark an aggregate binding `[WheelBind]` just to make injection work. If injection fails with `No view binding for X`, the scene is missing that component under the same scope, or the scene serialization has not been migrated.

### 4.6 Baked component settings

Scene and prefab authorship owns stable UI configuration:

- Image `type`, `preserveAspect`, `maskable`, raycast flags, materials, sprites used as static chrome.
- TMP wrapping, overflow, font style, maskability, base color.
- ScrollRect direction/movement settings, viewport/content assignment.
- RectTransform anchors, pivots, base size, shadow/glow/shine authored geometry.

Runtime view code may update:

- Snapshot-driven content: text value, reward sprite, amount text, enabled/active state.
- Transient presentation: alpha, scale, local rotation, anchored position used by animation/layout.
- Dynamic sizes that are genuinely data-dependent, such as scroll content height for a reward count.

If a baked setting is wrong, fix the scene/prefab/builder. Do not patch it every frame or every render call from a view.

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
- `RuntimeReady` / `RuntimeStopped` signals for `WheelViewScope` instances

Registration only from composition root / spinner `Awake`. Cleared on stop.

### 6.2 Lifecycle

```
CompositionRoot.Start
  → Register settings + gameplay
  → State.InitializeRuntime()
  → Publisher.Bind(bus), Flow.Bind(bus), Spinner.Bind(bus)
  → NotifyRuntimeReady()  → view scopes inject lifecycle participants
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

Views subscribe from `[WheelAfterInject]` and unsubscribe from `[WheelBeforeUnbind]`. No orphaned delegates.

### 7.2 Snapshot types (readonly structs)

- `WheelHudSnapshot` — zone label, phase, buttons flags, loot sprites, milestone text/colors, reward cards, etc.
- `WheelZoneSnapshot` — slices, positions, skin tier, background
- `WheelOutcomeSnapshot` — popup copy, colors, motion, icon

Built only in `WheelSnapshotFactory` from `WheelGameState` + `WheelGameSettings` + catalogs.

### 7.3 View contract

```csharp
[WheelBind]
public sealed class WheelStatusTextView : MonoBehaviour
{
    [WheelInject] private WheelEventBus _eventBus;

    [WheelAfterInject]
    private void Connect()
    {
        _eventBus.HudStateChanged += OnHudStateChanged;
    }

    [WheelBeforeUnbind]
    private void Disconnect()
    {
        _eventBus.HudStateChanged -= OnHudStateChanged;
    }

    private void OnHudStateChanged(WheelHudSnapshot snapshot)
    {
        _titleText.SetText(snapshot.SuperMilestoneBadgeText);
        _badgeImage.color = snapshot.SuperMilestoneBadgeColor;
    }
}
```

Forbidden in views:

- Reading `WheelGameSettings` for presentation strings/colors
- Formatting `"SAFE\nZONE\n{0}"` locally
- Hard-coded theme colors “just for this label”
- Mixing event orchestration, serialized scene reference ownership, animation timeline construction, state diffing, and layout math in one large `MonoBehaviour`
- Overwriting baked component settings (`maskable`, `preserveAspect`, raycast flags, Image type, TMP wrapping/style, anchors/pivots) while rendering snapshots

### 7.4 Reward presentation split

Reward presentation surfaces (`WheelRewardPanelView`, `WheelLootCardView`, `WheelRewardOpeningView`, `WheelRewardCardView`, outcome popup controllers) must stay single-purpose:

- View classes own lifecycle and event flow only.
- Binding classes own authored references and component-local prepare/clear state.
- Animator classes only compose timelines; they do not discover objects or decide gameplay state.
- Motion/config classes hold named values; avoid magic numbers inside view or animator method bodies.
- Inventory/pending buffers, renderer loops, scroll layout, and landing target math belong in focused collaborators, not in the view.

The reward flow is callback-driven. Popup-to-panel commits should stay tied to tween arrival/completion callbacks, not hardcoded timer guesses.

### 7.5 Publishing

Only `WheelStatePublisher` (and flow after spin) raises snapshot events. UI never pushes state back through snapshots.

### 7.6 Buttons

`WheelButtonAction` subclasses:

- `[WheelAfterInject]` → listen `HudStateChanged`, set `interactable` from snapshot flags
- `[WheelBeforeUnbind]` → remove listeners and kill transient tweens
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

Menus (Vertigo Case): Android setup, project asset repair, and Wheel Designer.

Editor assembly must not become a second runtime.

---

## 10. Anti-patterns (reject in review)

- Public fields on `Definitions` or SOs “for convenience”
- `FindObjectOfType` / `FindDeepChild` for UI pools at runtime
- Extra `WheelXxxCollection` MonoBehaviours only to fill arrays
- String-based child matching (`ui_slice_0_value`) in collectors
- Sorting collected UI pools by GameObject name at runtime instead of using serialized sibling order
- Cross-canvas `[SerializeField]` refs
- Views reading `WheelGameSettings` or formatting milestone/badge strings
- Views that become "god scripts" containing bindings, snapshot diffing, layout, animation, particle cleanup, and scroll math together
- Runtime style repair such as setting `maskable`, `raycastTarget`, `preserveAspect`, `Image.type`, TMP overflow/wrapping, anchors, pivots, or authored shadow/glow geometry during render
- `[WheelBind]` on pure aggregate binding holders just to make them injectable
- Gameplay logic in `Editor` scripts calling runtime flow directly
- Duplicate event systems (UnityEvents + bus for same intent)
- Mega inspector lists on runtime root
- Silent nulls in UI when wiring is missing (prefer throw on bind)
- Committing secrets or machine paths in `Assets/Config`

---

## 11. Validation

Before considering a feature done:

1. Play mode: spin, bomb, cash out, restart — HUD/wheel/popup update
2. Inspector: `[CollectChildren]` arrays populated, each UI canvas has exactly one `WheelViewScope`
3. Console after entering play mode: no `No view binding for X`, compile, or lifecycle exceptions
4. Architecture scan for touched views: no runtime `Find*`, no `new GameObject`, no name-parsed pool ordering, no baked UI setting overrides

---

## 12. Adding a new UI pool (checklist)

1. Create hierarchy under correct canvas; sibling order = index order.
2. Add view with `[SerializeField]` pool root + `[CollectChildren]` array.
3. Add `[WheelBind]` only to lifecycle participants; keep pure aggregate `*Bindings` injectable-only.
4. Extend `WheelSnapshotFactory` / catalog if new text or colors needed.
5. Subscribe in `[WheelAfterInject]`; apply only snapshot fields in handlers.
6. Editor wiring: wire root, `CollectChildren`, `FinalizeCollect` / mark dirty.
7. If the pool view has more than a few serialized references, split `*Bindings`, `*View`, `*Animator`, `*Motion`, and focused state/layout collaborators before it grows.

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
    rootProperty: nameof(_slicePoolRoot),  // null = target component transform
    scope: ChildCollectionScope.DirectChildren,
    includeInactive: true,
    collectOnPreprocessBuild: true)]
```

| Parameter | Meaning |
|-----------|---------|
| `rootProperty` | Name of `Transform` field on same behaviour; empty = collect from target component transform |
| `scope` | `DirectChildren` (default) or `Descendants` |
| `includeInactive` | Include inactive children when collecting |
| `collectOnPreprocessBuild` | Auto-refresh in `ChildCollectionPreprocessBuildCollector` |

Implementation lives in `Assets/Scripts/Collections/Editor/` — not in play builds.

---

## 15. Glossary

| Term | Meaning |
|------|---------|
| **Snapshot** | Immutable UI-facing read model for one frame of state |
| **View scope** | Canvas-level binder that builds same-scope DI registry and invokes `[WheelBind]` lifecycle |
| **Definition** | Serializable DTO nested in SO (not an asset file) |
| **Catalog** | Shared SO listing copy, motion, spin resolve, skins, etc. |
| **Pool root** | Transform whose ordered children become array indices |
| **Bootstrap** | Editor/scene-builder path that creates or resets defaults |

---

*Last updated to match the Vertigo Wheel case study architecture (SO-driven config, snapshot UI, `[CollectChildren]` editor binding, encapsulated definitions, single composition root).*
