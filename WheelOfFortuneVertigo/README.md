Vertigo Games - Game Developer Demo

Case source:
- `Vertigo Games Game Developer Demo.pdf`
- `demo_content.zip` UI assets

Unity:
- Required by case: Unity 2021 LTS
- Project version target: 2021.3.45f1
- Android App Bundle target

Implemented:
- Wheel-of-fortune core loop with standard, safe, and super zones
- Bomb loss, retry, cash-out/exit, and continue-to-next-zone flow
- Leave/collect allowed only on safe or super zones while not spinning
- Gameplay config asset at `Assets/Config/WheelGameSettings.asset`
- Instance-scoped event bus between UI and gameplay
- Composition root controlled runtime lifecycle
- TextMeshPro UI and provided UI assets
- Pre-baked slice pool, no runtime child lookup or object destruction
- Split static, wheel, and HUD canvases with one HUD raycaster
- Android setup: API 35 target, IL2CPP, ARM64, App Bundle, landscape
- Sprite Atlas and shared UI material for first-pass batching
- DOTween initialized with recyclable tween policy and fixed capacity

Editor tools:
- `Vertigo Case/Apply Android Case Setup`
- `Vertigo Case/Ensure Project Assets`
- `Vertigo Case/Wheel Designer`
- `Vertigo Case/Play Commands/Spin`
- `Vertigo Case/Play Commands/Retry`
- `Vertigo Case/Play Commands/Exit`

Validation:
- MainScene is the source of truth for scene-authored UI layout.

Android note:
- Google Play submission currently requires Android 15 / API 35 or higher for new apps and updates.
- Unity 2021.3 can be configured for API 35, IL2CPP, ARM64, and AAB here. Full 16 KB page-size compliance depends on the installed Android SDK/NDK and Unity patch support, so a real release build should still be checked with Android tooling before store upload.
