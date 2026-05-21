# UX Flow and UI Rules

This document is the target UX/UI contract for the Vertigo wheel case. It is based on the provided demo brief, the local reference captures in `screenshots/ui reference visuals`, and the current Unity project structure under `WheelOfFortuneVertigo`.

The goal is not only to make the game functional. The final result must visibly use the supplied atlas content, feel alive through DOTween/TMP animation, and communicate the risk loop clearly at every moment: spin to grow rewards, leave at safe/super opportunities, or lose everything on bomb.

## Source References

Primary visual references:

- `screenshots/ui reference visuals/selected/01_start_wheel_idle.png`
- `screenshots/ui reference visuals/selected/02_middle_run_wheel_state.png`
- `screenshots/ui reference visuals/selected/03_after_exit_confirmation.png`
- `screenshots/ui reference visuals/selected/04_after_collect_card_opening_rewards.png`
- `screenshots/ui reference visuals/contact/gameplay_video_contact_sheet.jpg`
- `screenshots/ui reference visuals/source/GameplayVideo.mp4`

Implementation references already present in code:

- `WheelGamePhase`: `Ready`, `Spinning`, `Won`, `Bombed`, `CashedOut`
- `ZoneType`: `Standard`, `Safe`, `Super`
- `WheelGameSettings`: 8 slices, safe interval 5, super interval 30
- `WheelSkinCatalog`: bronze, silver, golden wheel skins
- `WheelUiCopyCatalog`: zone labels, phase copy, outcome copy
- `WheelRewardPanelView`, `WheelZoneProgressView`, `WheelMilestoneBadgesView`, `WheelRewardOpeningView`, `WheelOutcomePopupView`

## Core Loop

1. Player enters Zone 1.
2. UI shows the current zone, upcoming zone path, next safe/super milestones, empty reward strip, and a bronze standard wheel.
3. Player presses `SPIN`.
4. Wheel spins and resolves one slice.
5. If the slice is a reward:
   - reward flies/pops from wheel to the left loot strip,
   - reward is added to inventory,
   - the game advances to the next zone,
   - wheel skin/content updates for the new zone,
   - player can spin again.
6. If the slice is a bomb:
   - bomb pop-in opens,
   - all collected rewards are visually burned/cleared,
   - player can restart.
7. If the player is in a Safe or Super zone and not spinning:
   - `EXIT` is available,
   - pressing it opens confirmation,
   - collecting rewards moves to the full reward opening screen.
8. In reward opening:
   - all collected rewards are displayed as cards,
   - cards animate open in order,
   - player can restart with `PLAY AGAIN`.

## Zone Rules

Zone type is determined by interval priority:

- `Super`: every 30th zone, e.g. 30, 60, 90. Super overrides Safe because 30 is also divisible by 5.
- `Safe`: every 5th zone that is not Super, e.g. 5, 10, 15, 20, 25, 35.
- `Standard`: every other zone.

### Standard Zone

Purpose: risk state.

- Wheel skin: bronze.
- Bomb: included.
- Leave: not allowed by game rule, even if a button exists visually. The `EXIT` button should be disabled/greyed or visually present but non-interactive.
- Rewards: ordinary weapon/point rewards, increasing in value as zone number grows.
- Tone: risky, darker, orange/bronze accents.

### Safe Zone

Purpose: checkpoint state.

- Wheel skin: silver.
- Bomb: not included.
- Leave: allowed.
- Rewards: better than standard, but not as rare as super.
- Tone: cleaner silver/cyan highlight, more confident, less danger.
- UI must explicitly show that this is risk-free.

### Super Zone

Purpose: premium checkpoint state.

- Wheel skin: golden.
- Bomb: not included.
- Leave: allowed.
- Rewards: highest-value and special rewards.
- Tone: gold/green glow, strongest reward celebration.
- Super visual weight must be greater than Safe. If both conditions match, Super always wins.

## Screen Layout Contract

The game is landscape-first and must remain stable in 20:9, 16:9, and 4:3.

### Left Loot Strip

Reference: the vertical panel on the left side in the gameplay captures.

Rules:

