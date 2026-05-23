#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;
using Vertigo.Wheel.Diagnostics;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    internal static class VertigoWheelSceneBuilder
    {
        private const string UiParticleLayerName = "UIParticle";
        private const string UiParticleFolder = "Assets/UIPrefab";
        private const string UiParticleRenderTexturePath = UiParticleFolder + "/UIParticleRenderTexture.renderTexture";
        private const string UiParticleMaterialPath = UiParticleFolder + "/UIParticle_Additive.mat";
        private const string UiParticleDisplayMaterialPath = UiParticleFolder + "/UIParticle_Display.mat";
        private const string UiParticleDisplayShaderPath = UiParticleFolder + "/UIParticle_Display.shader";
        private const string UiParticlePrefabPath = UiParticleFolder + "/UIParticlePrefab.prefab";

        public static void CreateScene(Material material)
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("ui_preview_camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            WheelUiCopyCatalog uiCopyCatalog = VertigoWheelAssetPipeline.EnsureUiCopyCatalog();
            WheelSkinCatalog skinCatalog = VertigoWheelAssetPipeline.EnsureSkinCatalog();
            WheelSliceLayoutCatalog sliceLayoutCatalog = VertigoWheelAssetPipeline.EnsureSliceLayoutCatalog();
            WheelOutcomePopupMotionCatalog outcomePopupMotionCatalog = VertigoWheelAssetPipeline.EnsureOutcomePopupMotionCatalog();
            WheelSpinResolveCatalog spinResolveCatalog = VertigoWheelAssetPipeline.EnsureSpinResolveCatalog();

            WheelGameSettings settings = VertigoWheelAssetPipeline.EnsureGameSettings();
            settings.ConfigureUiCopy(uiCopyCatalog);
            settings.ConfigureSkinCatalog(skinCatalog);
            settings.ConfigureSliceLayoutCatalog(sliceLayoutCatalog);
            settings.ConfigureOutcomePopupMotionCatalog(outcomePopupMotionCatalog);
            settings.ConfigureSpinResolveCatalog(spinResolveCatalog);
            ConfigureDefaultRewards(settings);
            EditorUtility.SetDirty(settings);
            UiParticleAssets uiParticleAssets = EnsureUiParticleAssets(settings);
            EnsureUiParticlePrefab(settings, uiParticleAssets);

            var gameObject = new GameObject("game_wheel_runtime");
            var gameState = gameObject.AddComponent<WheelGameState>();
            var publisher = gameObject.AddComponent<WheelStatePublisher>();
            var flowController = gameObject.AddComponent<WheelGameFlowController>();

            Transform staticCanvas = CreateCanvas("ui_static_canvas", settings, 0, false);
            Transform wheelCanvas = CreateCanvas("ui_wheel_canvas", settings, 10, false);
            Transform hudCanvas = CreateCanvas("ui_hud_canvas", settings, 20, true);
            Transform debugCanvas = CreateCanvas("debug_canvas", settings, 30, false);

            Image background = CreateImage(staticCanvas, "ui_image_background", null, FullStretch(), material);
            var backgroundView = background.gameObject.AddComponent<WheelBackgroundView>();
            WheelEditorWiring.SetReference(backgroundView, "_image", background);

            var wheelRoot = new GameObject("ui_wheel_animator_root");
            wheelRoot.transform.SetParent(wheelCanvas, false);
            var wheelRect = wheelRoot.AddComponent<RectTransform>();
            wheelRect.anchorMin = new Vector2(0.5f, 0.5f);
            wheelRect.anchorMax = new Vector2(0.5f, 0.5f);
            wheelRect.pivot = new Vector2(0.5f, 0.5f);
            wheelRect.anchoredPosition = settings.Layout.WheelPosition;
            wheelRect.sizeDelta = settings.Layout.WheelSize;

            Image wheelBase = CreateImage(wheelRoot.transform, "ui_image_spin_base", FindSprite("ui_spin_silver_base"), FullStretch(), material);
            var wheelBaseSkinView = wheelBase.gameObject.AddComponent<WheelSkinView>();
            WheelEditorWiring.SetEnum(wheelBaseSkinView, "_slot", (int)WheelSkinSlot.WheelBase);
            WheelEditorWiring.SetReference(wheelBaseSkinView, "_catalog", skinCatalog);
            WheelEditorWiring.SetReference(wheelBaseSkinView, "_image", wheelBase);

            var sliceRoot = new GameObject("ui_wheel_slice_root");
            sliceRoot.transform.SetParent(wheelRoot.transform, false);
            var sliceRect = sliceRoot.AddComponent<RectTransform>();
            sliceRect.anchorMin = Vector2.zero;
            sliceRect.anchorMax = Vector2.one;
            sliceRect.offsetMin = Vector2.zero;
            sliceRect.offsetMax = Vector2.zero;
            WheelSliceView[] sliceViews = CreateSlicePool(sliceRect, settings.SliceCount, material);

            Image indicator = CreateImage(wheelCanvas, "ui_image_spin_indicator", FindSprite("ui_spin_silver_indicator"), Anchored(new Vector2(0.5f, 0.5f), settings.Layout.IndicatorPosition, settings.Layout.IndicatorSize), material);
            RectTransform indicatorRect = indicator.rectTransform;
            indicatorRect.pivot = new Vector2(0.5f, 1f);
            indicatorRect.localRotation = Quaternion.Euler(0f, 0f, 180f);
            var indicatorSkinView = indicator.gameObject.AddComponent<WheelSkinView>();
            WheelEditorWiring.SetEnum(indicatorSkinView, "_slot", (int)WheelSkinSlot.Indicator);
            WheelEditorWiring.SetReference(indicatorSkinView, "_catalog", skinCatalog);
            WheelEditorWiring.SetReference(indicatorSkinView, "_image", indicator);

            var view = wheelRoot.AddComponent<WheelView>();
            WheelEditorWiring.SetReference(view, "_slicePoolRoot", sliceRoot.transform);
            WheelEditorWiring.CollectChildren(view, "_sliceViews", sliceRoot.transform);
            var spinner = wheelRoot.AddComponent<WheelSpinner>();
            HeaderViews headerViews = BuildHeader(hudCanvas, settings, material);
            WheelOutcomePopupView outcomePopupView = BuildOutcomePopup(hudCanvas, settings, material, uiParticleAssets);
            WheelExitConfirmationView exitConfirmationView = BuildExitConfirmation(hudCanvas, settings, material);
            RewardOpeningViews rewardOpeningViews = BuildRewardOpening(hudCanvas, settings, material);
            WheelStatusTextView statusTextView = BuildFooter(hudCanvas, settings);
            ButtonActions buttonActions = BuildButtons(hudCanvas, headerViews.lootRoot, settings, material);
            outcomePopupView.transform.SetAsLastSibling();
            BuildDebugOverlay(debugCanvas, settings);

            WireStaticHost(staticCanvas, backgroundView);
            WireWheelHost(wheelCanvas, view, wheelBaseSkinView, indicatorSkinView);
            WireHudHost(hudCanvas, headerViews, outcomePopupView, exitConfirmationView, rewardOpeningViews, statusTextView, buttonActions);

            var configSource = gameObject.AddComponent<WheelGameConfigSource>();
            WheelEditorWiring.SetReference(configSource, "_settings", settings);
            gameObject.AddComponent<WheelRuntimeCompositionRoot>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, VertigoWheelPaths.ScenePath);
            VertigoWheelProjectSetup.ConfigureAndroidProject();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void WireStaticHost(Transform staticCanvas, WheelBackgroundView backgroundView)
        {
            var host = staticCanvas.gameObject.AddComponent<WheelStaticUiHost>();
            WheelEditorWiring.SetReference(host, "_background", backgroundView);
        }

        private static void WireWheelHost(
            Transform wheelCanvas,
            WheelView wheelView,
            WheelSkinView wheelBaseSkinView,
            WheelSkinView indicatorSkinView)
        {
            var host = wheelCanvas.gameObject.AddComponent<WheelWheelUiHost>();
            WheelEditorWiring.SetReference(host, "_wheel", wheelView);
            WheelEditorWiring.SetReference(host, "_wheelBaseSkin", wheelBaseSkinView);
            WheelEditorWiring.SetReference(host, "_indicatorSkin", indicatorSkinView);
        }

        private static void WireHudHost(
            Transform hudCanvas,
            HeaderViews headerViews,
            WheelOutcomePopupView outcomePopupView,
            WheelExitConfirmationView exitConfirmationView,
            RewardOpeningViews rewardOpeningViews,
            WheelStatusTextView statusTextView,
            ButtonActions buttonActions)
        {
            var host = hudCanvas.gameObject.AddComponent<WheelHudUiHost>();
            WheelEditorWiring.SetReference(host, "_zoneProgress", headerViews.zoneProgress);
            WheelEditorWiring.SetReference(host, "_milestoneBadges", headerViews.milestoneBadges);
            WheelEditorWiring.SetReference(host, "_zonePanel", headerViews.zonePanel);
            WheelEditorWiring.SetReference(host, "_zoneText", headerViews.zoneText);
            WheelEditorWiring.SetReference(host, "_zoneTypeText", headerViews.zoneTypeText);
            WheelEditorWiring.SetReference(host, "_lootPanel", headerViews.lootPanel);
            WheelEditorWiring.SetReference(host, "_outcomePopup", outcomePopupView);
            WheelEditorWiring.SetReference(host, "_exitConfirmation", exitConfirmationView);
            WheelEditorWiring.SetReference(host, "_rewardOpening", rewardOpeningViews.opening);
            WheelEditorWiring.SetReference(host, "_statusText", statusTextView);
            WheelEditorWiring.SetReference(host, "_leaveButton", buttonActions.leave);
            WheelEditorWiring.SetReference(host, "_spinButton", buttonActions.spin);
            WheelEditorWiring.SetReference(host, "_restartButton", buttonActions.restart);
            WheelEditorWiring.SetReference(host, "_rewardOpeningRestartButton", rewardOpeningViews.restart);
        }

        private static Transform CreateCanvas(string name, WheelGameSettings settings, int sortingOrder, bool raycasterEnabled)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = settings.Layout.ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            if (raycasterEnabled)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            return canvasObject.transform;
        }

        private static void BuildDebugOverlay(Transform parent, WheelGameSettings settings)
        {
            TextMeshProUGUI text = CreateText(
                parent,
                "debug_text_performance",
                "PERF",
                Anchored(new Vector2(0f, 1f), new Vector2(168f, -92f), new Vector2(312f, 144f)),
                16,
                TextAlignmentOptions.TopLeft,
                settings.Theme.SecondaryTextColor);
            text.enableAutoSizing = false;
            text.fontSize = 16f;
            text.gameObject.AddComponent<WheelPerformanceMonitor>();
            text.gameObject.AddComponent<WheelPerformanceOverlay>();
        }

        private static void ConfigureDefaultRewards(WheelGameSettings settings)
        {
            settings.SetBombReward(RewardDefinition.Create(
                "bomb",
                "Bomb",
                FindSprite("ui_card_icon_death"),
                1,
                RewardTier.Legendary,
                settings.Theme.DangerColor));

            settings.StandardRewards.Clear();
            settings.StandardRewards.Add(Reward("cash", "Cash", FindSprite("UI_icon_cash"), 150, RewardTier.Common, new Color(0.52f, 1f, 0.42f, 1f)));
            settings.StandardRewards.Add(Reward("pistol_points", "Pistol Points", FindSprite("UI_Icons_Pistol_Points"), 25, RewardTier.Common, new Color(0.62f, 0.9f, 1f, 1f)));
            settings.StandardRewards.Add(Reward("shotgun_points", "Shotgun Points", FindSprite("UI_Icons_Shotgun_Points"), 20, RewardTier.Common, new Color(0.84f, 0.72f, 0.58f, 1f)));
            settings.StandardRewards.Add(Reward("smg_points", "SMG Points", FindSprite("UI_Icons_SMG_Points"), 18, RewardTier.Common, new Color(0.48f, 0.88f, 1f, 1f)));
            settings.StandardRewards.Add(Reward("knife_points", "Knife Points", FindSprite("UI_Icons_Knife_Points"), 14, RewardTier.Common, new Color(0.8f, 0.86f, 0.96f, 1f)));
            settings.StandardRewards.Add(Reward("armor_points", "Armor Points", FindSprite("UI_Icons_Armor_Points"), 16, RewardTier.Common, new Color(0.52f, 0.74f, 1f, 1f)));
            settings.StandardRewards.Add(Reward("vest_points", "Vest Points", FindSprite("UI_Icons_Vest_Points"), 16, RewardTier.Common, new Color(0.58f, 0.78f, 1f, 1f)));
            settings.StandardRewards.Add(Reward("small_chest", "Small Chest", FindSprite("UI_icon_chest_small_noligt"), 1, RewardTier.Common, new Color(0.72f, 0.55f, 0.38f, 1f)));
            settings.StandardRewards.Add(Reward("standard_chest", "Standard Chest", FindSprite("UI_icon_chest_standart_nolight"), 1, RewardTier.Rare, new Color(0.88f, 0.72f, 0.48f, 1f)));
            settings.StandardRewards.Add(Reward("bronze_chest", "Bronze Chest", FindSprite("UI_icon_chest_Bronze_nolight"), 1, RewardTier.Rare, new Color(0.88f, 0.55f, 0.24f, 1f)));
            settings.StandardRewards.Add(Reward("tier1_shotgun", "Tier 1 Shotgun", FindSprite("UI_Icon_Renders_tier1_shotgun"), 1, RewardTier.Common, new Color(0.9f, 0.82f, 0.62f, 1f)));
            settings.StandardRewards.Add(Reward("tier2_rifle", "Tier 2 Rifle", FindSprite("UI_Icon_Renders_tier2_rifle"), 1, RewardTier.Rare, new Color(0.48f, 0.72f, 1f, 1f)));
            settings.StandardRewards.Add(Reward("grenade_m26", "M26 Grenade", FindSprite("ui_icon_render_cons_grenade_m26"), 1, RewardTier.Common, Color.white));
            settings.StandardRewards.Add(Reward("molotov", "Molotov", FindSprite("ui_icon_render_t_cons_molotov"), 1, RewardTier.Common, new Color(1f, 0.54f, 0.24f, 1f)));

            settings.SafeRewards.Clear();
            settings.SafeRewards.Add(Reward("cash_safe", "Cash", FindSprite("UI_icon_cash"), 300, RewardTier.Common, new Color(0.52f, 1f, 0.42f, 1f)));
            settings.SafeRewards.Add(Reward("gold_safe", "Gold", FindSprite("UI_icon_gold"), 10, RewardTier.Rare, new Color(1f, 0.78f, 0.22f, 1f)));
            settings.SafeRewards.Add(Reward("pistol_points_high", "Pistol Bonus", FindSprite("UI_Icons_Pistol_Points_"), 45, RewardTier.Rare, new Color(0.62f, 0.9f, 1f, 1f)));
            settings.SafeRewards.Add(Reward("rifle_points", "Rifle Points", FindSprite("UI_Icons_Rifle_Points"), 35, RewardTier.Rare, new Color(0.48f, 0.72f, 1f, 1f)));
            settings.SafeRewards.Add(Reward("submachine_points", "Submachine Bonus", FindSprite("UI_Icons_Submachine_Points"), 32, RewardTier.Rare, new Color(0.42f, 0.92f, 1f, 1f)));
            settings.SafeRewards.Add(Reward("sniper_points", "Sniper Points", FindSprite("UI_Icons_Sniper_Points"), 28, RewardTier.Rare, new Color(0.72f, 0.86f, 1f, 1f)));
            settings.SafeRewards.Add(Reward("silver_chest", "Silver Chest", FindSprite("UI_icon_chest_silver_nolight"), 1, RewardTier.Rare, settings.Theme.SafeZoneColor));
            settings.SafeRewards.Add(Reward("standard_chest_safe", "Standard Chest", FindSprite("UI_icon_chest_standart_nolight"), 1, RewardTier.Rare, settings.Theme.SafeZoneColor));
            settings.SafeRewards.Add(Reward("tier2_melee", "Tier 2 Melee", FindSprite("UI_Icon_Renders_tier2_mle"), 1, RewardTier.Rare, new Color(0.46f, 0.74f, 1f, 1f)));
            settings.SafeRewards.Add(Reward("tier3_smg", "Tier 3 SMG", FindSprite("UI_Icon_Renders_tier3_smg"), 1, RewardTier.Epic, new Color(0.58f, 0.95f, 1f, 1f)));
            settings.SafeRewards.Add(Reward("grenade_m67", "M67 Grenade", FindSprite("ui_icon_render_cons_grenade_m67"), 1, RewardTier.Common, Color.white));
            settings.SafeRewards.Add(Reward("neurostim", "Neurostim", FindSprite("ui_icon_render_cons_healthshot_2_neurostim"), 1, RewardTier.Common, Color.white));
            settings.SafeRewards.Add(Reward("regenerator", "Regenerator", FindSprite("ui_icon_render_cons_healthshot_2_regenerator"), 1, RewardTier.Rare, new Color(0.48f, 1f, 0.72f, 1f)));

            settings.SuperRewards.Clear();
            settings.SuperRewards.Add(Reward("gold", "Gold", FindSprite("UI_icon_gold"), 25, RewardTier.Legendary, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("gold_chest", "Gold Chest", FindSprite("UI_icon_chest_gold_nolight"), 1, RewardTier.Epic, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("super_chest", "Super Chest", FindSprite("UI_icon_chest_super_nolight"), 1, RewardTier.Legendary, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("big_chest", "Big Chest", FindSprite("UI_icon_chest_big_nolight"), 1, RewardTier.Epic, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("tier3_sniper", "Tier 3 Sniper", FindSprite("UI_Icon_Renders_tier3_sniper"), 1, RewardTier.Legendary, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("tier3_shotgun", "Tier 3 Shotgun", FindSprite("UI_Icon_Renders_tier3_shotgun"), 1, RewardTier.Legendary, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("easter_bayonet", "Easter Bayonet", FindSprite("ui_icon_mle_bayonet_easter_time"), 1, RewardTier.Legendary, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("summer_bayonet", "Summer Bayonet", FindSprite("ui_icon_mle_bayonet_summer_vice"), 1, RewardTier.Legendary, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("aviator_glasses", "Aviator Glasses", FindSprite("ui_icon_aviator_glasses_easter"), 1, RewardTier.Epic, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("baseball_cap", "Baseball Cap", FindSprite("ui_icon_baseball_cap_easter"), 1, RewardTier.Epic, settings.Theme.SuperZoneColor));
            settings.SuperRewards.Add(Reward("pumpkin_helmet", "Pumpkin Helmet", FindSprite("ui_icon_helmet_pumpkin"), 1, RewardTier.Epic, new Color(1f, 0.48f, 0.22f, 1f)));
            settings.SuperRewards.Add(Reward("regenerator_super", "Regenerator", FindSprite("ui_icon_render_cons_healthshot_2_regenerator"), 2, RewardTier.Epic, new Color(0.48f, 1f, 0.72f, 1f)));

            WheelLayoutSettings layout = settings.Layout;
            layout.ApplyCenteredComposition();
            layout.BindFrameSprites(
                FindSprite(layout.RewardPanelFrameSpriteName),
                FindSprite(layout.RewardCardFrameSpriteName));
            layout.BindUiSprites(
                FindSprite(layout.ZoneStandardPanelSpriteName),
                FindSprite(layout.ZoneUpcomingPanelSpriteName),
                FindSprite(layout.ZoneSmallFrameSpriteName),
                FindSprite(layout.StarFlashSpriteName),
                FindSprite(layout.StarGlowSpriteName),
                FindSprite(layout.ShineSpriteName));
        }

        private static HeaderViews BuildHeader(Transform parent, WheelGameSettings settings, Material material)
        {
            WheelLayoutSettings layout = settings.Layout;
            TMP_FontAsset hudFont = VertigoWheelAssetPipeline.EnsureHudFontAsset();
            WheelZoneProgressView zoneProgress = BuildZoneProgressRow(parent, settings, material);
            WheelMilestoneBadgesView milestoneBadges = BuildMilestoneBadges(parent, settings, material);
            WheelZonePanelView zonePanelView = BuildZonePanel(parent, settings, layout, material);
            WheelRewardPanelView lootPanelView = BuildLootStrip(parent, settings, layout, material);
            TextMeshProUGUI zoneText = CreateText(
                zoneProgress.transform,
                "ui_header_zone_value",
                "ZONE 1",
                Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -46f), new Vector2(150f, 16f)),
                12,
                TextAlignmentOptions.Center,
                settings.Theme.PrimaryTextColor);
            TextMeshProUGUI zoneTypeText = CreateText(
                zoneProgress.transform,
                "ui_header_zone_type_value",
                "STANDARD",
                Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -61f), new Vector2(150f, 12f)),
                10,
                TextAlignmentOptions.Center,
                settings.Theme.StandardZoneColor);
            ApplyZoneHeaderTextStyle(zoneText, hudFont, 12f, 3f);
            ApplyZoneHeaderTextStyle(zoneTypeText, hudFont, 10f, 5f);
            WheelZoneTextView zoneTextView = zoneText.gameObject.AddComponent<WheelZoneTextView>();
            WheelZoneTypeTextView zoneTypeTextView = zoneTypeText.gameObject.AddComponent<WheelZoneTypeTextView>();
            WheelEditorWiring.SetReference(zoneTextView, "_text", zoneText);
            WheelEditorWiring.SetReference(zoneTypeTextView, "_text", zoneTypeText);
            return new HeaderViews
            {
                zoneProgress = zoneProgress,
                milestoneBadges = milestoneBadges,
                zonePanel = zonePanelView,
                lootPanel = lootPanelView,
                zoneText = zoneTextView,
                zoneTypeText = zoneTypeTextView,
                lootRoot = lootPanelView.transform
            };
        }

        private static WheelZoneProgressView BuildZoneProgressRow(Transform parent, WheelGameSettings settings, Material material)
        {
            TMP_FontAsset hudFont = VertigoWheelAssetPipeline.EnsureHudFontAsset();
            var progressRoot = new GameObject("ui_zone_progress_root");
            progressRoot.transform.SetParent(parent, false);
            var progressRect = progressRoot.AddComponent<RectTransform>();
            Apply(progressRect, Anchored(new Vector2(0.5f, 1f), new Vector2(0f, -125f), new Vector2(1120f, 138f)));
            progressRoot.AddComponent<RectMask2D>();
            CreateZoneTargetGuide(progressRoot.transform, settings, material, -252f);
            CreateZoneTargetGuide(progressRoot.transform, settings, material, 252f);

            int cellCount = 11;
            float spacing = 100f;
            float startX = -spacing * ((cellCount - 1) * 0.5f);
            for (int i = 0; i < cellCount; i++)
            {
                var cellRoot = new GameObject("ui_zone_progress_cell_" + i);
                cellRoot.transform.SetParent(progressRoot.transform, false);
                var cellRect = cellRoot.AddComponent<RectTransform>();
                Apply(cellRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(startX + (i * spacing), 12f), new Vector2(62f, 62f)));

                Image cellImage = cellRoot.AddComponent<Image>();
                cellImage.sprite = settings.Layout.ZoneStandardPanelSprite;
                cellImage.material = material;
                cellImage.type = Image.Type.Sliced;
                cellImage.raycastTarget = false;
                cellImage.maskable = true;

                Image glow = CreateImage(cellRoot.transform, "ui_zone_progress_cell_" + i + "_glow", settings.Layout.StarGlowSprite, Anchored(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(88f, 88f)), material);
                glow.maskable = true;
                glow.color = Color.clear;
                glow.transform.SetAsFirstSibling();

                Image cellFrame = CreateImage(cellRoot.transform, "ui_zone_progress_cell_" + i + "_frame", settings.Layout.ZoneSmallFrameSprite, FullStretch(), material);
                cellFrame.type = Image.Type.Sliced;
                cellFrame.preserveAspect = false;
                cellFrame.maskable = true;
                cellFrame.color = new Color(1f, 1f, 1f, 0.62f);

                TextMeshProUGUI label = CreateText(cellRoot.transform, "ui_zone_progress_cell_" + i + "_value", "1", FullStretch(), 28, TextAlignmentOptions.Center, settings.Theme.SecondaryTextColor);
                label.maskable = true;
                label.font = hudFont;
                label.fontStyle = FontStyles.Normal;
                label.outlineWidth = 0.08f;
                label.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            }

            CreateZoneTargetReticle(progressRoot.transform, settings, material);

            WheelZoneProgressView view = progressRoot.AddComponent<WheelZoneProgressView>();
            WheelZoneProgressViewEditor.CollectZoneCells(view);
            WheelEditorWiring.SetReference(view, "_standardSprite", settings.Layout.ZoneStandardPanelSprite);
            WheelEditorWiring.SetReference(view, "_currentSprite", FindSprite("ui_card_panel_zone_current_white"));
            WheelEditorWiring.SetReference(view, "_safeSprite", FindSprite("ui_card_panel_zone_white"));
            WheelEditorWiring.SetReference(view, "_superSprite", FindSprite("ui_card_panel_zone_super"));
            WheelEditorWiring.SetFloat(view, "_cellSpacing", spacing);
            WheelEditorWiring.SetFloat(view, "_slideDuration", 0.42f);
            WheelEditorWiring.SetFloat(view, "_regularCellScale", 0.88f);
            WheelEditorWiring.SetFloat(view, "_currentCellScale", 1.16f);
            return view;
        }

        private static void CreateZoneTargetGuide(Transform parent, WheelGameSettings settings, Material material, float centerX)
        {
            Color glowColor = settings.Theme.SafeZoneColor;
            glowColor.a = 0.13f;
            Image glow = CreateImage(parent, "ui_zone_target_guide_glow_" + centerX, null, Anchored(new Vector2(0.5f, 0.5f), new Vector2(centerX, 12f), new Vector2(390f, 12f)), material);
            glow.preserveAspect = false;
            glow.color = glowColor;

            Color coreColor = Color.white;
            coreColor.a = 0.3f;
            Image core = CreateImage(parent, "ui_zone_target_guide_core_" + centerX, null, Anchored(new Vector2(0.5f, 0.5f), new Vector2(centerX, 12f), new Vector2(368f, 3f)), material);
            core.preserveAspect = false;
            core.color = coreColor;

            Color endColor = settings.Theme.SafeZoneColor;
            endColor.a = 0.48f;
            Image endCap = CreateImage(parent, "ui_zone_target_guide_cap_" + centerX, null, Anchored(new Vector2(0.5f, 0.5f), new Vector2(centerX > 0f ? centerX + 188f : centerX - 188f, 12f), new Vector2(3f, 18f)), material);
            endCap.preserveAspect = false;
            endCap.color = endColor;
        }

        private static void CreateZoneTargetReticle(Transform parent, WheelGameSettings settings, Material material)
        {
            Color glowColor = settings.Theme.SafeZoneColor;
            glowColor.a = 0.26f;
            Image glow = CreateImage(parent, "ui_zone_target_center_glow", settings.Layout.StarGlowSprite, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(104f, 104f)), material);
            glow.color = glowColor;
            glow.raycastTarget = false;

            CreateZoneTargetCornerLock(parent, material, new Vector2(0f, 12f));
        }

        private static void CreateZoneTargetCornerLock(Transform parent, Material material, Vector2 center)
        {
            Color glowColor = new Color(1f, 1f, 1f, 0.28f);
            CreateZoneTargetCornerSegments(parent, material, center, "glow", 43f, 29f, 9f, glowColor);

            Color coreColor = new Color(1f, 1f, 1f, 0.96f);
            CreateZoneTargetCornerSegments(parent, material, center, "core", 40f, 23f, 5f, coreColor);
        }

        private static void CreateZoneTargetCornerSegments(
            Transform parent,
            Material material,
            Vector2 center,
            string layer,
            float halfSize,
            float armLength,
            float thickness,
            Color color)
        {
            int index = 0;
            for (int xSign = -1; xSign <= 1; xSign += 2)
            {
                for (int ySign = -1; ySign <= 1; ySign += 2)
                {
                    Vector2 horizontalPosition = center + new Vector2(xSign * (halfSize - (armLength * 0.5f)), ySign * halfSize);
                    Vector2 verticalPosition = center + new Vector2(xSign * halfSize, ySign * (halfSize - (armLength * 0.5f)));
                    CreateZoneTargetTick(parent, material, horizontalPosition, new Vector2(armLength, thickness), color, "corner_" + layer + "_" + index + "_h");
                    CreateZoneTargetTick(parent, material, verticalPosition, new Vector2(thickness, armLength), color, "corner_" + layer + "_" + index + "_v");
                    index++;
                }
            }
        }

        private static void CreateZoneTargetTick(Transform parent, Material material, Vector2 position, Vector2 size, Color color, string suffix)
        {
            Image tick = CreateImage(parent, "ui_zone_target_reticle_" + suffix, null, Anchored(new Vector2(0.5f, 0.5f), position, size), material);
            tick.preserveAspect = false;
            tick.raycastTarget = false;
            tick.color = color;
        }

        private static WheelMilestoneBadgesView BuildMilestoneBadges(Transform parent, WheelGameSettings settings, Material material)
        {
            TMP_FontAsset hudFont = VertigoWheelAssetPipeline.EnsureHudFontAsset();
            var badgesRoot = new GameObject("ui_zone_milestone_badges_root");
            badgesRoot.transform.SetParent(parent, false);
            var badgesRect = badgesRoot.AddComponent<RectTransform>();
            Apply(badgesRect, Anchored(new Vector2(1f, 0.5f), new Vector2(-248f, 168f), new Vector2(360f, 252f)));

            Image superBadge = CreateMilestoneBadge(badgesRoot.transform, "ui_zone_badge_super", FindSprite("ui_card_panel_zone_super"), settings.Layout.StarGlowSprite, new Vector2(0f, 48f), settings.Theme.SuperMilestoneBadgeBackground, material);
            Image safeBadge = CreateMilestoneBadge(badgesRoot.transform, "ui_zone_badge_safe", settings.Layout.ZoneUpcomingPanelSprite, settings.Layout.StarGlowSprite, new Vector2(0f, -48f), settings.Theme.SafeMilestoneBadgeBackground, material);
            Image superGlow = superBadge.transform.Find("ui_zone_badge_super_glow").GetComponent<Image>();
            Image safeGlow = safeBadge.transform.Find("ui_zone_badge_safe_glow").GetComponent<Image>();
            TextMeshProUGUI superFallbackText = CreateText(superBadge.transform, "ui_zone_badge_super_value", string.Empty, FullStretch(), 30, TextAlignmentOptions.Center, settings.Theme.SuperZoneColor);
            TextMeshProUGUI safeFallbackText = CreateText(safeBadge.transform, "ui_zone_badge_safe_value", string.Empty, FullStretch(), 30, TextAlignmentOptions.Center, settings.Theme.SafeZoneColor);
            superFallbackText.gameObject.SetActive(false);
            safeFallbackText.gameObject.SetActive(false);

            Color superBadgeTextColor = new Color(0.68f, 1f, 0.14f, 1f);
            Color safeBadgeTextColor = new Color(0.66f, 0.93f, 1f, 1f);
            TextMeshProUGUI superLabelText = CreateText(superBadge.transform, "ui_zone_badge_super_label", "SUPER\nZONE", Anchored(new Vector2(0.5f, 0.5f), new Vector2(-62f, 0f), new Vector2(146f, 72f)), 32, TextAlignmentOptions.Center, superBadgeTextColor);
            TextMeshProUGUI safeLabelText = CreateText(safeBadge.transform, "ui_zone_badge_safe_label", "SAFE\nZONE", Anchored(new Vector2(0.5f, 0.5f), new Vector2(-62f, 0f), new Vector2(146f, 72f)), 32, TextAlignmentOptions.Center, safeBadgeTextColor);
            TextMeshProUGUI superNumberText = CreateText(superBadge.transform, "ui_zone_badge_super_number", "30", Anchored(new Vector2(0.5f, 0.5f), new Vector2(76f, 0f), new Vector2(112f, 80f)), 74, TextAlignmentOptions.Center, superBadgeTextColor);
            TextMeshProUGUI safeNumberText = CreateText(safeBadge.transform, "ui_zone_badge_safe_number", "5", Anchored(new Vector2(0.5f, 0.5f), new Vector2(76f, 0f), new Vector2(112f, 80f)), 74, TextAlignmentOptions.Center, safeBadgeTextColor);
            ApplyMilestoneTextStyle(superLabelText, hudFont, 12f);
            ApplyMilestoneTextStyle(safeLabelText, hudFont, 12f);
            ApplyMilestoneTextStyle(superNumberText, hudFont, 0f);
            ApplyMilestoneTextStyle(safeNumberText, hudFont, 0f);

            WheelMilestoneBadgesView view = badgesRoot.AddComponent<WheelMilestoneBadgesView>();
            WheelEditorWiring.SetReference(view, "_superBadgeImage", superBadge);
            WheelEditorWiring.SetReference(view, "_safeBadgeImage", safeBadge);
            WheelEditorWiring.SetReference(view, "_superBadgeGlowImage", superGlow);
            WheelEditorWiring.SetReference(view, "_safeBadgeGlowImage", safeGlow);
            WheelEditorWiring.SetReference(view, "_superBadgeText", superFallbackText);
            WheelEditorWiring.SetReference(view, "_safeBadgeText", safeFallbackText);
            WheelEditorWiring.SetReference(view, "_superBadgeLabelText", superLabelText);
            WheelEditorWiring.SetReference(view, "_safeBadgeLabelText", safeLabelText);
            WheelEditorWiring.SetReference(view, "_superBadgeNumberText", superNumberText);
            WheelEditorWiring.SetReference(view, "_safeBadgeNumberText", safeNumberText);
            return view;
        }

        private static Image CreateMilestoneBadge(Transform parent, string name, Sprite panelSprite, Sprite glowSprite, Vector2 position, Color color, Material material)
        {
            Image image = CreateImage(parent, name, panelSprite, Anchored(new Vector2(0.5f, 0.5f), position, new Vector2(330f, 98f)), material);
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = color;
            Image glow = CreateImage(image.transform, name + "_glow", glowSprite, Anchored(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(368f, 136f)), material);
            glow.color = new Color(1f, 1f, 1f, 0.18f);
            glow.transform.SetAsFirstSibling();
            return image;
        }

        private static void ApplyMilestoneTextStyle(TextMeshProUGUI text, TMP_FontAsset font, float spacing)
        {
            text.font = font;
            text.fontStyle = FontStyles.Normal;
            text.characterSpacing = spacing;
            text.outlineWidth = 0.08f;
            text.outlineColor = new Color(0f, 0f, 0f, 0.86f);
        }

        private static void ApplyZoneHeaderTextStyle(TextMeshProUGUI text, TMP_FontAsset font, float fontSize, float spacing)
        {
            text.font = font;
            text.enableAutoSizing = false;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Normal;
            text.characterSpacing = spacing;
            text.outlineWidth = 0.07f;
            text.outlineColor = new Color(0f, 0f, 0f, 0.88f);
        }

        private static WheelZonePanelView BuildZonePanel(Transform parent, WheelGameSettings settings, WheelLayoutSettings layout, Material material)
        {
            var zonePanelRoot = new GameObject("ui_zone_panel_root");
            zonePanelRoot.transform.SetParent(parent, false);
            var zonePanelRect = zonePanelRoot.AddComponent<RectTransform>();
            Apply(zonePanelRect, Anchored(new Vector2(0.5f, 1f), new Vector2(0f, -126f), new Vector2(1f, 1f)));

            Image panelBg = CreateImage(zonePanelRoot.transform, "ui_zone_panel_bg", null, FullStretch(), material);
            panelBg.type = Image.Type.Sliced;
            panelBg.preserveAspect = false;

            Image mapFrame = CreateImage(zonePanelRoot.transform, "ui_zone_map_frame", null, FullStretch(), material);
            mapFrame.type = Image.Type.Sliced;
            mapFrame.preserveAspect = false;
            var mapFrameRect = mapFrame.rectTransform;
            mapFrameRect.offsetMin = new Vector2(5f, 4f);
            mapFrameRect.offsetMax = new Vector2(-5f, -4f);

            WheelZonePanelView zonePanelView = zonePanelRoot.AddComponent<WheelZonePanelView>();
            WheelEditorWiring.SetReference(zonePanelView, "_panelImage", panelBg);
            WheelEditorWiring.SetReference(zonePanelView, "_mapFrameImage", mapFrame);
            return zonePanelView;
        }

        private static WheelRewardPanelView BuildLootStrip(Transform parent, WheelGameSettings settings, WheelLayoutSettings layout, Material material)
        {
            var lootRoot = new GameObject("ui_loot_strip_root");
            lootRoot.transform.SetParent(parent, false);
            var lootRect = lootRoot.AddComponent<RectTransform>();
            Apply(lootRect, Anchored(new Vector2(0.5f, 0.5f), layout.RewardPanelPosition, layout.RewardPanelSize));

            Image stripFrame = CreateImage(lootRoot.transform, "ui_loot_strip_frame", layout.RewardPanelFrameSprite, FullStretch(), material);
            stripFrame.type = Image.Type.Sliced;
            stripFrame.preserveAspect = false;

            var cardsViewport = new GameObject("ui_loot_cards_viewport");
            cardsViewport.transform.SetParent(lootRoot.transform, false);
            var viewportRect = cardsViewport.AddComponent<RectTransform>();
            Apply(viewportRect, FullStretch());
            viewportRect.offsetMin = new Vector2(24f, 24f);
            viewportRect.offsetMax = new Vector2(-24f, -112f);
            cardsViewport.AddComponent<RectMask2D>();

            var cardsContent = new GameObject("ui_loot_cards_scroll_content");
            cardsContent.transform.SetParent(cardsViewport.transform, false);
            var contentRect = cardsContent.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            CreateRewardCardPool(cardsContent.transform, layout, material);
            WheelRewardPanelView lootPanelView = lootRoot.AddComponent<WheelRewardPanelView>();
            ScrollRect scrollRect = lootRoot.AddComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 28f;
            WheelEditorWiring.SetReference(lootPanelView, "_panelFrameImage", stripFrame);
            WheelEditorWiring.SetReference(lootPanelView, "_cardPoolRoot", cardsContent.transform);
            WheelEditorWiring.SetReference(lootPanelView, "_viewportRect", viewportRect);
            WheelEditorWiring.SetReference(lootPanelView, "_contentRect", contentRect);
            WheelEditorWiring.SetReference(lootPanelView, "_scrollRect", scrollRect);
            WheelEditorWiring.CollectChildren(lootPanelView, "_cardViews", cardsContent.transform);
            return lootPanelView;
        }

        private static WheelRewardCardView[] CreateRewardCardPool(Transform parent, WheelLayoutSettings layout, Material material)
        {
            int cardCount = layout.MaxRewardCards;
            var cardViews = new WheelRewardCardView[cardCount];
            float cardWidth = layout.RewardCardSize.x;
            float cardHeight = layout.RewardCardSize.y;
            float spacing = 12f;
            bool vertical = layout.RewardPanelSize.y > layout.RewardPanelSize.x;
            float rowWidth = (cardCount * cardWidth) + ((cardCount - 1) * spacing);
            float columnHeight = (cardCount * cardHeight) + ((cardCount - 1) * spacing);
            float startX = (-rowWidth * 0.5f) + (cardWidth * 0.5f);
            float startY = (columnHeight * 0.5f) - (cardHeight * 0.5f);

            for (int i = 0; i < cardCount; i++)
            {
                float x = vertical ? 0f : startX + (i * (cardWidth + spacing));
                float y = vertical ? startY - (i * (cardHeight + spacing)) : 0f;
                var cardObject = new GameObject("ui_loot_card_" + i);
                cardObject.transform.SetParent(parent, false);
                var cardRect = cardObject.AddComponent<RectTransform>();
                Apply(cardRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(x, y), layout.RewardCardSize));

                Image frame = CreateImage(cardObject.transform, "ui_loot_card_" + i + "_frame", layout.RewardCardFrameSprite, FullStretch(), material);
                frame.type = Image.Type.Sliced;
                frame.preserveAspect = false;
                frame.maskable = true;

                Image icon = CreateImage(
                    cardObject.transform,
                    "ui_loot_card_" + i + "_icon",
                    null,
                    Anchored(new Vector2(0.5f, 0.5f), ResolveRewardCardIconPosition(vertical), ResolveRewardCardIconSize(cardWidth, cardHeight, vertical)),
                    material);
                icon.maskable = true;

                TextMeshProUGUI amount = CreateText(
                    cardObject.transform,
                    "ui_loot_card_" + i + "_amount",
                    string.Empty,
                    Anchored(new Vector2(0.5f, 0.5f), ResolveRewardCardAmountPosition(vertical), ResolveRewardCardAmountSize(cardWidth, cardHeight, vertical)),
                    18,
                    TextAlignmentOptions.Center,
                    Color.white);
                amount.fontStyle = FontStyles.Bold;
                amount.maskable = true;
                amount.enabled = false;

                WheelRewardCardView cardView = cardObject.AddComponent<WheelRewardCardView>();
                WheelEditorWiring.SetReference(cardView, "_frameImage", frame);
                WheelEditorWiring.SetReference(cardView, "_iconImage", icon);
                WheelEditorWiring.SetReference(cardView, "_amountText", amount);
                cardObject.SetActive(false);
                cardViews[i] = cardView;
            }

            return cardViews;
        }

        private static Vector2 ResolveRewardCardIconPosition(bool vertical)
        {
            Vector2[] positions = { new Vector2(0f, 8f), new Vector2(-58f, 0f) };
            return positions[System.Convert.ToInt32(vertical)];
        }

        private static Vector2 ResolveRewardCardIconSize(float cardWidth, float cardHeight, bool vertical)
        {
            Vector2[] sizes =
            {
                new Vector2(cardWidth * 0.62f, cardHeight * 0.52f),
                new Vector2(58f, 54f)
            };
            return sizes[System.Convert.ToInt32(vertical)];
        }

        private static Vector2 ResolveRewardCardAmountPosition(bool vertical)
        {
            Vector2[] positions = { new Vector2(0f, -44f), new Vector2(62f, 0f) };
            return positions[System.Convert.ToInt32(vertical)];
        }

        private static Vector2 ResolveRewardCardAmountSize(float cardWidth, float cardHeight, bool vertical)
        {
            Vector2[] sizes =
            {
                new Vector2(cardWidth - 8f, 28f),
                new Vector2(78f, cardHeight - 10f)
            };
            return sizes[System.Convert.ToInt32(vertical)];
        }

        private static WheelOutcomePopupView BuildOutcomePopup(Transform parent, WheelGameSettings settings, Material material, UiParticleAssets uiParticleAssets)
        {
            var controllerObject = new GameObject("ui_outcome_popup_controller");
            controllerObject.transform.SetParent(parent, false);
            var controllerRect = controllerObject.AddComponent<RectTransform>();
            Apply(controllerRect, FullStretch());

            Image overlay = CreateImage(controllerObject.transform, "ui_outcome_overlay", null, FullStretch(), material);
            overlay.color = new Color(0f, 0f, 0f, 0.68f);
            overlay.preserveAspect = false;
            CanvasGroup canvasGroup = overlay.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            RawImage rewardBurstDisplay = CreateRawImage(
                overlay.transform,
                "ui_reward_burst_render_texture",
                uiParticleAssets.renderTexture,
                uiParticleAssets.displayMaterial,
                Anchored(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1824f, 1824f)));
            rewardBurstDisplay.enabled = false;

            var contentObject = new GameObject("ui_outcome_content_root");
            contentObject.transform.SetParent(overlay.transform, false);
            var contentRect = contentObject.AddComponent<RectTransform>();
            Apply(contentRect, FullStretch());
            UiParticleRig rewardBurstRig = CreateUiParticleSceneRig(controllerObject.transform, settings, uiParticleAssets);

            CanvasGroup chromeGroup = BuildOutcomeChrome(contentObject.transform, material);
            TextMeshProUGUI title = CreateText(contentObject.transform, "ui_outcome_title_value", "REWARD WON", Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 182f), new Vector2(560f, 50f)), 38, TextAlignmentOptions.Center, settings.Theme.PrimaryTextColor);
            Image icon = CreateImage(contentObject.transform, "ui_outcome_icon_value", null, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(160f, 160f)), material);
            Image[] flightIconPool = CreateFlightIconPool(overlay.transform, material, 1);
            TextMeshProUGUI result = CreateText(contentObject.transform, "ui_outcome_result_value", "Reward", Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -160f), new Vector2(620f, 58f)), 31, TextAlignmentOptions.Center, settings.Theme.SecondaryTextColor);
            TextMeshProUGUI summary = CreateText(contentObject.transform, "ui_outcome_summary_value", "Flying into your reward stash", Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -206f), new Vector2(760f, 38f)), 21, TextAlignmentOptions.Center, settings.Theme.SecondaryTextColor);
            title.fontStyle = FontStyles.Bold;

            WheelOutcomePopupView view = controllerObject.AddComponent<WheelOutcomePopupView>();
            WheelEditorWiring.SetReference(view, "_root", overlay.gameObject);
            WheelEditorWiring.SetReference(view, "_iconImage", icon);
            WheelEditorWiring.SetReference(view, "_titleText", title);
            WheelEditorWiring.SetReference(view, "_resultText", result);
            WheelEditorWiring.SetReference(view, "_summaryText", summary);
            WheelEditorWiring.SetReference(view, "_canvasGroup", canvasGroup);
            WheelEditorWiring.SetReference(view, "_contentRoot", contentRect);
            WheelEditorWiring.SetReference(view, "_rewardChromeGroup", chromeGroup);
            WheelEditorWiring.SetObjectArray(view, "_flightIconPool", flightIconPool);
            WheelEditorWiring.SetReference(view, "_rewardBurstCamera", rewardBurstRig.camera);
            WheelEditorWiring.SetReference(view, "_rewardBurstDisplay", rewardBurstDisplay);
            WheelEditorWiring.SetReference(view, "_rewardBurstParticle", rewardBurstRig.particle);
            WheelEditorWiring.SetReference(view, "_rewardBurstRenderer", rewardBurstRig.renderer);
            overlay.gameObject.SetActive(false);
            return view;
        }

        private static CanvasGroup BuildOutcomeChrome(Transform parent, Material material)
        {
            var chromeObject = new GameObject("ui_outcome_reward_chrome");
            chromeObject.transform.SetParent(parent, false);
            var chromeRect = chromeObject.AddComponent<RectTransform>();
            Apply(chromeRect, FullStretch());
            CanvasGroup chromeGroup = chromeObject.AddComponent<CanvasGroup>();
            chromeGroup.interactable = false;
            chromeGroup.blocksRaycasts = false;
            chromeGroup.alpha = 0f;

            Sprite popupSprite = FindSprite("ui_card_frame_gardient");
            if (popupSprite == null)
            {
                popupSprite = FindSprite("ui_card_frame_12px_neutral");
            }

            Image popupBackground = CreateImage(chromeObject.transform, "ui_outcome_popup_bg", popupSprite, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(552f, 488f)), material);
            popupBackground.type = Image.Type.Sliced;
            popupBackground.color = new Color(0.92f, 0.66f, 0.28f, 0.88f);
            popupBackground.preserveAspect = false;

            WheelCircleGraphic halo = CreateCircleGraphic(chromeObject.transform, "ui_outcome_halo", Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(360f, 360f)), new Color(1f, 0.62f, 0.16f, 0.16f), 0f);
            halo.raycastTarget = false;

            return chromeGroup;
        }

        private static Image[] CreateFlightIconPool(Transform parent, Material material, int count)
        {
            var pool = new Image[count];
            for (int i = 0; i < count; i++)
            {
                Image icon = CreateImage(parent, "ui_reward_flight_icon_" + i, null, Anchored(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(72f, 72f)), material);
                icon.color = Color.clear;
                icon.gameObject.SetActive(false);
                pool[i] = icon;
            }

            return pool;
        }

        private static UiParticleRig CreateUiParticleSceneRig(Transform parent, WheelGameSettings settings, UiParticleAssets uiParticleAssets)
        {
            int layer = EnsureLayer(UiParticleLayerName);

            var root = new GameObject("UIParticleWorldRig");
            root.layer = layer;
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(2000f, 2000f, 0f);
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            Camera camera = CreateUiParticleCamera(root.transform, layer, uiParticleAssets.renderTexture);
            ParticleSystem particle = CreateUiParticleSystem(root.transform, layer, settings, uiParticleAssets.particleMaterial);
            var renderer = particle.GetComponent<ParticleSystemRenderer>();

            return new UiParticleRig
            {
                camera = camera,
                particle = particle,
                renderer = renderer
            };
        }

        private static Camera CreateUiParticleCamera(Transform parent, int layer, RenderTexture renderTexture)
        {
            var cameraObject = new GameObject("UIParticleCamera");
            cameraObject.layer = 0;
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.cullingMask = 1 << layer;
            camera.orthographic = true;
            camera.orthographicSize = renderTexture != null ? renderTexture.height * 0.5f : 256f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 30f;
            camera.depth = 40f;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.targetTexture = renderTexture;
            camera.enabled = false;
            return camera;
        }

        private static ParticleSystem CreateUiParticleSystem(Transform parent, int layer, WheelGameSettings settings, Material particleMaterial)
        {
            var particleObject = new GameObject("UIParticleRewardBurst");
            particleObject.layer = layer;
            particleObject.transform.SetParent(parent, false);
            particleObject.transform.localPosition = Vector3.zero;
            var particle = particleObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = particle.main;
            main.duration = 1.1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.72f, 1.24f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(230f, 420f);
            main.startSize = new ParticleSystem.MinMaxCurve(44f, 82f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.283185f);
            main.startColor = Color.white;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 64;
            main.playOnAwake = false;

            ParticleSystem.EmissionModule emission = particle.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 28) });

            ParticleSystem.ShapeModule shape = particle.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 8f;
            shape.arc = 360f;

            ParticleSystem.ColorOverLifetimeModule color = particle.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;

            ParticleSystem.SizeOverLifetimeModule size = particle.sizeOverLifetime;
            size.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.7f),
                new Keyframe(0.18f, 1f),
                new Keyframe(1f, 0f));
            size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            ParticleSystem.TextureSheetAnimationModule textureSheet = particle.textureSheetAnimation;
            textureSheet.enabled = true;
            textureSheet.mode = ParticleSystemAnimationMode.Sprites;
            textureSheet.timeMode = ParticleSystemAnimationTimeMode.Lifetime;
            textureSheet.fps = 1f;
            Sprite seedSprite = settings.BombReward.Icon != null ? settings.BombReward.Icon : settings.Layout.StarFlashSprite;
            if (seedSprite != null)
            {
                textureSheet.AddSprite(seedSprite);
            }

            var renderer = particle.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortingOrder = 0;
            renderer.material = particleMaterial;
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            return particle;
        }

        private static UiParticleAssets EnsureUiParticleAssets(WheelGameSettings settings)
        {
            if (!AssetDatabase.IsValidFolder(UiParticleFolder))
            {
                AssetDatabase.CreateFolder("Assets", "UIPrefab");
            }

            RenderTexture renderTexture = AssetDatabase.LoadAssetAtPath<RenderTexture>(UiParticleRenderTexturePath);
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                renderTexture.name = "UIParticleRenderTexture";
                renderTexture.antiAliasing = 1;
                renderTexture.useMipMap = false;
                renderTexture.autoGenerateMips = false;
                renderTexture.wrapMode = TextureWrapMode.Clamp;
                renderTexture.filterMode = FilterMode.Bilinear;
                AssetDatabase.CreateAsset(renderTexture, UiParticleRenderTexturePath);
            }
            else
            {
                renderTexture.width = 512;
                renderTexture.height = 512;
                renderTexture.depth = 16;
                renderTexture.format = RenderTextureFormat.ARGB32;
                renderTexture.antiAliasing = 1;
                renderTexture.useMipMap = false;
                renderTexture.autoGenerateMips = false;
                renderTexture.wrapMode = TextureWrapMode.Clamp;
                renderTexture.filterMode = FilterMode.Bilinear;
                EditorUtility.SetDirty(renderTexture);
            }

            Material particleMaterial = AssetDatabase.LoadAssetAtPath<Material>(UiParticleMaterialPath);
            if (particleMaterial == null)
            {
                Shader shader = Shader.Find("Particles/Additive");
                if (shader == null)
                {
                    shader = Shader.Find("Legacy Shaders/Particles/Additive");
                }

                if (shader == null)
                {
                    shader = Shader.Find("Sprites/Default");
                }

                particleMaterial = new Material(shader);
                particleMaterial.name = "UIParticle_Additive";
                if (settings.Layout.StarGlowSprite != null)
                {
                    particleMaterial.mainTexture = settings.Layout.StarGlowSprite.texture;
                }

                AssetDatabase.CreateAsset(particleMaterial, UiParticleMaterialPath);
            }

            Material displayMaterial = AssetDatabase.LoadAssetAtPath<Material>(UiParticleDisplayMaterialPath);
            AssetDatabase.ImportAsset(UiParticleDisplayShaderPath, ImportAssetOptions.ForceUpdate);
            Shader displayShader = AssetDatabase.LoadAssetAtPath<Shader>(UiParticleDisplayShaderPath);
            if (displayShader == null)
            {
                displayShader = Shader.Find("UI/Default");
            }

            if (displayMaterial == null)
            {
                displayMaterial = new Material(displayShader);
                displayMaterial.name = "UIParticle_Display";
                displayMaterial.mainTexture = renderTexture;
                if (displayMaterial.HasProperty("_BlackCutoff"))
                {
                    displayMaterial.SetFloat("_BlackCutoff", 0.015f);
                }

                if (displayMaterial.HasProperty("_AlphaSoftness"))
                {
                    displayMaterial.SetFloat("_AlphaSoftness", 0.12f);
                }

                AssetDatabase.CreateAsset(displayMaterial, UiParticleDisplayMaterialPath);
            }
            else
            {
                if (displayShader != null && displayMaterial.shader != displayShader)
                {
                    displayMaterial.shader = displayShader;
                }

                displayMaterial.mainTexture = renderTexture;
                if (displayMaterial.HasProperty("_BlackCutoff"))
                {
                    displayMaterial.SetFloat("_BlackCutoff", 0.015f);
                }

                if (displayMaterial.HasProperty("_AlphaSoftness"))
                {
                    displayMaterial.SetFloat("_AlphaSoftness", 0.12f);
                }

                EditorUtility.SetDirty(displayMaterial);
            }

            return new UiParticleAssets
            {
                renderTexture = renderTexture,
                particleMaterial = particleMaterial,
                displayMaterial = displayMaterial
            };
        }

        private static void EnsureUiParticlePrefab(WheelGameSettings settings, UiParticleAssets uiParticleAssets)
        {
            int layer = EnsureLayer(UiParticleLayerName);
            var root = new GameObject("UIParticlePrefab");
            root.layer = layer;
            root.transform.position = new Vector3(2000f, 2000f, 0f);

            Camera camera = CreateUiParticleCamera(root.transform, layer, uiParticleAssets.renderTexture);
            camera.enabled = true;
            CreateUiParticleSystem(root.transform, layer, settings, uiParticleAssets.particleMaterial);

            PrefabUtility.SaveAsPrefabAsset(root, UiParticlePrefabPath);
            Object.DestroyImmediate(root);
        }

        private static int EnsureLayer(string layerName)
        {
            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) == layerName)
                {
                    return i;
                }
            }

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 8; i < 32; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    tagManager.ApplyModifiedPropertiesWithoutUndo();
                    return i;
                }
            }

            return 5;
        }

        private static WheelExitConfirmationView BuildExitConfirmation(Transform parent, WheelGameSettings settings, Material material)
        {
            Image overlay = CreateImage(parent, "ui_exit_confirmation_overlay", null, FullStretch(), material);
            overlay.color = new Color(0f, 0f, 0f, 0.62f);
            overlay.preserveAspect = false;
            overlay.raycastTarget = true;
            CanvasGroup canvasGroup = overlay.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Image panel = CreateImage(
                overlay.transform,
                "ui_exit_confirmation_panel",
                settings.Layout.ZoneStandardPanelSprite,
                Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(760f, 390f)),
                material);
            panel.type = Image.Type.Sliced;
            panel.preserveAspect = false;
            panel.color = new Color(0.09f, 0.10f, 0.12f, 0.98f);
            panel.raycastTarget = true;

            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Image glow = CreateImage(panel.transform, "ui_exit_confirmation_glow", settings.Layout.StarGlowSprite, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 54f), new Vector2(560f, 220f)), material);
            glow.color = new Color(1f, 1f, 1f, 0.12f);
            glow.transform.SetAsFirstSibling();

            TextMeshProUGUI title = CreateText(
                panel.transform,
                "ui_exit_confirmation_title_value",
                "COLLECT REWARDS?",
                Anchored(new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(650f, 70f)),
                44,
                TextAlignmentOptions.Center,
                settings.Theme.PrimaryTextColor);
            title.fontStyle = FontStyles.Bold;

            TextMeshProUGUI body = CreateText(
                panel.transform,
                "ui_exit_confirmation_body_value",
                "Are you sure you want to go out and collect your rewards? We have saved the best rewards for last!",
                Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 18f), new Vector2(600f, 116f)),
                24,
                TextAlignmentOptions.Center,
                settings.Theme.SecondaryTextColor);

            Image shine = CreateImage(panel.transform, "ui_exit_confirmation_shine", settings.Layout.ShineSprite, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -82f), new Vector2(420f, 42f)), material);
            shine.color = new Color(1f, 1f, 1f, 0.18f);

            Button collectButton = CreateButton(
                panel.transform,
                "ui_button_exit_collect_rewards",
                "COLLECT REWARDS",
                Anchored(new Vector2(0.5f, 0f), new Vector2(-170f, 64f), new Vector2(300f, 78f)),
                FindSprite("UI_button_orange_standard"),
                material);
            Button comeBackButton = CreateButton(
                panel.transform,
                "ui_button_exit_come_back",
                "COME BACK",
                Anchored(new Vector2(0.5f, 0f), new Vector2(170f, 64f), new Vector2(260f, 78f)),
                FindSprite("UI_button_grey_standard"),
                material);

            WheelExitConfirmationView view = overlay.gameObject.AddComponent<WheelExitConfirmationView>();
            WheelEditorWiring.SetReference(view, "_root", overlay.gameObject);
            WheelEditorWiring.SetReference(view, "_canvasGroup", canvasGroup);
            WheelEditorWiring.SetReference(view, "_contentRoot", panelRect);
            WheelEditorWiring.SetReference(view, "_titleText", title);
            WheelEditorWiring.SetReference(view, "_bodyText", body);
            WheelEditorWiring.SetReference(view, "_collectButton", collectButton);
            WheelEditorWiring.SetReference(view, "_comeBackButton", comeBackButton);
            overlay.gameObject.SetActive(false);
            return view;
        }

        private static RewardOpeningViews BuildRewardOpening(Transform parent, WheelGameSettings settings, Material material)
        {
            var overlayObject = new GameObject("ui_reward_opening_overlay");
            overlayObject.transform.SetParent(parent, false);
            var overlayRect = overlayObject.AddComponent<RectTransform>();
            Apply(overlayRect, FullStretch());
            Image overlayImage = overlayObject.AddComponent<Image>();
            overlayImage.color = new Color(0.06f, 0.18f, 0.25f, 0.94f);
            overlayImage.raycastTarget = false;
            overlayImage.maskable = false;

            TextMeshProUGUI title = CreateText(
                overlayObject.transform,
                "ui_reward_opening_title_value",
                "YOUR REWARDS:",
                Anchored(new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(760f, 96f)),
                58,
                TextAlignmentOptions.Center,
                settings.Theme.PrimaryTextColor);
            title.fontStyle = FontStyles.Bold;

            var cardsRoot = new GameObject("ui_reward_opening_cards_root");
            cardsRoot.transform.SetParent(overlayObject.transform, false);
            var cardsRect = cardsRoot.AddComponent<RectTransform>();
            Apply(cardsRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(1780f, 550f)));
            CreateOpeningRewardCards(cardsRoot.transform, settings, material);

            Button restartButton = CreateButton(
                overlayObject.transform,
                "ui_button_reward_opening_restart",
                "PLAY AGAIN",
                Anchored(new Vector2(1f, 0f), new Vector2(-246f, 94f), new Vector2(300f, 92f)),
                FindSprite("UI_button_orange_standard"),
                material);
            WheelRestartButtonAction restart = restartButton.gameObject.AddComponent<WheelRestartButtonAction>();
            WheelEditorWiring.SetReference(restart, "_button", restartButton);

            WheelRewardOpeningView opening = overlayObject.AddComponent<WheelRewardOpeningView>();
            WheelEditorWiring.SetReference(opening, "_root", overlayObject);
            WheelEditorWiring.SetReference(opening, "_titleText", title);
            WheelEditorWiring.SetReference(opening, "_cardPoolRoot", cardsRoot.transform);
            WheelEditorWiring.CollectChildren(opening, "_rewardCards", cardsRoot.transform);
            WheelRewardCardView firstCard = cardsRoot.transform.GetChild(0).GetComponent<WheelRewardCardView>();
            WheelEditorWiring.SetReference(opening, "_cardFrameSource", firstCard.GetComponentInChildren<Image>());
            overlayObject.SetActive(false);

            return new RewardOpeningViews
            {
                opening = opening,
                restart = restart
            };
        }

        private static WheelRewardCardView[] CreateOpeningRewardCards(Transform parent, WheelGameSettings settings, Material material)
        {
            int cardCount = settings.Layout.MaxRewardCards;
            WheelRewardCardView[] cardViews = new WheelRewardCardView[cardCount];
            Vector2 cardSize = new Vector2(230f, 500f);
            float spacing = 34f;
            float rowWidth = (cardCount * cardSize.x) + ((cardCount - 1) * spacing);
            float startX = (-rowWidth * 0.5f) + (cardSize.x * 0.5f);

            for (int i = 0; i < cardCount; i++)
            {
                var cardObject = new GameObject("ui_reward_opening_card_" + i);
                cardObject.transform.SetParent(parent, false);
                var cardRect = cardObject.AddComponent<RectTransform>();
                Apply(cardRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(startX + (i * (cardSize.x + spacing)), 0f), cardSize));

                Image frame = CreateImage(cardObject.transform, "ui_reward_opening_card_" + i + "_frame", settings.Layout.RewardCardFrameSprite, FullStretch(), material);
                frame.type = Image.Type.Sliced;
                frame.preserveAspect = false;

                Image glow = CreateImage(cardObject.transform, "ui_reward_opening_card_" + i + "_glow", settings.Layout.StarGlowSprite, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(182f, 182f)), material);
                glow.color = new Color(1f, 1f, 1f, 0.16f);
                Image shine = CreateImage(cardObject.transform, "ui_reward_opening_card_" + i + "_shine", settings.Layout.ShineSprite, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -152f), new Vector2(162f, 30f)), material);
                shine.color = new Color(1f, 1f, 1f, 0.18f);

                Image icon = CreateImage(cardObject.transform, "ui_reward_opening_card_" + i + "_icon", null, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(166f, 166f)), material);
                TextMeshProUGUI amount = CreateText(cardObject.transform, "ui_reward_opening_card_" + i + "_amount", string.Empty, Anchored(new Vector2(0.5f, 0f), new Vector2(0f, 44f), new Vector2(170f, 42f)), 24, TextAlignmentOptions.Center, Color.white);
                amount.fontStyle = FontStyles.Bold;

                WheelRewardCardView cardView = cardObject.AddComponent<WheelRewardCardView>();
                WheelEditorWiring.SetReference(cardView, "_frameImage", frame);
                WheelEditorWiring.SetReference(cardView, "_iconImage", icon);
                WheelEditorWiring.SetReference(cardView, "_amountText", amount);
                cardObject.SetActive(false);
                cardViews[i] = cardView;
            }

            return cardViews;
        }

        private static WheelStatusTextView BuildFooter(Transform parent, WheelGameSettings settings)
        {
            TextMeshProUGUI text = CreateText(parent, "ui_footer_status_value", "Spin to risk your loot and advance toward safe zones.", Anchored(new Vector2(0.5f, 0f), settings.Layout.StatusPosition, settings.Layout.StatusSize), 24, TextAlignmentOptions.Center, settings.Theme.SecondaryTextColor);
            WheelStatusTextView view = text.gameObject.AddComponent<WheelStatusTextView>();
            WheelEditorWiring.SetReference(view, "_text", text);
            return view;
        }

        private static ButtonActions BuildButtons(Transform parent, Transform lootPanelRoot, WheelGameSettings settings, Material material)
        {
            Button leaveButton = CreateButton(
                lootPanelRoot,
                "ui_button_leave",
                "EXIT",
                Anchored(new Vector2(0.5f, 1f), new Vector2(0f, -58f), new Vector2(220f, 78f)),
                FindSprite("UI_button_grey_standard"),
                material);
            Button spinButton = CreateButton(
                parent,
                "ui_button_spin",
                "SPIN",
                Anchored(new Vector2(0.5f, 0.5f), settings.Layout.WheelPosition, new Vector2(196f, 196f)),
                FindSprite("ui_spin_generic_button"),
                material);
            Button restartButton = CreateButton(
                parent,
                "ui_button_restart",
                "RETRY",
                Anchored(new Vector2(0.5f, 0f), settings.Layout.ButtonRowPosition, settings.Layout.ButtonSize),
                FindSprite("UI_button_grey_standard"),
                material);
            WheelLeaveButtonAction leave = leaveButton.gameObject.AddComponent<WheelLeaveButtonAction>();
            WheelSpinButtonAction spin = spinButton.gameObject.AddComponent<WheelSpinButtonAction>();
            WheelRestartButtonAction restart = restartButton.gameObject.AddComponent<WheelRestartButtonAction>();
            WheelEditorWiring.SetReference(leave, "_button", leaveButton);
            WheelEditorWiring.SetReference(spin, "_button", spinButton);
            WheelEditorWiring.SetReference(restart, "_button", restartButton);
            return new ButtonActions
            {
                leave = leave,
                spin = spin,
                restart = restart
            };
        }

        private static WheelSliceView[] CreateSlicePool(Transform parent, int count, Material material)
        {
            var views = new WheelSliceView[count];
            for (int i = 0; i < count; i++)
            {
                var item = new GameObject("ui_wheel_slice_" + i + "_value");
                item.transform.SetParent(parent, false);
                var rect = item.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(92f, 92f);

                WheelCircleGraphic background = CreateCircleGraphic(
                    item.transform,
                    "ui_wheel_slice_" + i + "_medallion_bg",
                    Anchored(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(78f, 78f)),
                    new Color(0.04f, 0.055f, 0.065f, 0.92f),
                    0f);
                WheelCircleGraphic rim = CreateCircleGraphic(
                    item.transform,
                    "ui_wheel_slice_" + i + "_medallion_rim",
                    Anchored(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(86f, 86f)),
                    Color.white,
                    0.86f);
                background.gameObject.SetActive(false);
                rim.gameObject.SetActive(false);

                var iconObject = new GameObject("ui_wheel_slice_" + i + "_icon_value");
                iconObject.transform.SetParent(item.transform, false);
                var iconRect = iconObject.AddComponent<RectTransform>();
                Apply(iconRect, Anchored(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(68f, 68f)));
                var icon = iconObject.AddComponent<Image>();
                icon.material = material;
                icon.raycastTarget = false;
                icon.maskable = false;
                icon.preserveAspect = true;

                var badgeObject = new GameObject("ui_wheel_slice_" + i + "_amount_badge");
                badgeObject.transform.SetParent(item.transform, false);
                var badgeRect = badgeObject.AddComponent<RectTransform>();
                Apply(badgeRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(31f, 31f), new Vector2(32f, 32f)));

                WheelCircleGraphic badgeBackground = CreateCircleGraphic(
                    badgeObject.transform,
                    "ui_wheel_slice_" + i + "_amount_badge_bg",
                    FullStretch(),
                    new Color(0.025f, 0.03f, 0.035f, 0.96f),
                    0f);
                WheelCircleGraphic badgeRim = CreateCircleGraphic(
                    badgeObject.transform,
                    "ui_wheel_slice_" + i + "_amount_badge_rim",
                    FullStretch(),
                    Color.white,
                    0.78f);

                var labelObject = new GameObject("ui_wheel_slice_" + i + "_amount_value");
                labelObject.transform.SetParent(badgeObject.transform, false);
                var labelRect = labelObject.AddComponent<RectTransform>();
                Apply(labelRect, FullStretch());

                var label = labelObject.AddComponent<TextMeshProUGUI>();
                label.enableAutoSizing = true;
                label.fontSizeMax = 14f;
                label.fontSizeMin = 8f;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                label.fontStyle = FontStyles.Bold;
                label.outlineWidth = 0.12f;
                label.outlineColor = new Color32(3, 8, 12, 255);

                var view = item.AddComponent<WheelSliceView>();
                WheelEditorWiring.SetReference(view, "_root", rect);
                WheelEditorWiring.SetReference(view, "_iconRoot", iconRect);
                WheelEditorWiring.SetReference(view, "_icon", icon);
                WheelEditorWiring.SetReference(view, "_background", background);
                WheelEditorWiring.SetReference(view, "_rim", rim);
                WheelEditorWiring.SetReference(view, "_amountBadge", badgeObject);
                WheelEditorWiring.SetReference(view, "_amountBadgeBackground", badgeBackground);
                WheelEditorWiring.SetReference(view, "_amountBadgeRim", badgeRim);
                WheelEditorWiring.SetReference(view, "_amount", label);
                badgeObject.SetActive(false);
                views[i] = view;
            }

            return views;
        }

        private static WheelCircleGraphic CreateCircleGraphic(Transform parent, string name, RectSpec rectSpec, Color color, float innerRadius)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            var rect = item.AddComponent<RectTransform>();
            Apply(rect, rectSpec);
            item.AddComponent<CanvasRenderer>();
            WheelCircleGraphic graphic = item.AddComponent<WheelCircleGraphic>();
            graphic.color = color;
            graphic.raycastTarget = false;
            graphic.maskable = false;
            graphic.Configure(innerRadius);
            return graphic;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, Sprite sprite, Material material)
        {
            return CreateButton(parent, name, label, Anchored(new Vector2(0.5f, 0f), anchoredPosition, size), sprite, material);
        }

        private static Button CreateButton(Transform parent, string name, string label, RectSpec rectSpec, Sprite sprite, Material material)
        {
            Image image = CreateImage(parent, name, sprite, rectSpec, material);
            image.type = Image.Type.Sliced;
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            TextMeshProUGUI labelText = CreateText(image.transform, name + "_label_value", label, FullStretch(), 28, TextAlignmentOptions.Center, Color.white);
            labelText.fontStyle = FontStyles.Bold;
            return button;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string value, RectSpec rectSpec, int fontSize, TextAlignmentOptions alignment, Color color)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            var rect = item.AddComponent<RectTransform>();
            Apply(rect, rectSpec);
            var text = item.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.enableAutoSizing = true;
            text.fontSizeMax = fontSize;
            text.fontSizeMin = 12f;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, RectSpec rectSpec, Material material)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            var rect = item.AddComponent<RectTransform>();
            Apply(rect, rectSpec);
            var image = item.AddComponent<Image>();
            image.sprite = sprite;
            image.material = material;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.maskable = false;
            return image;
        }

        private static RawImage CreateRawImage(Transform parent, string name, Texture texture, Material material, RectSpec rectSpec)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            var rect = item.AddComponent<RectTransform>();
            Apply(rect, rectSpec);
            var image = item.AddComponent<RawImage>();
            image.texture = texture;
            image.material = material;
            image.color = Color.white;
            image.raycastTarget = false;
            image.maskable = false;
            return image;
        }

        private static RewardDefinition Reward(string id, string displayName, Sprite icon, int amount, RewardTier tier, Color accentColor)
        {
            return RewardDefinition.Create(id, displayName, icon, amount, tier, accentColor);
        }

        private static Sprite FindSprite(string name)
        {
            return VertigoWheelAssetPipeline.FindSprite(name);
        }

        private static RectSpec FullStretch()
        {
            return new RectSpec { min = Vector2.zero, max = Vector2.one, pivot = new Vector2(0.5f, 0.5f), position = Vector2.zero, size = Vector2.zero };
        }

        private static RectSpec Anchored(Vector2 anchor, Vector2 position, Vector2 size)
        {
            return new RectSpec { min = anchor, max = anchor, pivot = new Vector2(0.5f, 0.5f), position = position, size = size };
        }

        private static void Apply(RectTransform rect, RectSpec spec)
        {
            rect.anchorMin = spec.min;
            rect.anchorMax = spec.max;
            rect.pivot = spec.pivot;
            rect.anchoredPosition = spec.position;
            rect.sizeDelta = spec.size;
            if (spec.min == Vector2.zero && spec.max == Vector2.one)
            {
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        private struct RectSpec
        {
            public Vector2 min;
            public Vector2 max;
            public Vector2 pivot;
            public Vector2 position;
            public Vector2 size;
        }

        private struct UiParticleAssets
        {
            public RenderTexture renderTexture;
            public Material particleMaterial;
            public Material displayMaterial;
        }

        private struct UiParticleRig
        {
            public Camera camera;
            public ParticleSystem particle;
            public ParticleSystemRenderer renderer;
        }

        private struct HeaderViews
        {
            public WheelZoneProgressView zoneProgress;
            public WheelMilestoneBadgesView milestoneBadges;
            public WheelZonePanelView zonePanel;
            public WheelRewardPanelView lootPanel;
            public WheelZoneTextView zoneText;
            public WheelZoneTypeTextView zoneTypeText;
            public Transform lootRoot;
        }

        private struct RewardOpeningViews
        {
            public WheelRewardOpeningView opening;
            public WheelRestartButtonAction restart;
        }

        private struct ButtonActions
        {
            public WheelLeaveButtonAction leave;
            public WheelSpinButtonAction spin;
            public WheelRestartButtonAction restart;
        }
    }
}
#endif
