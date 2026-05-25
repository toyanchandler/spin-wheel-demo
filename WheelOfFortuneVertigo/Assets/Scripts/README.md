# Wheel Scripts — klasör rehberi

**Architecture constitution:** [`SoftwareArchitectureRules.md`](SoftwareArchitectureRules.md)

## İki farklı “config”

| Konum | Ne |
|--------|-----|
| `Assets/Config/*.asset` | **ScriptableObject dosyaları** (Unity asset). Inspector’da düzenlenir. |
| `Assets/Scripts/Data/` | Bu asset’leri tanımlayan **C# sınıfları** ve kurallar. |

`Scripts/Data` ≠ asset klasörü. Karışıklık buradan geliyordu; eski ad `Config` kaldırıldı.

## `Editor/`

Sadece Unity Editor: menüler ve asset pipeline. **Play modda çalışmaz.**

## `Runtime/`

Oyun açıkken çalışan mantık (MonoBehaviour + saf C#).

| Alt klasör | Sorumluluk |
|------------|------------|
| `Bootstrap/` | Composition root, `WheelRuntimeLocator`, event bus |
| `Views/Hosts/` | Canvas-level UI hosts (child-only wiring) |

See `HIERARCHY_WIRING.md` for serialization rules.
| `Events/` | `WheelEventBus` |
| `Flow/` | Spin / leave / restart akışı |
| `Game/` | `WheelGameState`, envanter, spin sonucu |
| `Publishing/` | Snapshot üretimi ve event publish |
| `Spin/` | Çark animasyonu (`WheelSpinner`) |

## `Views/`

Sahnedeki UI `MonoBehaviour` bileşenleri (eski `UI/`).

| Alt klasör | Örnek |
|------------|--------|
| `Hud/` | Zone metni, status, arka plan |
| `Wheel/` | Dilimler, skin, `WheelView` |
| `Outcome/` | Sonuç popup |
| `Buttons/` | Spin / leave / restart |
| `Shared/` | `WheelHudTextView`, `WheelButtonAction` tabanı |

## `Data/`

Veri modeli ve kurallar (asset dosyası yok).

| Alt klasör | Ne |
|------------|-----|
| `ScriptableObjects/` | `[CreateAssetMenu]` sınıfları → `Assets/Config/` altına asset üretir |
| `Definitions/` | Struct, enum, profil (`WheelSliceDefinition`, …) |
| `Rules/` | Generator, resolver, statik tablolar |
| `ScriptableObjects/` | `WheelGameSettings` ve diğer config SO scriptleri |

## `Diagnostics/`

Performans overlay / monitor (debug).

## Namespace

- `Vertigo.Wheel.Runtime`
- `Vertigo.Wheel.Views`
- `Vertigo.Wheel.Data`
- `Vertigo.Wheel.EditorTools`
- `Vertigo.Wheel.Diagnostics`