- Position: left anchored, vertically centered.
- Contains:
  - `EXIT` button at the top,
  - collected reward rows stacked downward,
  - each row has icon, amount, and optional tier glow.
- It must not stretch reward icons. Icons keep aspect ratio.
- It must have a framed card/panel look using the supplied card frame assets.
- Reward rows must not appear silently. Every new reward must animate into this strip from the wheel result area.
- When bomb hits, the strip should shake, flash red, then clear.
- When cashing out, strip should dim behind the confirmation/reward opening flow, not disappear instantly.

### Center Wheel

Reference: the wheel is the strongest visual object in all gameplay frames.

Rules:

- Position: slightly right of center to leave space for the left loot strip.
- Wheel base changes by zone type:
  - Standard: `ui_spin_bronze_base`
  - Safe: `ui_spin_silver_base`
  - Super: `ui_spin_golden_base`
- Indicator changes with wheel base:
  - Standard: `ui_spin_bronze_indicator`
  - Safe: `ui_spin_silver_indicator`
  - Super: `ui_spin_golden_indicator`
- Center button uses `ui_spin_generic_button`.
- Slice content must change per zone from editable reward pools.
- The bomb slice appears only in Standard zones.
- The wheel must have idle motion: subtle breathing scale or slow rim shimmer.
- During spin:
  - center button is disabled,
  - wheel rotates with acceleration and deceleration,
  - indicator ticks or bumps as slices pass,
  - selected slice receives a final pulse.

### Top Zone Progress Row

Reference: small boxes across the top, with current zone highlighted.

Rules:

- Position: top center.
- Shows a horizontal window around the current zone, not the entire run.
- Current zone is larger/brighter.
- Standard future zones use neutral/dark frame.
- Safe zones use cyan/silver frame.
- Super zones use green/gold frame.
- Zone numbers must be TMP, legible, and centered.
- Current cell must tween when advancing:
  - old current cell scales down,
  - new current cell scales up,
  - row slides if the current zone would otherwise leave the middle.
- Safe and Super cells must glow before the player reaches them.
- At 20:9 the row may show more cells; at 4:3 it should show fewer cells or reduce gaps, never overlap the right milestone badges.

### Right Milestone Badges

Reference: stacked green/blue milestone boxes on the right.

Rules:

- Position: upper-right, aligned to the wheel vertical range.
- Always shows:
  - next Super zone,
  - next Safe zone.
- Super badge has higher priority and should be visually above Safe.
- Text format:
  - `SUPER\nZONE\n30`
  - `SAFE\nZONE\n25`
- The next milestone value must update immediately after zone advance.
- When the current zone is Safe or Super, its related badge should pulse.
- Use zone panel assets, not plain colored rectangles.

### Outcome Popup

Reference: bomb overlay and reward pop-in frames.

Rules:

- Reward result popup appears after each successful spin.
- Bomb popup appears after bomb hit and blocks spin/leave.
- Popup root should animate separately from static layout roots.
- Popup animation:
  - canvas fade in,
  - content root scale from 0.9 to 1,
  - icon pops with overshoot,
  - title TMP letter spacing or alpha animates quickly.
- Reward popup should be brief and non-blocking enough to keep flow fast.
- Bomb popup should feel heavier:
  - red/dark overlay,
  - bomb/death icon grows,
  - left loot strip shakes,
  - rewards clear after the impact beat.

### Exit Confirmation

Reference: confirmation modal before reward collection.

Rules:

- Trigger: only when player presses `EXIT` in Safe or Super zone, and wheel is not spinning.
- Modal copy:
  - Title: `COLLECT REWARDS?`
  - Body: `Are you sure you want to go out and collect your rewards? We have saved the best rewards for last!`
- Buttons:
  - primary orange: `COLLECT REWARDS`
  - secondary grey: `COME BACK`
- Background dims gameplay.
- Wheel and loot strip stay visible but de-emphasized.
- `COME BACK` closes modal and returns to same zone.
- `COLLECT REWARDS` transitions to reward opening screen.

### Reward Opening Screen

Reference: blue full-screen `YOUR REWARDS:` screen.

