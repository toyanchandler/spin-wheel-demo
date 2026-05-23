# UIParticle Prefab

This folder is the shared UI particle setup for the wheel case.

## Assets

- `UIParticleRenderTexture.renderTexture`
  - Size: `512 x 512`
  - Depth Buffer: `16-bit`
  - Anti Aliasing: `1x`
  - Format: `ARGB32`
- `UIParticle_Additive.mat`
  - Particle material used by the world Particle System.
- `UIParticle_Display.shader`
  - UI display shader that cuts the black RenderTexture background into alpha.
- `UIParticle_Display.mat`
  - RawImage material.
  - `Main Texture` is `UIParticleRenderTexture`.
  - `Black Cutoff` and `Alpha Softness` control the black-background alpha cut.
- `UIParticlePrefab.prefab`
  - Contains only the world capture rig: `UIParticleCamera` and `UIParticleRewardBurst`.

## Flow

```text
Particle sprite
        -> world Particle System on UIParticle layer
        -> UIParticleCamera sees only UIParticle layer
        -> UIParticleRenderTexture
        -> RawImage.texture + UIParticle_Display material
        -> Canvas UI
```

## Reward Popup Usage

The scene builder creates one scene-authored instance for the reward popup:

```text
ui_outcome_popup_controller
    UIParticleWorldRig
        UIParticleCamera
        UIParticleRewardBurst

ui_outcome_overlay
    ui_reward_burst_render_texture
```

`UIParticleRewardBurst` is not a UI object and is not rendered by the Canvas. It stays in the world capture rig. `UIParticleCamera` renders that world particle into `UIParticleRenderTexture`. The popup RawImage only displays that RenderTexture through `UIParticle_Display.mat`.

## Required Connections

```text
UIParticleCamera.targetTexture = UIParticleRenderTexture
UIParticleCamera.cullingMask = UIParticle
UIParticleRewardBurst.layer = UIParticle
ui_reward_burst_render_texture.texture = UIParticleRenderTexture
ui_reward_burst_render_texture.material = UIParticle_Display
```

The camera clears to black with alpha `0`. If the RenderTexture background leaves dark edges, tune these material values:

```text
UIParticle_Display.Black Cutoff
UIParticle_Display.Alpha Softness
```

## Changing The Effect

1. Open `UIParticlePrefab.prefab`.
2. Select `UIParticleRewardBurst`.
3. Change Particle System modules such as Emission, Shape, Color over Lifetime, Size over Lifetime, or Renderer material.
4. Keep these connections unchanged:
   - `UIParticleCamera.targetTexture = UIParticleRenderTexture`
   - `UIParticleCamera.cullingMask = UIParticle`
   - Particle objects stay on the `UIParticle` layer

For a larger premium effect, duplicate the RenderTexture and use `1024 x 1024`. For small reward bursts, keep `512 x 512`.

## Batching Notes

The UI receives one RawImage with one material, so the Canvas only pays for that single UI graphic while the particles render offscreen. The particle camera is disabled until the reward popup plays the burst.
