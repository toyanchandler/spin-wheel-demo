# View presentation registry

Cross-view coordination without type references or hierarchy search.

## Rules

1. **Gameplay** → `WheelEventBus` snapshots + UI intents only.
2. **View ↔ view** → `WheelEventBus.Presentation` channels.
3. **Same GameObject bindings** → `[SerializeField]` or `GetComponent` on self — never `WheelViewContainer.Resolve` for `*Bindings`.

## Channels

| Type | Location |
|------|----------|
| `WheelViewPresentationRegistry` | Aggregates all channels; cleared in `WheelEventBus.Clear()` |
| `WheelLootPresentationChannel` | Loot flight (outcome popup → strip) |
| `WheelSpinPresentationChannel` | Spin driver + slice layout (spinner ↔ wheel views) |

## Adding a new cross-view need

1. Add a small interface under `Runtime/Presentation/` (e.g. `IWheelFooHandler`).
2. Add a channel with `Register` / `Clear` / forward methods.
3. Expose on `WheelViewPresentationRegistry` (or nested channel).
4. Provider view: `Register(this)` in `[WheelAfterInject]`, `Clear()` in `[WheelBeforeUnbind]`.
5. Consumer: call `_eventBus.Presentation.YourChannel` only — no `using` of the provider view type.
