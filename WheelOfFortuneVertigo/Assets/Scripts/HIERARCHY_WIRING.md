# Hierarchy Wiring Rules

> Full constitution: [`SoftwareArchitectureRules.md`](SoftwareArchitectureRules.md)

## SerializeField

- Only reference components on the **same GameObject** or **descendants** in that subtree.
- **ScriptableObject** catalogs in `Assets/Config/` are allowed (shared data).
- Never drag references across canvas roots, runtime roots, or unrelated branches.

## Cross-system communication

Use one of:

1. `WheelRuntimeLocator` — gameplay session (settings, state, publisher, spinner, event bus)
2. `WheelEventBus` — UI binds on `RuntimeReady`, unbinds on `RuntimeStopped`
3. Shared SO catalogs — `WheelGameSettings`, skin, UI copy, layout, resolve profiles (`Assets/Config/`)

`game_wheel_runtime` holds only `WheelGameConfigSource` → `WheelGameSettings.asset` (same GameObject, SO asset reference).

## Child collection (`Vertigo.Collections`)

Editor-only helper. **No extra collection MonoBehaviours.**

| Piece | Role |
|-------|------|
| `[CollectChildren]` | Metadata on a Component array field (+ optional root `Transform` field name) |
| `CollectChildrenPropertyDrawer` | **Collect / Reset Children**, preprocess toggle, **(i)** help foldout |
| `ChildCollectionEditorService` | Collect + serialize in editor tooling and preprocess build |
| View-specific `CustomEditor` | Custom bindings (zone progress cells) |

### Example

```csharp
[SerializeField] private Transform _cardPoolRoot;
[CollectChildren(nameof(_cardPoolRoot))]
[SerializeField] private WheelRewardCardView[] _cardViews;
```

Ordering = hierarchy sibling index under the pool root. GameObject names are not used.

### Runtime

Views use the pre-filled serialized arrays only.

## Validation

Use the view-specific collectors before entering play mode.
