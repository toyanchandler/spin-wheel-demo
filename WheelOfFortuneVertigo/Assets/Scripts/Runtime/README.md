# Runtime layer

Gameplay session: state, flow, spin, publish, bootstrap.

**Logic & architecture:** [`../ARCHITECTURE_AND_LOGIC.md`](../ARCHITECTURE_AND_LOGIC.md)

| Folder | Responsibility |
|--------|----------------|
| `Bootstrap/` | `WheelRuntimeCompositionRoot`, `WheelRuntimeLocator`, `WheelGameplaySession` |
| `Events/` | `WheelEventBus` + capability interfaces (`IWheelUiIntent*`, `IWheelSnapshot*`) |
| `Flow/` | `WheelGameFlowController` — player journeys |
| `Game/` | `WheelGameState`, `IRandomSource`, `WheelSpinResolvePipeline`, inventory |
| `Publishing/` | Snapshots + `WheelStatePublisher` |
| `Spin/` | `IWheelSpinDriver`, `WheelSpinner`, `WheelSpinAnglePlanner` |
| `DI/` | View scopes and injection |
