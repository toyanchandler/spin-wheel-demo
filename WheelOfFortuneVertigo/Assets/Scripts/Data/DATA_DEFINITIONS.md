# Data definitions (case study pattern)

> Full constitution: [`../SoftwareArchitectureRules.md`](../SoftwareArchitectureRules.md)

## Rule

| Layer | Pattern |
|-------|---------|
| **ScriptableObject roots** (`WheelGameSettings`, catalogs) | `[SerializeField] private` + public **get-only** API |
| **Definitions** (nested DTOs under `Assets/Scripts/Data/Definitions/`) | Same: serialized private fields, **no public mutable fields** |
| **Runtime mutation** (e.g. slice pool fill) | Explicit methods (`WheelSliceDefinition.ApplySlot`) |
| **Editor asset pipeline** | Narrow APIs (`SetBombReward`, `BindFrameSprites`) |

## Examples

```csharp
[SerializeField] private Color _safeZoneColor = ...;
public Color SafeZoneColor => _safeZoneColor;
```

```csharp
public static RewardDefinition Create(string id, string displayName, ...) { ... }
```

Unity still serializes `_safeZoneColor` in YAML; gameplay code reads `SafeZoneColor` only.

## UI copy structs

`WheelZoneUiCopy`, `WheelPhaseUiCopy`, `WheelOutcomeUiCopy` live in `WheelUiCopyEntries.cs` and use the same encapsulation + static `Create(...)` for defaults.
