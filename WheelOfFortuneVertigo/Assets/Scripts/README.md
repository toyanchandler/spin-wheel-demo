# Wheel Scripts — klasör rehberi

**Architecture constitution:** [`SoftwareArchitectureRules.md`](SoftwareArchitectureRules.md)
**Advanced rules:** [`AdvancedSoftwareRules.md`](AdvancedSoftwareRules.md)

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
| `DI/` | Attribute tabanlı same-scope view injection/lifecycle — [`DI/README.md`](Runtime/DI/README.md) |
| `Events/` | `WheelEventBus` |
| `Flow/` | Spin / leave / restart akışı |
| `Game/` | `WheelGameState`, envanter, spin sonucu |
| `Publishing/` | Snapshot üretimi ve event publish |
| `Spin/` | Çark animasyonu (`WheelSpinner`) |

See `HIERARCHY_WIRING.md` for serialization rules.

## `Views/`

Sahnedeki UI `MonoBehaviour` bileşenleri (eski `UI/`).

| Alt klasör | Örnek |
|------------|--------|
| `Panels/ZonePanel/` | Zone progress ve milestone badge |
| `Panels/LootPanel/` | Current loot panel ve loot card |
| `Panels/WheelSpinPanel/` | Spin / leave / restart butonları ve footer status |
| `Panels/RewardPopup/` | Sonuç popup |
| `Panels/CardReveal/` | Cashout card reveal |
| `Panels/Background/` | Arka plan tint view |
| `Panels/ExitConfirmation/` | Exit confirmation overlay |
| `Wheel/` | Dilimler, skin, `WheelView` |
| `Shared/` | `WheelHudTextView`, `WheelButtonAction` tabanı |

Canvas başına bir `WheelViewScope` (`Runtime/DI/`) — eski `*UiHost` script'leri kaldırıldı.

## `Data/`

Veri modeli ve kurallar (asset dosyası yok).

| Alt klasör | Ne |
|------------|-----|
| `ScriptableObjects/` | `[CreateAssetMenu]` sınıfları → `Assets/Config/` altına asset üretir |
| `Definitions/` | Struct, enum, profil (`WheelSliceDefinition`, …) |
| `Rules/` | Generator, resolver, statik tablolar |
| `SceneSettings/` | Scene-authored settings contracts when needed |

## `Diagnostics/`

Performans overlay / monitor (debug).

## Namespace

- `Vertigo.Wheel.Runtime`
- `Vertigo.Wheel.Views`
- `Vertigo.Wheel.Data`
- `Vertigo.Wheel.EditorTools`
- `Vertigo.Wheel.Diagnostics`
