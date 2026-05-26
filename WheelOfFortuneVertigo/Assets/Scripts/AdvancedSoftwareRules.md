# Advanced Software Rules

This document explains the second layer of architecture rules: view binding lifecycle, marker components, aggregate bindings, static helper classes, and custom attributes.

Base constitution: [`SoftwareArchitectureRules.md`](SoftwareArchitectureRules.md)  
Hierarchy detail: [`HIERARCHY_WIRING.md`](HIERARCHY_WIRING.md)  
DI detail: [`Runtime/DI/README.md`](Runtime/DI/README.md)

---

## 1. Runtime binding lifecycle

The view graph is bound by `WheelViewScope`, not by a central HUD host.

Runtime flow:

1. `WheelViewScope.Awake` builds a `WheelViewContainer` from every `MonoBehaviour` under that canvas scope.
2. `WheelBindDiscovery` selects only classes marked with `[WheelBind]`.
3. When `WheelRuntimeLocator.RuntimeReady` fires, every bindable receives `[WheelInject]` fields.
4. After all fields are assigned, `[WheelAfterInject]` methods run.
5. On scope disable or runtime stop, `[WheelBeforeUnbind]` methods run.

Important consequence:

- A component can be injectable without `[WheelBind]`.
- `[WheelBind]` means "call my lifecycle", not "make me discoverable by type".
- Aggregate binding components should usually be injectable only, not lifecycle participants.
- If `[WheelInject] private SomeBindings _bindings;` fails with `No view binding for SomeBindings`, first check whether the component exists under the same `WheelViewScope` in the scene. Do not add `[WheelBind]` as a workaround.

Correct aggregate pattern:

```csharp
public sealed class WheelOutcomePopupBindings : MonoBehaviour
{
    [SerializeField] private WheelOutcomePopupRootBinding _root;
    [SerializeField] private WheelOutcomePopupIconBinding _icon;

    public void Validate() { /* fail loud */ }
    public void CaptureHome() { /* capture authored transform state */ }
    internal WheelOutcomePopupRefs CreateRefs(...) { /* create presenter refs */ }
}

[WheelBind]
public sealed class WheelOutcomePopupView : MonoBehaviour
{
    [WheelInject] private WheelEventBus _eventBus;
    [WheelInject] private WheelOutcomePopupBindings _bindings;

    [WheelAfterInject]
    private void Connect()
    {
        _bindings.Validate();
    }
}
```

Wrong aggregate pattern:

```csharp
[WheelBind]
public sealed class WheelOutcomePopupBindings : MonoBehaviour
{
    // This has no event lifecycle and should not be invoked by WheelViewScope.
}
```

---

## 2. Marker component rules

Marker components are allowed when the hierarchy has semantic nodes that the runtime must address directly.

Good marker:

- Names one authored scene node.
- Wraps required sibling components.
- Owns tiny scene-state helpers like `CaptureHome`, `SetAlpha`, `RestoreHome`.
- Throws clear errors when a required sibling component is missing.

Bad marker:

- Parses names.
- Searches outside its GameObject.
- Knows gameplay rules.
- Becomes a reusable utility without a clear scene contract.

Example:

```csharp
public sealed class WheelOutcomePopupChromeBinding
    : WheelOutcomePopupSceneComponentBinding<CanvasGroup>
{
    public CanvasGroup CanvasGroup { get { return RequiredComponent; } }
}
```

`WheelOutcomePopupSceneComponentBinding<T>` intentionally lives in the popup folder. It is not a shared utility layer. Its job is narrower: "this popup marker requires this sibling component." If another panel needs the same idea, create a panel-local base first; promote to shared only after two or more panels have the same proven contract.

---

## 3. Aggregate binding rules

Use an aggregate binding when a controller view would otherwise need many serialized fields or many `[WheelInject]` fields.

Good candidates:

- Popup controllers.
- Overlay controllers.
- Multi-part panels with authored child nodes.
- Views that create a presenter/ref object.

Bad candidates:

- Leaf card views.
- Simple buttons.
- Small text views.
- Components with one or two local references.

Aggregate responsibilities:

- Hold serialized scene references under the same hierarchy subtree.
- Validate required references.
- Capture authored home state.
- Build plain runtime ref objects or collaborators.
- Keep optional references rare and explicit.

Aggregate must not:

- Subscribe to events.
- Hold gameplay state.
- Call `WheelRuntimeLocator`.
- Search the hierarchy in play mode.
- Be marked `[WheelBind]` unless it has its own lifecycle.

Rule of thumb:

If a class has more than 7-8 serialized scene references and also has event/presenter logic, split it into:

- `*View` for lifecycle, events, and orchestration.
- `*Bindings` for authored scene references.
- `*Refs` or focused collaborator classes for presenter input.
- `*Animator` for timeline composition only.
- `*Motion` / `*Config` for named timings, offsets, alpha values, and fallback values.

The split must preserve baked scene ownership. Bindings may prepare transient alpha/scale/active state, but they must not repair authored UI settings such as maskability, raycast flags, image type, TMP wrapping/style, anchors, pivots, or chrome geometry at runtime.

---

## 4. Required vs optional bindings

Default to required. Optional is not a styling preference; it is a compatibility contract.

Use required when:

- The object exists in the current hierarchy.
- The feature cannot work correctly without it.
- Missing wiring should fail before shipping.

Use optional only when:

- The same code intentionally supports multiple scene variants.
- The feature is genuinely degraded but valid without that object.
- The missing object is expected in some builds.

Do not leave `[WheelInject(Optional = true)]` or nullable serialized fields just because a migration used to be in progress. After hierarchy is settled, remove the optional path or delete the dead binding type.

Validation should fail loud:

