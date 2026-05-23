#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Diagnostics;
using Vertigo.Wheel.Runtime;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    public static class VertigoCaseSmokeChecks
    {
        [MenuItem("Vertigo Case/Run Smoke Checks")]
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");

            WheelGameConfigSource configSource = UnityEngine.Object.FindObjectOfType<WheelGameConfigSource>();
            WheelGameSettings settings = configSource != null ? configSource.Settings : null;
            if (settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<WheelGameSettings>(VertigoWheelPaths.GameSettingsPath);
            }

            WheelGameState state = UnityEngine.Object.FindObjectOfType<WheelGameState>();
            WheelStatePublisher publisher = UnityEngine.Object.FindObjectOfType<WheelStatePublisher>();
            WheelGameFlowController flowController = UnityEngine.Object.FindObjectOfType<WheelGameFlowController>();
            WheelRuntimeCompositionRoot compositionRoot = UnityEngine.Object.FindObjectOfType<WheelRuntimeCompositionRoot>();
            WheelSpinner spinner = UnityEngine.Object.FindObjectOfType<WheelSpinner>();
            WheelView view = UnityEngine.Object.FindObjectOfType<WheelView>();
            WheelPerformanceMonitor performanceMonitor = UnityEngine.Object.FindObjectOfType<WheelPerformanceMonitor>();
            WheelPerformanceOverlay performanceOverlay = UnityEngine.Object.FindObjectOfType<WheelPerformanceOverlay>();
            CanvasScaler[] scalers = UnityEngine.Object.FindObjectsOfType<CanvasScaler>();

            Require(settings != null, "WheelGameSettings asset exists");
            Require(configSource != null, "WheelGameConfigSource exists on game_wheel_runtime");
            RequireObjectReference(configSource, "_settings", "WheelGameConfigSource references WheelGameSettings asset");
            Require(GameObject.Find("game_wheel_settings") == null, "legacy game_wheel_settings scene object is removed");
            WheelRuntimeLocator.RegisterSettings(settings);
            Require(state != null, "WheelGameState component exists");
            Require(publisher != null, "WheelStatePublisher component exists");
            Require(flowController != null, "WheelGameFlowController component exists");
            Require(compositionRoot != null, "WheelRuntimeCompositionRoot exists");
            Require(spinner != null, "WheelSpinner component exists");
            WheelRuntimeLocator.RegisterSpinner(spinner);
            Require(view != null, "WheelView component exists");
            Require(performanceMonitor != null, "WheelPerformanceMonitor exists");
            Require(performanceOverlay != null, "WheelPerformanceOverlay exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelSpinButtonAction>() != null, "Spin button action exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelLeaveButtonAction>() != null, "Leave button action exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelRestartButtonAction>() != null, "Restart button action exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelZoneTextView>() != null, "Zone text view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelZoneTypeTextView>() != null, "Zone type text view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelZonePanelView>() != null, "Zone panel view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelZoneProgressView>() != null, "Zone progress row view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelMilestoneBadgesView>() != null, "Safe and super milestone badge view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelRewardPanelView>() != null, "Loot panel view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelExitConfirmationView>(true) != null, "Exit confirmation view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelRewardOpeningView>(true) != null, "Reward opening view exists");
            Require(GameObject.Find("ui_zone_panel_root") != null, "ui_zone_panel_root exists in HUD hierarchy");
            Require(GameObject.Find("ui_zone_progress_root") != null, "ui_zone_progress_root exists in HUD hierarchy");
            Require(GameObject.Find("ui_zone_milestone_badges_root") != null, "ui_zone_milestone_badges_root exists in HUD hierarchy");
            Require(GameObject.Find("ui_loot_strip_root") != null, "ui_loot_strip_root exists in HUD hierarchy");
            settings.InitializeRuntime();
            RequireZoneAndLootPresentation(settings);
            RequireUxFlowAssetCoverage(settings);
            Require(UnityEngine.Object.FindObjectOfType<WheelOutcomePopupView>(true) != null, "Outcome popup view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelStatusTextView>() != null, "Status text view exists");
            Require(UnityEngine.Object.FindObjectOfType<WheelBackgroundView>() != null, "Background view exists");
            Require(UnityEngine.Object.FindObjectsOfType<WheelSkinView>().Length == 2, "Scene has wheel base and indicator skin views");
            Require(UnityEngine.Object.FindObjectsOfType<WheelSpinner>().Length == 1, "Scene has exactly one WheelSpinner");
            Require(UnityEngine.Object.FindObjectsOfType<Canvas>().Length == 4, "Scene has static, wheel, HUD, and debug canvases");
            Require(UnityEngine.Object.FindObjectsOfType<GraphicRaycaster>().Length == 1, "Only HUD canvas has a GraphicRaycaster");
            RequireDebugCanvas();
            RequireNoLegacyRewardPanelObjects();
            RequireWheelRewardsUseConfiguredIcons(settings);
            RequireHudWheelVerticalSeparation(settings);
            RequireRaycastTargetWhitelist();
            Require(scalers.Length == 4, "Scene has four CanvasScaler components");
            for (int i = 0; i < scalers.Length; i++)
            {
                Require(scalers[i].screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight, "CanvasScaler " + i + " uses balanced screen match mode");
                Require(Mathf.Approximately(scalers[i].matchWidthOrHeight, 1f), "CanvasScaler " + i + " scales from landscape height");
                Require(scalers[i].referenceResolution == settings.Layout.ReferenceResolution, "CanvasScaler " + i + " uses configured reference resolution");
            }
            RequireObjectReference(settings, "_uiCopy", "WheelGameSettings has serialized UI copy catalog");
            RequireObjectReference(settings, "_skinCatalog", "WheelGameSettings has serialized skin catalog");
            RequireObjectReference(settings, "_sliceLayoutCatalog", "WheelGameSettings has serialized slice layout catalog");
            RequireObjectReference(settings, "_outcomePopupMotionCatalog", "WheelGameSettings has serialized outcome popup motion catalog");
            RequireObjectReference(settings, "_spinResolveCatalog", "WheelGameSettings has serialized spin resolve catalog");
            Require(WheelRuntimeLocator.Settings != null, "WheelRuntimeLocator has settings");
            Require(WheelRuntimeLocator.Spinner != null, "WheelRuntimeLocator has spinner on wheel hierarchy");
            RequireHostReferences();
            RequireOutcomePopupReferences();
            RequireExitConfirmationReferences();
            RequireRewardOpeningReferences();
            RequireCompositionRootHasNoSerializedFields(compositionRoot);
            WheelHierarchyWiringValidator.ValidateScene();
            RequireObjectReference(view, "_slicePoolRoot", "WheelView references slice pool root");
            SerializedProperty sliceViews = new SerializedObject(view).FindProperty("_sliceViews");
            Require(sliceViews != null && sliceViews.isArray, "WheelView has collected slice view array");
            Require(sliceViews.arraySize == settings.SliceCount, "WheelView slice view count matches settings");
            WheelSliceDefinition[] slices = CreateSliceBuffer(settings.SliceCount);
            state.InitializeRuntime();
            Require(settings.GetZoneType(1) == ZoneType.Standard, "Zone 1 is standard");
            Require(settings.GetZoneType(5) == ZoneType.Safe, "Zone 5 is safe");
            Require(settings.GetZoneType(30) == ZoneType.Super, "Zone 30 is super");
            state.Restart();
            WheelHudSnapshot hudSnapshot = WheelSnapshotFactory.CreateHud(state);
            Require(hudSnapshot.ZonePanelSprite != null, "HUD snapshot includes zone panel sprite");
            Require(hudSnapshot.RewardCardFrameSprite != null, "HUD snapshot includes loot card frame sprite");
            WheelZoneSnapshot zoneSnapshot = WheelSnapshotFactory.CreateZone(state);
            Require(zoneSnapshot.SlicePositions != null && zoneSnapshot.SlicePositions.Length == state.SliceCount, "Zone snapshot includes precomputed slice positions");
            WheelOutcomeSnapshot outcomeSnapshot = WheelSnapshotFactory.CreateOutcome(state.Phase, state.LastResult, state.HasLastResult, state.Inventory, settings);
            Require(outcomeSnapshot.Motion.FadeDuration > 0f, "Outcome snapshot includes popup motion from catalog");
            Require(WheelSliceQueries.ContainsBomb(slices, settings.FillSlicesForZone(1, slices)), "Standard zone contains a bomb");
            Require(!WheelSliceQueries.ContainsBomb(slices, settings.FillSlicesForZone(5, slices)), "Safe zone has no bomb");
            Require(!WheelSliceQueries.ContainsBomb(slices, settings.FillSlicesForZone(30, slices)), "Super zone has no bomb");
            compositionRoot.StartRuntime();
            compositionRoot.RequestRestart();
            compositionRoot.RequestLeave();
            compositionRoot.StopRuntime();
            RequirePerformanceMonitorLifecycle(performanceMonitor);
            Require(DOTween.TotalActiveTweens() == 0, "DOTween has no active tweens after runtime stop");
            Require(typeof(DOTween).Assembly != null, "DOTween is available");
            Require(!TypeExists("Vertigo.Wheel.Runtime.WheelGameController"), "WheelGameController class is removed");
            Require(!TypeExists("Vertigo.Wheel.Runtime.WheelGameEvents"), "Static WheelGameEvents class is removed");
            Require(!DeclaresUpdate(typeof(WheelGameState)), "WheelGameState has no Update loop");
            Require(!DeclaresUpdate(typeof(WheelGameFlowController)), "WheelGameFlowController has no Update loop");
            Require(!TypeExists("Vertigo.Wheel.Runtime.WheelSpinRequestHandler"), "WheelSpinRequestHandler class is removed");
            Require(!TypeExists("Vertigo.Wheel.Runtime.IWheelRuntimeComponent"), "IWheelRuntimeComponent registry is removed");
            Require(!TypeExists("Vertigo.Wheel.Runtime.WheelRuntimeUnityLifecycle"), "WheelRuntimeUnityLifecycle class is removed");
            Require(!DeclaresUpdate(typeof(WheelSpinner)), "WheelSpinner has no Update loop");
            Require(!DeclaresUpdate(typeof(WheelView)), "WheelView has no Update loop");
            RequireRuntimeScriptsAvoidForbiddenApis();
            RequireRuntimeUiAvoidAllocationTextPatterns();

            Debug.Log("Vertigo case smoke checks passed.");
        }

        private static void RequireObjectReference(UnityEngine.Object target, string propertyName, string message)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Require(property != null && property.objectReferenceValue != null, message);
        }

        private static void RequireObjectArray(UnityEngine.Object target, string propertyName, int expectedSize, string message)
        {
            var serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            Require(property != null && property.isArray && property.arraySize == expectedSize, message + " size");

            for (int i = 0; i < expectedSize; i++)
            {
                Require(property.GetArrayElementAtIndex(i).objectReferenceValue != null, message + " element " + i);
            }
        }

        private static void RequireCompositionRootHasNoSerializedFields(WheelRuntimeCompositionRoot compositionRoot)
        {
            var serializedObject = new SerializedObject(compositionRoot);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool entered = iterator.NextVisible(true);
            int serializedFieldCount = 0;
            while (entered)
            {
                if (iterator.name != "m_Script")
                {
                    serializedFieldCount++;
                }

                entered = iterator.NextVisible(false);
            }

            Require(serializedFieldCount == 0, "WheelRuntimeCompositionRoot has no serialized cross-hierarchy fields");
        }

        private static void RequireOutcomePopupReferences()
        {
            WheelOutcomePopupView popup = UnityEngine.Object.FindObjectOfType<WheelOutcomePopupView>(true);
            Require(popup != null, "WheelOutcomePopupView exists including inactive overlay");
            RequireObjectReference(popup, "_root", "WheelOutcomePopupView has serialized root");
            RequireObjectReference(popup, "_iconImage", "WheelOutcomePopupView has serialized icon");
            RequireObjectReference(popup, "_titleText", "WheelOutcomePopupView has serialized title");
            RequireObjectReference(popup, "_resultText", "WheelOutcomePopupView has serialized result");
            RequireObjectReference(popup, "_summaryText", "WheelOutcomePopupView has serialized summary");
            RequireObjectReference(popup, "_rewardChromeGroup", "WheelOutcomePopupView has serialized reward chrome group");
            RequireObjectArray(popup, "_flightIconPool", 1, "WheelOutcomePopupView uses one pooled reward flight icon");
        }

        private static void RequireRewardOpeningReferences()
        {
            WheelRewardOpeningView opening = UnityEngine.Object.FindObjectOfType<WheelRewardOpeningView>(true);
            Require(opening != null, "WheelRewardOpeningView exists including inactive overlay");
            RequireObjectReference(opening, "_root", "WheelRewardOpeningView has serialized root");
            RequireObjectReference(opening, "_titleText", "WheelRewardOpeningView has serialized title");
            RequireObjectReference(opening, "_cardFrameSource", "WheelRewardOpeningView has serialized card frame source");
            RequireObjectReference(opening, "_cardPoolRoot", "WheelRewardOpeningView has serialized card pool root");
            RequireObjectArray(opening, "_rewardCards", 8, "WheelRewardOpeningView has serialized card pool");
        }

        private static void RequireRaycastTargetWhitelist()
        {
            Graphic[] graphics = UnityEngine.Object.FindObjectsOfType<Graphic>();
            int enabledCount = 0;
            for (int i = 0; i < graphics.Length; i++)
            {
                if (!graphics[i].raycastTarget)
                {
                    continue;
                }

                string objectName = graphics[i].gameObject.name;
                bool allowed = objectName == "ui_button_leave" || objectName == "ui_button_spin" || objectName == "ui_button_restart" || objectName == "ui_button_reward_opening_restart";
                Require(allowed, objectName + " is not allowed to receive raycasts");
                enabledCount++;
            }

            Require(enabledCount == 3, "Only three active button graphics receive raycasts");
        }

        private static void RequireDebugCanvas()
        {
            GameObject debugCanvas = GameObject.Find("debug_canvas");
            Require(debugCanvas != null, "debug_canvas exists");
            Require(debugCanvas.GetComponent<GraphicRaycaster>() == null, "debug_canvas has no GraphicRaycaster");
            Canvas canvas = debugCanvas.GetComponent<Canvas>();
            Require(canvas != null && canvas.sortingOrder > 20, "debug_canvas renders above HUD");
        }

        private static void RequireNoLegacyRewardPanelObjects()
        {
            Transform[] transforms = UnityEngine.Object.FindObjectsOfType<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                string objectName = transforms[i].name.ToLowerInvariant();
                Require(!objectName.Contains("reward_panel"), transforms[i].name + " legacy reward panel object is removed");
                Require(!objectName.Contains("reward_summary"), transforms[i].name + " legacy reward summary object is removed");
            }

            TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                Require(texts[i].text != "No rewards", texts[i].name + " does not render the old persistent No rewards panel");
            }
        }

        private static void RequireWheelRewardsUseConfiguredIcons(WheelGameSettings settings)
        {
            RequireRewardIcons(settings.StandardRewards, ZoneType.Standard);
            RequireRewardIcons(settings.SafeRewards, ZoneType.Safe);
            RequireRewardIcons(settings.SuperRewards, ZoneType.Super);
            Require(settings.BombReward.Icon != null, "Bomb reward icon exists");
            RequireWheelSlicesUseConfiguredIcons(settings, 1);
            RequireWheelSlicesUseConfiguredIcons(settings, 5);
            RequireWheelSlicesUseConfiguredIcons(settings, 30);
        }

        private static void RequireRewardIcons(System.Collections.Generic.List<RewardDefinition> rewards, ZoneType zoneType)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                Require(rewards[i].Icon != null, zoneType + " reward icon exists");
            }
        }

        private static void RequireWheelSlicesUseConfiguredIcons(WheelGameSettings settings, int zone)
        {
            WheelSliceDefinition[] slices = CreateSliceBuffer(settings.SliceCount);
            int sliceCount = settings.FillSlicesForZone(zone, slices);
            for (int i = 0; i < sliceCount; i++)
            {
                WheelSliceDefinition slice = slices[i];
                Sprite expectedIcon = slice.IsBomb ? settings.BombReward.WheelIcon : slice.Reward.WheelIcon;
                Require(slice.DisplayIcon == expectedIcon, "Zone " + zone + " slice " + i + " uses the configured wheel reward icon");
                Require(new WheelSpinResult(i, slice).Icon == slice.Reward.WheelIcon, "Zone " + zone + " slice " + i + " result icon matches reward wheel icon");
                Require(slice.DisplayIcon == slice.Reward.WheelIcon, "Zone " + zone + " slice " + i + " display icon matches reward wheel icon");
            }
        }

        private static void RequireZoneAndLootPresentation(WheelGameSettings settings)
        {
            WheelUiCopyCatalog catalog = settings.UiCopy;
            Require(catalog != null, "UI copy catalog exists for zone presentation");
            WheelZoneUiCopy standardZone = catalog.GetZoneCopy(ZoneType.Standard);
            WheelZoneUiCopy safeZone = catalog.GetZoneCopy(ZoneType.Safe);
            WheelZoneUiCopy superZone = catalog.GetZoneCopy(ZoneType.Super);
            Require(standardZone.PanelSprite != null, "Standard zone panel sprite is assigned");
            Require(safeZone.PanelSprite != null, "Safe zone panel sprite is assigned");
            Require(superZone.PanelSprite != null, "Super zone panel sprite is assigned");
            Require(standardZone.MapFrameSprite != null, "Zone map frame sprite is assigned");

            WheelLayoutSettings layout = settings.Layout;
            Require(layout.RewardPanelFrameSprite != null, "Loot strip frame sprite is assigned");
            Require(layout.RewardCardFrameSprite != null, "Loot card frame sprite is assigned");

            WheelRewardPanelView lootPanel = UnityEngine.Object.FindObjectOfType<WheelRewardPanelView>();
            RequireObjectArray(lootPanel, "_cardViews", layout.MaxRewardCards, "WheelRewardPanelView has serialized loot card pool");
        }

        private static void RequireUxFlowAssetCoverage(WheelGameSettings settings)
        {
            var usedSpriteNames = new HashSet<string>();
            CollectSettingsSpriteNames(settings, usedSpriteNames);

            Image[] images = UnityEngine.Object.FindObjectsOfType<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].sprite != null)
                {
                    usedSpriteNames.Add(images[i].sprite.name);
                    string imageSpritePath = AssetDatabase.GetAssetPath(images[i].sprite);
                    if (!string.IsNullOrEmpty(imageSpritePath))
                    {
                        usedSpriteNames.Add(Path.GetFileNameWithoutExtension(imageSpritePath));
                    }
                }
            }

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                CollectSerializedSpriteNames(behaviours[i], usedSpriteNames);
            }

            for (int i = 0; i < RequiredUxFlowSprites.Length; i++)
            {
                string spriteName = Path.GetFileNameWithoutExtension(RequiredUxFlowSprites[i]);
                Require(usedSpriteNames.Contains(spriteName), RequiredUxFlowSprites[i] + " is assigned outside the atlas");
            }
        }

        private static void CollectSerializedSpriteNames(UnityEngine.Object target, HashSet<string> usedSpriteNames)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool entered = iterator.NextVisible(true);
            while (entered)
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue is Sprite sprite)
                {
                    AddSprite(usedSpriteNames, sprite);
                }

                entered = iterator.NextVisible(false);
            }
        }

        private static void CollectSettingsSpriteNames(WheelGameSettings settings, HashSet<string> usedSpriteNames)
        {
            AddSprite(usedSpriteNames, settings.BombReward.Icon);
            CollectRewardSpriteNames(settings.StandardRewards, usedSpriteNames);
            CollectRewardSpriteNames(settings.SafeRewards, usedSpriteNames);
            CollectRewardSpriteNames(settings.SuperRewards, usedSpriteNames);

            WheelLayoutSettings layout = settings.Layout;
            AddSprite(usedSpriteNames, layout.RewardPanelFrameSprite);
            AddSprite(usedSpriteNames, layout.RewardCardFrameSprite);
            AddSprite(usedSpriteNames, layout.ZoneStandardPanelSprite);
            AddSprite(usedSpriteNames, layout.ZoneUpcomingPanelSprite);
            AddSprite(usedSpriteNames, layout.ZoneSmallFrameSprite);
            AddSprite(usedSpriteNames, layout.StarFlashSprite);
            AddSprite(usedSpriteNames, layout.StarGlowSprite);
            AddSprite(usedSpriteNames, layout.ShineSprite);

            WheelUiCopyCatalog uiCopy = settings.UiCopy;
            AddZoneSprites(uiCopy.GetZoneCopy(ZoneType.Standard), usedSpriteNames);
            AddZoneSprites(uiCopy.GetZoneCopy(ZoneType.Safe), usedSpriteNames);
            AddZoneSprites(uiCopy.GetZoneCopy(ZoneType.Super), usedSpriteNames);

            WheelSkinCatalog skinCatalog = settings.SkinCatalog;
            AddSprite(usedSpriteNames, skinCatalog.GetWheelBase(WheelSkinTier.Bronze));
            AddSprite(usedSpriteNames, skinCatalog.GetIndicator(WheelSkinTier.Bronze));
            AddSprite(usedSpriteNames, skinCatalog.GetWheelBase(WheelSkinTier.Silver));
            AddSprite(usedSpriteNames, skinCatalog.GetIndicator(WheelSkinTier.Silver));
            AddSprite(usedSpriteNames, skinCatalog.GetWheelBase(WheelSkinTier.Golden));
            AddSprite(usedSpriteNames, skinCatalog.GetIndicator(WheelSkinTier.Golden));
        }

        private static void CollectRewardSpriteNames(List<RewardDefinition> rewards, HashSet<string> usedSpriteNames)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                AddSprite(usedSpriteNames, rewards[i].Icon);
            }
        }

        private static void AddZoneSprites(WheelZoneUiCopy zoneCopy, HashSet<string> usedSpriteNames)
        {
            AddSprite(usedSpriteNames, zoneCopy.PanelSprite);
            AddSprite(usedSpriteNames, zoneCopy.MapFrameSprite);
        }

        private static void AddSprite(HashSet<string> usedSpriteNames, Sprite sprite)
        {
            if (sprite != null)
            {
                usedSpriteNames.Add(sprite.name);
                string assetPath = AssetDatabase.GetAssetPath(sprite);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    usedSpriteNames.Add(Path.GetFileNameWithoutExtension(assetPath));
                }
            }
        }

        private static readonly string[] RequiredUxFlowSprites =
        {
            "ui_spin_bronze_base.png",
            "ui_spin_bronze_indicator.png",
            "ui_spin_silver_base.png",
            "ui_spin_silver_indicator.png",
            "ui_spin_golden_base.png",
            "ui_spin_golden_indicator.png",
            "ui_spin_generic_button.png",
            "UI_button_orange_standard.png",
            "UI_button_grey_standard.png",
            "ui_card_zone_map_frame.png",
            "ui_card_panel_zone_bg.png",
            "ui_card_panel_zone_coming.png",
            "ui_card_panel_zone_current.png",
            "ui_card_panel_zone_current_white.png",
            "ui_card_panel_zone_super.png",
            "ui_card_panel_zone_white.png",
            "ui_card_frame_12px_neutral.png",
            "ui_card_frame_4px_zone.png",
            "ui_card_frame_gardient.png",
            "ui_card_icon_death.png",
            "star_flash_alpha.png",
            "star_glow_alpha.png",
            "ui_vfx_offer_shine.tga",
            "UI_icon_cash.png",
            "UI_icon_gold.png",
            "UI_icon_chest_small_noligt.png",
            "UI_icon_chest_standart_nolight.png",
            "UI_icon_chest_Bronze_nolight.png",
            "UI_icon_chest_silver_nolight.png",
            "UI_icon_chest_gold_nolight.png",
            "UI_icon_chest_big_nolight.png",
            "UI_icon_chest_super_nolight.png",
            "UI_Icons_Pistol_Points.png",
            "UI_Icons_Pistol_Points_.png",
            "UI_Icons_Shotgun_Points.png",
            "UI_Icons_Rifle_Points.png",
            "UI_Icons_SMG_Points.png",
            "UI_Icons_Submachine_Points.png",
            "UI_Icons_Sniper_Points.png",
            "UI_Icons_Knife_Points.png",
            "UI_Icons_Armor_Points.png",
            "UI_Icons_Vest_Points.png",
            "UI_Icon_Renders_tier1_shotgun.png",
            "UI_Icon_Renders_tier2_rifle.png",
            "UI_Icon_Renders_tier2_mle.png",
            "UI_Icon_Renders_tier3_shotgun.png",
            "UI_Icon_Renders_tier3_smg.png",
            "UI_Icon_Renders_tier3_sniper.png",
            "ui_icon_mle_bayonet_easter_time.png",
            "ui_icon_mle_bayonet_summer_vice.png",
            "ui_icon_aviator_glasses_easter.png",
            "ui_icon_baseball_cap_easter.png",
            "ui_icon_helmet_pumpkin.png",
            "ui_icon_render_cons_grenade_m26.png",
            "ui_icon_render_cons_grenade_m67.png",
            "ui_icon_render_t_cons_molotov.png",
            "ui_icon_render_cons_healthshot_2_neurostim.png",
            "ui_icon_render_cons_healthshot_2_regenerator.png"
        };

        private static void RequireHudWheelVerticalSeparation(WheelGameSettings settings)
        {
            WheelLayoutSettings layout = settings.Layout;
            float referenceHeight = layout.ReferenceResolution.y;
            float wheelBottomFromCanvasBottom = (referenceHeight * 0.5f) + layout.WheelPosition.y - (layout.WheelSize.y * 0.5f);
            float statusTopFromCanvasBottom = layout.StatusPosition.y + (layout.StatusSize.y * 0.5f);
            float statusBottomFromCanvasBottom = layout.StatusPosition.y - (layout.StatusSize.y * 0.5f);
            float buttonTopFromCanvasBottom = layout.ButtonRowPosition.y + (layout.ButtonSize.y * 0.5f);

            Require(wheelBottomFromCanvasBottom - statusTopFromCanvasBottom >= 40f, "status text keeps at least 40px from wheel bottom");
            Require(statusBottomFromCanvasBottom - buttonTopFromCanvasBottom >= 0f, "status text does not overlap the button row");
        }

        private static void RequirePerformanceMonitorLifecycle(WheelPerformanceMonitor monitor)
        {
            monitor.StopRecorders();
            Require(!monitor.IsRunning, "WheelPerformanceMonitor stops recorders");
            monitor.StartRecorders();
            Require(monitor.IsRunning, "WheelPerformanceMonitor starts recorders");
            WheelPerformanceMetricSnapshot snapshot = monitor.Snapshot(1f / 60f);
            Require(snapshot.FrameMilliseconds > 0f, "WheelPerformanceMonitor returns frame metric");
            monitor.StopRecorders();
            Require(!monitor.IsRunning, "WheelPerformanceMonitor disposes recorders");
        }

        private static bool DeclaresUpdate(Type type)
        {
            return type.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null;
        }

        private static void RequireRuntimeScriptsAvoidForbiddenApis()
        {
            string[] roots =
            {
                "Assets/Scripts/Data",
                "Assets/Scripts/Runtime",
                "Assets/Scripts/Views"
            };

            string[] forbiddenTokens =
            {
                "Destroy(",
                "DestroyImmediate(",
                ".GetChild(",
                "GetComponentsInChildren",
                "ChildByName",
                "FindDeepChild",
                "new GameObject(",
                "FindObjectOfType",
                "GameObject.Find",
                "switch ("
            };

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                string[] files = Directory.GetFiles(roots[rootIndex], "*.cs", SearchOption.AllDirectories);
                for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
                {
                    string source = File.ReadAllText(files[fileIndex]);
                    for (int tokenIndex = 0; tokenIndex < forbiddenTokens.Length; tokenIndex++)
                    {
                        Require(!source.Contains(forbiddenTokens[tokenIndex]), files[fileIndex] + " avoids " + forbiddenTokens[tokenIndex]);
                    }
                }
            }
        }

        private static void RequireHostReferences()
        {
            WheelStaticUiHost staticHost = UnityEngine.Object.FindObjectOfType<WheelStaticUiHost>();
            WheelWheelUiHost wheelHost = UnityEngine.Object.FindObjectOfType<WheelWheelUiHost>();
            WheelHudUiHost hudHost = UnityEngine.Object.FindObjectOfType<WheelHudUiHost>();
            Require(staticHost != null, "WheelStaticUiHost exists on static canvas");
            Require(wheelHost != null, "WheelWheelUiHost exists on wheel canvas");
            Require(hudHost != null, "WheelHudUiHost exists on HUD canvas");
            RequireObjectReference(staticHost, "_background", "WheelStaticUiHost references background view");
            RequireObjectReference(wheelHost, "_wheel", "WheelWheelUiHost references wheel view");
            RequireObjectReference(wheelHost, "_wheelBaseSkin", "WheelWheelUiHost references wheel base skin");
            RequireObjectReference(wheelHost, "_indicatorSkin", "WheelWheelUiHost references indicator skin");
            RequireObjectReference(hudHost, "_zoneProgress", "WheelHudUiHost references zone progress");
            RequireObjectReference(hudHost, "_milestoneBadges", "WheelHudUiHost references milestone badges");
            RequireObjectReference(hudHost, "_zonePanel", "WheelHudUiHost references zone panel");
            RequireObjectReference(hudHost, "_zoneText", "WheelHudUiHost references zone text");
            RequireObjectReference(hudHost, "_zoneTypeText", "WheelHudUiHost references zone type text");
            RequireObjectReference(hudHost, "_lootPanel", "WheelHudUiHost references loot panel");
            RequireObjectReference(hudHost, "_outcomePopup", "WheelHudUiHost references outcome popup");
            RequireObjectReference(hudHost, "_exitConfirmation", "WheelHudUiHost references exit confirmation");
            RequireObjectReference(hudHost, "_rewardOpening", "WheelHudUiHost references reward opening");
            RequireObjectReference(hudHost, "_statusText", "WheelHudUiHost references status text");
            RequireObjectReference(hudHost, "_leaveButton", "WheelHudUiHost references leave button");
            RequireObjectReference(hudHost, "_spinButton", "WheelHudUiHost references spin button");
            RequireObjectReference(hudHost, "_restartButton", "WheelHudUiHost references restart button");
            RequireObjectReference(hudHost, "_rewardOpeningRestartButton", "WheelHudUiHost references reward opening restart button");
        }

        private static void RequireExitConfirmationReferences()
        {
            WheelExitConfirmationView confirmation = UnityEngine.Object.FindObjectOfType<WheelExitConfirmationView>(true);
            Require(confirmation != null, "WheelExitConfirmationView exists including inactive overlay");
            RequireObjectReference(confirmation, "_root", "WheelExitConfirmationView has serialized root");
            RequireObjectReference(confirmation, "_canvasGroup", "WheelExitConfirmationView has serialized canvas group");
            RequireObjectReference(confirmation, "_contentRoot", "WheelExitConfirmationView has serialized content root");
            RequireObjectReference(confirmation, "_titleText", "WheelExitConfirmationView has serialized title");
            RequireObjectReference(confirmation, "_bodyText", "WheelExitConfirmationView has serialized body");
            RequireObjectReference(confirmation, "_collectButton", "WheelExitConfirmationView has collect button");
            RequireObjectReference(confirmation, "_comeBackButton", "WheelExitConfirmationView has come back button");
        }

        private static void RequireRuntimeUiAvoidAllocationTextPatterns()
        {
            string[] files =
            {
                "Assets/Scripts/Views/Hud/WheelZoneTextView.cs",
                "Assets/Scripts/Views/Hud/WheelZoneTypeTextView.cs",
                "Assets/Scripts/Views/Wheel/WheelSliceView.cs",
                "Assets/Scripts/Views/Hud/WheelStatusTextView.cs",
                "Assets/Scripts/Views/Outcome/WheelOutcomePopupView.cs"
            };

            string[] forbiddenTokens =
            {
                ".ToString(",
                "\" +"
            };

            for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
            {
                string source = File.ReadAllText(files[fileIndex]);
                for (int tokenIndex = 0; tokenIndex < forbiddenTokens.Length; tokenIndex++)
                {
                    Require(!source.Contains(forbiddenTokens[tokenIndex]), files[fileIndex] + " avoids text allocation token " + forbiddenTokens[tokenIndex]);
                }
            }
        }

        private static WheelSliceDefinition[] CreateSliceBuffer(int count)
        {
            var slices = new WheelSliceDefinition[count];
            for (int i = 0; i < slices.Length; i++)
            {
                slices[i] = new WheelSliceDefinition();
            }

            return slices;
        }

        private static bool TypeExists(string typeName)
        {
            return Type.GetType(typeName) != null;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Smoke check failed: " + message);
            }
        }
    }
}
#endif
