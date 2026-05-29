# Scene wiring standard (all panels)

Three layers — do not mix them up:

| Layer | What | Example |
|-------|------|---------|
| **Gameplay → UI** | `WheelEventBus` snapshots + intents | `HudStateChanged` |
| **View ↔ view** | `WheelEventBus.Presentation` channels | `Presentation.Loot` |
| **Scene hierarchy** | `[SerializeField]` wiring on `*Bindings` | `WheelOutcomePopupSceneWiring` |

Scene wiring is **not** DI, **not** reflection, **not** `GetComponentInChildren` at runtime.

## Standard panel shape

```
MyPanelView          [WheelBind]  → bus + lifecycle only
MyPanelBindings      one MonoBehaviour on same root
  └── _wiring        one [Serializable] struct OR flat fields
MyPanelHandles       (optional) plain C# built from wiring — tweens, no MB
```

### Preferred: one `_wiring` struct

```csharp
[Serializable]
public sealed class WheelOutcomePopupSceneWiring
{
    [Header("Content")]
    public Image Icon;
    public TextMeshProUGUI ResultText;
}

public sealed class WheelOutcomePopupBindings : MonoBehaviour
{
    [SerializeField] private WheelOutcomePopupSceneWiring _wiring;
    // Build handles once; validate in one place
}
```

Same idea as `WheelRewardOpeningBindings` and `WheelExitConfirmationBindings`.

### Avoid: per-child `*Binding` MonoBehaviours

Do **not** add empty `WheelFooIconBinding : MonoBehaviour` on every child only so the parent can hold 12 serialized references. That was the old outcome-popup pattern.

Logic lives in `*Handles` (plain classes). Inspector refs live in `*SceneWiring`.

## Arrays / pools

Use `[CollectChildren]` on the pool root (loot cards, wheel slices, zone cells) — editor collects once, runtime reads the array.

## Migrating old outcome popup

Menu: **Vertigo Case → Wiring → Migrate Outcome Popup To Flat Wiring** (editor only, selected popup root).

Then remove obsolete child `*Binding` components from the hierarchy.