```csharp
private void Require(UnityEngine.Object value, string fieldName)
{
    if (value == null)
    {
        throw new InvalidOperationException(name + " requires popup binding " + fieldName + ".");
    }
}
```

---

## 5. Attribute usage

### `[WheelBind]`

Use on classes that should receive injection lifecycle callbacks.

Allowed:

- Views that subscribe to event bus snapshots.
- Button actions that raise intents.
- Base classes whose subclasses all need the same lifecycle.

Avoid:

- Pure serialized binding holders.
- Marker components with no event lifecycle.
- Plain collaborator objects.

### `[WheelInject]`

Use for same-scope runtime dependencies.

Allowed injected types:

- `WheelEventBus`.
- A unique `Component` type under the same `WheelViewScope`.

Avoid:

- Injecting broad base classes when multiple instances exist.
- Using `Optional = true` to hide bad hierarchy wiring.
- Cross-canvas coupling.

### `[WheelAfterInject]`

Use for:

- Validation after all dependencies exist.
- Event subscription.
- Presenter/collaborator creation.
- Initial reset to a known UI state.

Do not use for:

- Hierarchy search.
- Scene rebuild.
- One-time migration.

### `[WheelBeforeUnbind]`

Use for:

- Event unsubscribe.
- Tween kill.
- Presenter reset.
- Runtime-only cleanup.

The method must be idempotent. It can run during stop, disable, or editor capture cleanup.

### `[CollectChildren]`

Use only for authored child pools that must be serialized in sibling order.

It is editor-populated metadata. Runtime must only read the serialized array.

---

## 6. Static class rules

Static classes are allowed for pure policies and stateless operations. They are not a place to hide lifecycle or mutable session state.

Good static classes:

- `*Rules`: deterministic domain decisions.
- `*Palette`: deterministic color selection.
- `*AnimationConfig`: constants and catalog defaults.
- `*Animator`: stateless tween construction from explicit refs.
- `*Applier`: maps a snapshot onto already-bound refs.
- `WheelUiTweenUtility`, `WheelUiGraphicUtility`: tiny reusable UI operations.

Bad static classes:

- Hold scene references.
- Subscribe to events.
- Read `WheelRuntimeLocator` directly for presentation.
- Mutate hidden global state.
- Replace a missing presenter or service boundary.

Allowed example:

```csharp
internal static class WheelOutcomePopupContentApplier
{
    public static void Apply(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
    {
        binding.Icon.ApplySprite(snapshot.Icon, snapshot.IconTint, 1f);
        binding.ResultText.Apply(snapshot.ResultText, snapshot.TextColor);
    }
}
```

The static class receives all dependencies as parameters. It does not discover the scene.

---

## 7. Presenter and refs rules

Presenter classes own flow decisions for a view, but they should not know Unity hierarchy.

Presenter may:

- Consume snapshots.
- Decide which animation path to run.
- Track small presentation state.
- Call methods on a typed refs object.

Presenter must not:

- Use `GetComponent`, `Find`, `Resources`, or scene names.
- Own serialized fields.
- Mutate game state directly.

Refs objects should be immutable after construction:

```csharp
internal sealed class WheelOutcomePopupRefs
{
    public WheelOutcomePopupRootBinding Root { get; }
    public WheelOutcomePopupIconBinding Icon { get; }

    public WheelOutcomePopupRefs(
        WheelOutcomePopupRootBinding root,
        WheelOutcomePopupIconBinding icon)
    {
        Root = root;
        Icon = icon;
    }
}
```

Do not use public mutable field bags for presenter refs. If a ref can change, there must be a named method that explains why.

---

## 8. Editor scripts and migrations

Editor scripts are for repeatable tooling, not permanent cleanup leftovers.

Keep:

- Inspectors.
- Asset pipeline tools.
- Build preprocess collectors.
- Supported validators and project setup checks.
- Scene builders that regenerate supported hierarchy.

Delete after use:

- One-off migrations.
- Temporary rebuilders.
- Screenshot/export scripts that only served a local debug pass.
- Play-mode command helpers that are not part of the supported validation path.

If a hierarchy change is now permanent, encode it in:

- The scene.
- The scene builder, if that builder owns the same objects.
- The relevant validator or setup check.
- The relevant binding aggregate.

Do not keep a migration around to explain history.

---

## 9. Android / player build expectations

Player builds do not run editor collection or migration scripts.

Before Android APK build:

- Scene references must already be serialized.
- `[CollectChildren]` arrays must already be populated or collected by preprocess build.
- Optional bindings must represent real supported variants, not missing scene work.
- No runtime script should depend on editor menus, editor-only rebuilders, or hierarchy search.

Safe in APK:

- `WheelViewScope` discovery under the active canvas scope.
- `[WheelBind]`, `[WheelInject]`, `[WheelAfterInject]`, `[WheelBeforeUnbind]`.
- Serialized aggregate bindings.
- Static helper classes that receive explicit refs/snapshots.

Not safe in APK:

- `UnityEditor` APIs.
- One-off migration scripts.
- Runtime `FindObjectOfType` or deep name search.
- Assuming editor scripts will repair missing references at launch.

---

## 10. Review checklist

Before merging a UI architecture change:

1. Does every `[WheelBind]` class actually need lifecycle callbacks?
2. Does every `[WheelInject]` required dependency exist once in the same scope?
3. Are optional dependencies backed by a real supported scene variant?
4. Are large field lists moved into a local `*Bindings` aggregate?
5. Are marker components named for scene semantics, not generic utility ideas?
6. Are refs immutable after construction?
7. Do static classes receive all dependencies as parameters?
8. Did editor-only migration/debug scripts get deleted after the permanent scene/schema changed?
9. Does the Android/player path work with serialized data only?