Rules:

- Trigger: after successful cashout.
- It is a separate full-screen presentation, not just a small popup.
- Background: deep blue panel with subtle vignette and glow.
- Title: `YOUR REWARDS:` with TMP uppercase, outline, and light shine pass.
- Cards appear horizontally if there is room; wrap or page for narrow 4:3.
- Each reward card:
  - uses a card frame,
  - shows reward name at top,
  - icon in center,
  - amount at bottom,
  - tier glow behind icon,
  - optional shine sweep for rare/super rewards.
- Card opening animation:
  - cards start as dim silhouettes or closed backs,
  - flip/scale open left to right,
  - icon pops after card frame lands,
  - amount counter fades in last.
- `PLAY AGAIN` button uses orange button asset and restarts.

## Reward and Asset Usage Model

No supplied 2D asset should remain unused in the final implementation. Every atlas sprite must be assigned to one of these roles:

- wheel skin,
- wheel indicator,
- button,
- zone/progress panel,
- reward icon,
- reward card/chest,
- effect/glow/shine,
- bomb/death outcome,
- alternate card/frame state.

Reward pools must be editor-editable through `WheelGameSettings`; the UI must not hardcode the final reward set in a view class.

### Reward Pool Policy

Standard rewards:

- Use ordinary point icons and tier 1/2 weapon renders.
- Amounts can be low to medium.
- Bomb is included.
- Suggested examples: pistol points, armor points, knife points, SMG points, shotgun points, tier1 shotgun, tier2 rifle.

Safe rewards:

- Use better point amounts, silver chest, standard chest, special but not top-tier weapons.
- Bomb is not included.
- Suggested examples: silver chest, standard chest, tier2 mle, tier3 smg, grenade M26/M67, healthshot neurostim.

Super rewards:

- Use gold/super/big chests, limited/event items, high tier weapons, golden/cash icons.
- Bomb is not included.
- Suggested examples: super chest, gold chest, big chest, aviator glasses, baseball cap, pumpkin helmet, bayonet variants, tier3 sniper, tier3 shotgun.

Duplicate/variant assets must be used intentionally:

- `UI_Icons_Pistol_Points` and `UI_Icons_Pistol_Points_` should not both represent the exact same outcome in the same pool. Use one as normal pistol points and the other as alternate/high amount pistol points or reward opening card variant.
- `UI_Icons_SMG_Points` and `UI_Icons_Submachine_Points` should be separated as SMG points vs submachine bonus points.
- Chest size/material variants should map to progression value, not random decoration.

## Animation Requirements

Every major state change must have feedback. Static UI is not enough.

### Global Motion Rules

- Use DOTween for UI motion, alpha, scale, shake, punch, and sequence timing.
- Do not use Unity inspector `OnClick` or animator references for core flow.
- UI animators/tween targets should be child transforms, not the root layout transform.
- Tween duration should be short and responsive:
  - small hover/pulse: 0.12-0.2s,
  - popup open: 0.18-0.3s,
  - card reveal: 0.25-0.45s per card,
  - bomb impact: 0.35-0.6s.
- Kill or reuse tweens on disable/unbind to avoid stale sequences.
- All animations must use `SetUpdate(true)` for UI popups if gameplay time scale changes later.

### Idle State

- Wheel base has a very subtle breathing scale.
- Center spin button has a soft blue pulse.
- Current zone cell in top row has a glow pulse.
- Next Safe/Super milestone badges have a low alpha shimmer.
- Reward strip cards idle quietly; only rare/super cards may shimmer.

### Spin State

- Button locks and visually depresses.
- Wheel accelerates fast, spins, then decelerates to selected slice.
- Indicator bumps/ticks per slice.
- Top row and side badges remain visible but not distracting.
- Text status uses TMP alpha pulse: `SPINNING...`.

### Reward Win State

- Selected slice pulses.
- Result icon pops above wheel.
- Reward flies from wheel to the left loot strip.
- Loot row inserts with scale from 0.85 to 1 and fade from 0 to 1.
- Amount label counts up if stack amount changes.
- Zone row advances after the reward lands.

