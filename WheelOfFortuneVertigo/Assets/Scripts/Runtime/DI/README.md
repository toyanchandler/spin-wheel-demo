# Wheel View DI (attribute tabanlı bağlama)

Mini Zenject tarzı bir sistem: view script'lerine attribute yazarsın, `WheelViewScope` altındaki her şeyi otomatik bulur, inject eder ve lifecycle'ı çalıştırır.

**Constitution ile uyum:** Play mode'da `transform.Find`, isim araması veya host'ta elle referans listesi yok. Editor'da hierarchy toplanır (`[CollectChildren]` vb.); runtime sadece serialize edilmiş / inject edilmiş referansları okur.

---

## Neden yaptık?

Eski modelde `WheelHudUiHost` gibi canvas host'larında 15+ `[SerializeField]` vardı:

- Hierarchy değişince referanslar `None` kalıyordu
- Yeni view eklemek = host script'ini + inspector'ı güncellemek
- Aynı tip birden fazla olduğunda (3 restart butonu) liste yönetimi zorlaşıyordu
- View'lar host'a sıkı bağlıydı (decoupled değildi)

Yeni model:

| Eski | Yeni |
|------|------|
| Host'ta manuel referans listesi | View kendi `[WheelBind]` lifecycle marker'ını taşır |
| `Bind(WheelEventBus)` her yerde | `[WheelAfterInject]` / `[WheelBeforeUnbind]` |
| View → view için host üzerinden geçiş | `[WheelInject]` ile aynı scope'tan çözülür |
| Kocaman host CS dosyası | İnce `WheelViewScope` (tek component, sıfır field) |

---

## Mimari (kısa)

```
WheelRuntimeCompositionRoot
  └── WheelEventBus publish eder
        └── WheelRuntimeLocator.RuntimeReady

HudCanvas / WheelCanvas / StaticCanvas
  └── WheelViewScope                    ← tek sorumluluk: scope altını bağla
        ├── [WheelBind] WheelZoneProgressView
        ├── [WheelBind] WheelRewardPanelView
        ├── [WheelBind] WheelOutcomePopupView   (+ [WheelInject] loot panel)
        └── [WheelBind] WheelSpinButtonAction     (base class'ta attribute)
```

**Akış:**