### Bomb State

- Wheel stops on bomb slice.
- Bomb/death icon expands.
- Screen overlay flashes red/dark.
- Left loot strip shakes.
- Collected cards dim and vanish after impact.
- Popup text should read as a strong failure state, not a generic message.
- Restart button appears only after the impact beat.

### Cashout State

- Confirmation modal opens with dimmed gameplay.
- If confirmed, rewards move from left strip into reward opening screen.
- Cards reveal one by one.
- Final `PLAY AGAIN` button appears after the last card lands.

## TMP Text Rules

All visible text must use TextMeshPro.

Text should not remain default/plain. Required treatment:

- zone numbers: bold, centered, outline/shadow, strong contrast,
- zone labels: uppercase, colored by zone type,
- button labels: uppercase, small outline/shadow, no wrapping,
- reward names: compact, readable, preferably max two lines,
- amounts: numeric badge style, e.g. `x4`, `x25`,
- popup titles: large uppercase with outline and glow,
- reward opening title: largest TMP title on that screen.

Avoid default placeholder copy. The final copy should directly explain game state:

- Standard ready: `RISK ZONE`
- Safe ready: `SAFE ZONE - NO BOMB`
- Super ready: `SUPER ZONE - PREMIUM SPIN`
- Spinning: `SPINNING...`
- Win: `ADDED TO REWARDS`
- Bomb: `BOMB EXPLODED`
- Cashout: `REWARDS SECURED`

## Sprite and Slicing Rules

- Button sprites and panel/card frame sprites must be imported as sliced sprites when used as scalable UI backgrounds.
- Do not stretch non-frame images such as reward icons, chests, wheel bases, indicators, or VFX sprites.
- Use `Preserve Aspect` for reward/chest/weapon icons.
- Disable `Raycast Target` on decorative images.
- Enable raycast only on actual buttons and modal blocking overlays.
- Keep atlas `Include In Build` enabled.
- All supplied sprites must remain packable through `ui_demo_content.spriteatlas`.

## Responsive Layout Rules

20:9:

- Wider zone progress row allowed.
- Reward opening can show more cards in one row.
- Right milestone badges can sit farther right.

16:9:

- Baseline target.
- Match reference captures most closely.
- Wheel should stay dominant and centered-right.

4:3:

- Reduce top progress visible cell count or gap.
- Keep left loot strip narrower.
- Reward opening should wrap cards or allow paged rows.
- Never let top progress overlap milestone badges.
- Never let reward cards cover `PLAY AGAIN`.

## State-by-State UI Checklist

Ready:

- correct wheel skin for current zone,
- current zone highlighted in top row,
- next Safe/Super badges visible,
- collected rewards visible on left,
- spin button enabled,
- exit enabled only in Safe/Super.

Spinning:

- spin button disabled,
- leave disabled,
- wheel/indicator animating,
- no modal open,
- current reward strip remains visible.

Won:

- reward popup appears,
- reward flies to loot strip,
- inventory row updates,
- zone increments,
- next zone skin/content applies.

Bombed:

- bomb popup appears,
- inventory clears,
- zone does not advance,
- restart is available,
- spin/leave remain disabled.

CashedOut:

- reward opening screen appears,
- reward cards reveal,
- play again restarts cleanly.

## Atlas Coverage Table

Every row below must be represented in the final UI or reward data. `Current target usage` describes where the asset should live. If an asset is not visible in the current build, this table is the implementation backlog.

| Asset | Type | Current target usage | Trigger/state | Required motion/effect |
|---|---|---|---|---|
| `ui_spin_bronze_base.png` | Wheel base | Standard zone wheel | Zones not divisible by 5 or 30 | Idle breath, spin rotation |
| `ui_spin_bronze_indicator.png` | Wheel pointer | Standard zone indicator | Standard zones | Tick bump during spin |
| `ui_spin_silver_base.png` | Wheel base | Safe zone wheel | Every 5th non-super zone | Silver glow, spin rotation |
| `ui_spin_silver_indicator.png` | Wheel pointer | Safe zone indicator | Safe zones | Tick bump, no bomb emphasis |
| `ui_spin_golden_base.png` | Wheel base | Super zone wheel | Every 30th zone | Strong gold glow, premium idle pulse |
| `ui_spin_golden_indicator.png` | Wheel pointer | Super zone indicator | Super zones | Gold tick bump |
| `ui_spin_generic_button.png` | Button center | Center `SPIN` button | Ready and spinning | Pulse idle, press scale, disabled dim |
| `UI_button_orange_standard.png` | Button frame | Primary buttons: spin/collect/revive/play again | Main CTA states | Hover/pulse, press scale |
| `UI_button_grey_standard.png` | Button frame | Secondary buttons: exit/come back/give up/disabled | Secondary or disabled states | Subtle fade, disabled desaturate |
| `ui_card_zone_map_frame.png` | Frame | Top zone progress container/cells | Always | Current cell punch on zone advance |
| `ui_card_panel_zone_bg.png` | Panel | Neutral zone progress background | Standard future/past cells | Low alpha idle |
| `ui_card_panel_zone_coming.png` | Panel | Upcoming milestone cell | Future safe/super cells | Glow as milestone approaches |
| `ui_card_panel_zone_current.png` | Panel | Current standard zone cell | Standard current zone | Scale pulse |
| `ui_card_panel_zone_current_white.png` | Panel | Current safe zone cell | Safe current zone | Cyan/silver pulse |
| `ui_card_panel_zone_super.png` | Panel | Super zone cell/badge | Super current/upcoming zone | Strong glow sweep |
| `ui_card_panel_zone_white.png` | Panel | Safe zone non-current cell/badge | Safe upcoming zone | Soft cyan shimmer |
| `ui_card_frame_12px_neutral.png` | Sliced frame | Reward card frame and loot strip cards | Reward inventory/opening | Card pop/flip |
| `ui_card_frame_4px_zone.png` | Sliced frame | Small zone cells/badges | Top row/right badges | Current cell scale |
| `ui_card_frame_gardient.png` | Card backdrop | Rare/super reward card interior | Reward opening, premium cards | Shine sweep |
| `ui_card_icon_death.png` | Outcome icon | Bomb slice and bomb popup | Standard bomb hit | Impact scale, red flash |
| `star_flash_alpha.png` | VFX | Big reward/super/cashout flash | Super reward, card reveal, cashout | Quick radial flash |
| `star_glow_alpha.png` | VFX | Small icon glow behind rewards | Rare/super cards, milestone badges | Looping alpha pulse |
| `ui_vfx_offer_shine.tga` | VFX | Shine sweep over buttons/cards/chests | CTA hover, reward reveal | Move/rotate shine mask |
| `UI_icon_cash.png` | Reward icon | Cash/currency reward | Standard/safe reward and cashout summary | Count-up pulse |
| `UI_icon_gold.png` | Reward icon | Gold/premium currency reward | Safe/super reward | Golden glow, count-up |
| `UI_icon_chest_small_noligt.png` | Chest reward | Small chest | Early standard zones | Card reveal pop |
| `UI_icon_chest_standart_nolight.png` | Chest reward | Standard chest | Standard/safe mid zones | Card reveal pop |
| `UI_icon_chest_Bronze_nolight.png` | Chest reward | Bronze chest | Standard zones | Bronze glow |
| `UI_icon_chest_silver_nolight.png` | Chest reward | Silver chest | Safe zones | Silver glow |
| `UI_icon_chest_gold_nolight.png` | Chest reward | Gold chest | Super zones | Gold shine sweep |
| `UI_icon_chest_big_nolight.png` | Chest reward | Big chest | Super/late safe zones | Heavy pop and shine |
| `UI_icon_chest_super_nolight.png` | Chest reward | Super chest | Super zones only | Strong flash and card emphasis |
| `UI_Icons_Pistol_Points.png` | Point reward | Pistol points | Early standard reward | Fly to loot strip |
| `UI_Icons_Pistol_Points_.png` | Point reward variant | High amount pistol points/alternate card art | Later standard/safe reward | Count-up pulse |
| `UI_Icons_Shotgun_Points.png` | Point reward | Shotgun points | Standard/safe reward | Fly to loot strip |
| `UI_Icons_Rifle_Points.png` | Point reward | Rifle points | Standard/safe reward | Fly to loot strip |
| `UI_Icons_SMG_Points.png` | Point reward | SMG points | Standard reward | Fly to loot strip |
| `UI_Icons_Submachine_Points.png` | Point reward variant | Submachine bonus points | Safe reward | Fly to loot strip |
| `UI_Icons_Sniper_Points.png` | Point reward | Sniper points | Safe/super reward | Rare glow |
| `UI_Icons_Knife_Points.png` | Point reward | Knife points | Standard/safe reward | Fly to loot strip |
| `UI_Icons_Armor_Points.png` | Point reward | Armor points | Standard/safe reward | Shield-like pulse |
| `UI_Icons_Vest_Points.png` | Point reward | Vest points | Standard/safe reward | Shield-like pulse |
| `UI_Icon_Renders_tier1_shotgun.png` | Weapon render | Tier 1 shotgun reward card/slice | Standard zone reward | Card reveal, low glow |
| `UI_Icon_Renders_tier2_rifle.png` | Weapon render | Tier 2 rifle reward card/slice | Standard/safe reward | Card reveal, blue glow |
| `UI_Icon_Renders_tier2_mle.png` | Weapon render | Tier 2 melee reward card/slice | Safe reward | Card reveal, blue glow |
| `UI_Icon_Renders_tier3_shotgun.png` | Weapon render | Tier 3 shotgun reward card/slice | Super reward | Gold flash |
| `UI_Icon_Renders_tier3_smg.png` | Weapon render | Tier 3 SMG reward card/slice | Safe/super reward | Rare glow |
| `UI_Icon_Renders_tier3_sniper.png` | Weapon render | Tier 3 sniper reward card/slice | Super reward | Gold flash |
| `ui_icon_mle_bayonet_easter_time.png` | Event weapon render | Easter bayonet reward | Super/event reward | Event shine |
| `ui_icon_mle_bayonet_summer_vice.png` | Event weapon render | Summer bayonet reward | Super/event reward | Event shine |
| `ui_icon_aviator_glasses_easter.png` | Event cosmetic | Aviator glasses reward | Super/event reward | Star glow |
| `ui_icon_baseball_cap_easter.png` | Event cosmetic | Baseball cap reward | Super/event reward | Star glow |
| `ui_icon_helmet_pumpkin.png` | Event cosmetic | Pumpkin helmet reward | Super/event reward | Orange flash |
| `ui_icon_render_cons_grenade_m26.png` | Consumable reward | M26 grenade reward | Safe/super reward | Small impact pop |
| `ui_icon_render_cons_grenade_m67.png` | Consumable reward | M67 grenade reward | Safe/super reward | Small impact pop |
| `ui_icon_render_t_cons_molotov.png` | Consumable reward | Molotov reward | Safe/super reward | Warm glow |
| `ui_icon_render_cons_healthshot_2_neurostim.png` | Consumable reward | Neurostim healthshot reward | Safe reward | Medical pulse |
| `ui_icon_render_cons_healthshot_2_regenerator.png` | Consumable reward | Regenerator healthshot reward | Safe/super reward | Medical pulse |

## Final Acceptance Rules

- Every sprite listed in the atlas coverage table must be reachable in gameplay, reward opening, or UI state presentation.
- Standard/Safe/Super wheel skins must be visibly different and correctly triggered by zone rules.
- Safe and Super zones must never contain a bomb.
- Standard zones must contain exactly one bomb slice.
- The player must be able to leave only in Safe/Super zones when not spinning.
- All visible text must be TMP and styled; no default-looking text should remain.
- Reward gain, bomb loss, exit confirmation, and reward opening must all animate.
- 20:9, 16:9, and 4:3 layouts must be captured and checked before final delivery.
- Decorative images must not block raycasts.
- Buttons must be wired by code/event bus, not inspector `OnClick`.
- The scene must pass smoke checks after UI rule implementation.