1. `WheelViewScope.Awake` → scope altındaki tüm `MonoBehaviour`'ları `WheelViewContainer`'a register eder
2. `WheelBindDiscovery` → `[WheelBind]` (veya base class'ta tanımlı) script'leri listeler
3. Runtime hazır olunca → her bindable için `WheelInjector.Inject`:
   - `[WheelInject]` field'ları doldurur
   - `[WheelAfterInject]` metodlarını çağırır
4. Stop/disable → `[WheelBeforeUnbind]` metodları

---

## Sahne kurulumu

Her UI canvas kökünde **bir** `WheelViewScope` olmalı (MainScene: `HudCanvas`, wheel canvas, background canvas).

Inspector'da scope seçiliyken bindable sayısı görünür:

> Auto-wiring: N [WheelBind] scripts under this scope.

Eski host migration'ı tamamlandı. `WheelHudUiHost`, `WheelWheelUiHost`, `WheelStaticUiHost`, ve `WheelUiHostBase` geri eklenmez.

---

## Yeni view ekleme (3 adım)

### 1. Script'e attribute koy

```csharp
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class MyNewHudView : MonoBehaviour
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
            // render
        }
    }
}
```

### 2. GameObject'i doğru canvas altına koy

`WheelViewScope` olan canvas'ın **child'ı** (veya deeper descendant) olmalı.

### 3. Bitti

Host script'ine referans ekleme, inspector'da sürükle-bırak yok. Play'de otomatik bağlanır.

---

## Attribute referansı

### `[WheelBind]` (class)

Script'i scope lifecycle döngüsüne dahil eder.

- Base class'a yazılırsa subclass'lar da inherit eder (`WheelButtonAction`, `WheelHudTextView`).
- Sadece `[WheelBind]` lifecycle katılımcıları `[WheelInject]`, `[WheelAfterInject]`, `[WheelBeforeUnbind]` akışından geçer.
- Bir component aynı scope içinde inject edilebilir olmak için `[WheelBind]` taşımak zorunda değildir; `WheelViewContainer` scope altındaki tüm `MonoBehaviour` tiplerini register eder.
- Scope dışındaki script'ler bind edilmez.

### `[WheelInject]` (field)

Inject edilebilir tipler:

- `WheelEventBus` → runtime'dan gelir
- Herhangi bir `Component` → aynı scope'taki `WheelViewContainer`'dan tip ile bulunur

```csharp
[WheelInject] private WheelEventBus _eventBus;
[WheelInject] private WheelRewardPanelView _lootPanel;
```

Required inject varsayılandır. Eksik dependency varsa sahne/hiyerarşi düzeltilir veya dependency aggregate binding ile tek tipe indirilir. `Optional = true` yeni UI akışında kullanılmaz; yalnızca gerçekten desteklenen birden fazla scene variant'ı varsa ayrı mimari karar olarak eklenir.

### `[WheelAfterInject]` (method, parametresiz)

Tüm `[WheelInject]` field'ları doldurulduktan **sonra** çağrılır. Event subscribe, validation, presenter init burada.

### `[WheelBeforeUnbind]` (method, parametresiz)

Scope unbind olurken çağrılır. Unsubscribe, tween kill, presenter reset burada.

---

## View → view coordination (presentation registry)

Başka bir view'a ihtiyaç varsa **tip inject etme**. `WheelEventBus.Presentation` kullan:

| Channel | Register (provider) | Call (consumer) |
|---------|---------------------|-----------------|
| `Presentation.Loot` | `WheelRewardPanelView` implements `IWheelLootFlightHandler` | Outcome popup via `Presentation.Loot.HoldForArrival()` etc. |
| `Presentation.Spin` | Composition root registers driver; `WheelView` registers slice layout | Spinner + `WheelSkinView` read spin state |

```csharp
[WheelBind]
public sealed class WheelOutcomePopupView : MonoBehaviour
{
    [SerializeField] private WheelOutcomePopupBindings _bindings; // same GameObject only
    [WheelInject] private WheelEventBus _eventBus;

    [WheelAfterInject]
    private void Connect()
    {
        _presenter = new WheelOutcomePopupPresenter(
            _bindings.CreateRefs(_eventBus.Presentation.Loot, ...), this);
    }
}
```

`WheelViewContainer.Resolve<T>()` is for **optional** same-scope components — not for cross-view coupling. Multiple `WheelRestartButtonAction` instances never resolve each other.

---

## Base class pattern

Ortak UI parçaları base'de `[WheelBind]` taşır:

- `WheelHudTextView` — zone/status metinleri
- `WheelButtonAction` — spin / leave / restart butonları

Subclass sadece davranışı override eder; host veya attribute tekrarı gerekmez.

---

## Hierarchy / child collection ile birlikte

View DI, inspector'daki **child collection** ile çakışmaz; farklı katmanlar:

| Mekanizma | Ne toplar | Ne zaman |
|-----------|-----------|----------|
| `[WheelBind]` + `WheelViewScope` | Event bus'a bağlanacak lifecycle katılımcıları | Runtime lifecycle |
| `[CollectChildren]` | Slice, loot card, zone cell gibi **iç hierarchy** | Editor collect → serialize |

Örnek: `WheelZoneProgressView` hem `[WheelBind]` (HUD event) hem `[CollectChildren]` (cell prefab referansları) kullanır.

---

## Dosyalar

| Dosya | Rol |
|-------|-----|
| `WheelViewScope.cs` | Canvas scope; discovery + inject/lifecycle döngüsü |
| `WheelViewContainer.cs` | Scope içi component registry (`Resolve` / `Find`) |
| `WheelInjector.cs` | Field inject + lifecycle method invoke |
| `WheelBindAttribute.cs` | Class marker |
| `WheelInjectAttribute.cs` | Field marker |
| `WheelAfterInjectAttribute.cs` | Post-inject hook |
| `WheelBeforeUnbindAttribute.cs` | Pre-unbind hook |

---

## Sık hatalar

**Scope'ta bindable sayısı 0**  
→ Script'te `[WheelBind]` yok veya GameObject scope dışında.

**`No view binding for X`**  
→ `[WheelInject]` required ama scope'ta o component yok. Aynı `WheelViewScope` altına component ekle veya dependency'yi aggregate binding ile tek tipe indir.

**`Multiple view bindings for X`**  
→ Aynı scope'ta aynı tipten birden fazla component var ve biri `Resolve` bekliyor. Unique concrete type kullan, aggregate binding ekle veya bu dependency'yi kaldır.

**Event gelmiyor**  
→ `[WheelAfterInject]` içinde subscribe unutulmuş veya `[WheelBeforeUnbind]` erken unsubscribe ediyor.

## Özet kural

> **View kendini tanır (`[WheelBind]`), ihtiyacını söyler (`[WheelInject]`), bağlanmayı lifecycle method'larında yapar. Scope sadece orchestrator'dır; business logic veya referans listesi taşımaz.**
