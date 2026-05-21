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

            var settingsObject = new GameObject("game_wheel_settings");
            var settings = settingsObject.AddComponent<WheelGameSettings>();
            settings.ResetToDefaults();
            settings.ConfigureUiCopy(uiCopyCatalog);
            settings.ConfigureSkinCatalog(skinCatalog);
            settings.ConfigureSliceLayoutCatalog(sliceLayoutCatalog);
            settings.ConfigureOutcomePopupMotionCatalog(outcomePopupMotionCatalog);
            settings.ConfigureSpinResolveCatalog(spinResolveCatalog);
            WheelEditorWiring.SetReference(settings, "_uiCopy", uiCopyCatalog);
            WheelEditorWiring.SetReference(settings, "_skinCatalog", skinCatalog);
            WheelEditorWiring.SetReference(settings, "_sliceLayoutCatalog", sliceLayoutCatalog);
            WheelEditorWiring.SetReference(settings, "_outcomePopupMotionCatalog", outcomePopupMotionCatalog);
            WheelEditorWiring.SetReference(settings, "_spinResolveCatalog", spinResolveCatalog);
            ConfigureDefaultRewards(settings);

            var gameObject = new GameObject("game_wheel_runtime");
            var gameState = gameObject.AddComponent<WheelGameState>();
            var publisher = gameObject.AddComponent<WheelStatePublisher>();
            var spinner = gameObject.AddComponent<WheelSpinner>();
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
            wheelRect.anchoredPosition = settings.Layout.wheelPosition;
            wheelRect.sizeDelta = settings.Layout.wheelSize;

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

            Image indicator = CreateImage(wheelCanvas, "ui_image_spin_indicator", FindSprite("ui_spin_silver_indicator"), Anchored(new Vector2(0.5f, 0.5f), settings.Layout.indicatorPosition, settings.Layout.indicatorSize), material);
            var indicatorSkinView = indicator.gameObject.AddComponent<WheelSkinView>();
            WheelEditorWiring.SetEnum(indicatorSkinView, "_slot", (int)WheelSkinSlot.Indicator);
            WheelEditorWiring.SetReference(indicatorSkinView, "_catalog", skinCatalog);
            WheelEditorWiring.SetReference(indicatorSkinView, "_image", indicator);

            var view = wheelRoot.AddComponent<WheelView>();
            WheelEditorWiring.SetObjectArray(view, "_sliceViews", sliceViews);

            HeaderViews headerViews = BuildHeader(hudCanvas, settings, material);
            WheelOutcomePopupView outcomePopupView = BuildOutcomePopup(hudCanvas, settings, material);
            RewardOpeningViews rewardOpeningViews = BuildRewardOpening(hudCanvas, settings, material);
            WheelStatusTextView statusTextView = BuildFooter(hudCanvas, settings);
            ButtonActions buttonActions = BuildButtons(hudCanvas, headerViews.lootRoot, settings, material);
            BuildDebugOverlay(debugCanvas, settings);

            WheelEditorWiring.SetReference(spinner, "_wheelTransform", wheelRect);
            WheelEditorWiring.SetReference(gameState, "_settings", settings);
            WheelEditorWiring.SetReference(publisher, "_state", gameState);
            WheelEditorWiring.SetReference(flowController, "_state", gameState);
            WheelEditorWiring.SetReference(flowController, "_spinner", spinner);
            WheelEditorWiring.SetReference(flowController, "_publisher", publisher);

            var compositionRoot = gameObject.AddComponent<WheelRuntimeCompositionRoot>();
            WheelEditorWiring.SetReference(compositionRoot, "_state", gameState);
            WheelEditorWiring.SetReference(compositionRoot, "_publisher", publisher);
            WheelEditorWiring.SetObjectArray(compositionRoot, "_runtimeComponents", new MonoBehaviour[]
            {
                publisher,
                backgroundView,
                wheelBaseSkinView,
                indicatorSkinView,
                view,
                headerViews.zoneProgress,
                headerViews.milestoneBadges,
                headerViews.zonePanel,
                headerViews.zoneText,
                headerViews.zoneTypeText,
                headerViews.lootPanel,
                outcomePopupView,
                rewardOpeningViews.opening,
                statusTextView,
                spinner,
                buttonActions.leave,
                buttonActions.spin,
                buttonActions.restart,
                rewardOpeningViews.restart,
                flowController
            });

            EditorSceneManager.SaveScene(scene, VertigoWheelPaths.ScenePath);
            VertigoWheelProjectSetup.ConfigureAndroidProject();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Transform CreateCanvas(string name, WheelGameSettings settings, int sortingOrder, bool raycasterEnabled)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = settings.Layout.referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            if (raycasterEnabled)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            return canvasObject.transform;
        }

        private static void BuildDebugOverlay(Transform parent, WheelGameSettings settings)
        {
            var monitor = parent.gameObject.AddComponent<WheelPerformanceMonitor>();
            TextMeshProUGUI text = CreateText(
                parent,
                "debug_text_performance",
                "PERF",
                Anchored(new Vector2(0f, 1f), new Vector2(168f, -92f), new Vector2(312f, 144f)),
                16,
                TextAlignmentOptions.TopLeft,
                settings.Theme.secondaryTextColor);
            text.enableAutoSizing = false;
            text.fontSize = 16f;
            var overlay = text.gameObject.AddComponent<WheelPerformanceOverlay>();
            WheelEditorWiring.SetReference(overlay, "_monitor", monitor);
            WheelEditorWiring.SetReference(overlay, "_text", text);
        }

        private static void ConfigureDefaultRewards(WheelGameSettings settings)
        {
            RewardDefinition bomb = settings.BombReward;
            bomb.id = "bomb";
            bomb.displayName = "Bomb";
            bomb.icon = FindSprite("ui_card_icon_death");
            bomb.amount = 1;
            bomb.tier = RewardTier.Legendary;
            bomb.accentColor = settings.Theme.dangerColor;

            settings.StandardRewards.Clear();
            settings.StandardRewards.Add(Reward("cash", "Cash", FindSprite("UI_icon_cash"), 150, RewardTier.Common, new Color(0.52f, 1f, 0.42f, 1f)));
            settings.StandardRewards.Add(Reward("gold", "Gold", FindSprite("UI_icon_gold"), 5, RewardTier.Rare, new Color(1f, 0.78f, 0.22f, 1f)));
            settings.StandardRewards.Add(Reward("bronze_chest", "Bronze Chest", FindSprite("UI_icon_chest_Bronze_nolight"), 1, RewardTier.Rare, new Color(0.88f, 0.55f, 0.24f, 1f)));
            settings.StandardRewards.Add(Reward("grenade_m26", "M26 Grenade", FindSprite("ui_icon_render_cons_grenade_m26"), 1, RewardTier.Common, Color.white));
            settings.StandardRewards.Add(Reward("molotov", "Molotov", FindSprite("ui_icon_render_t_cons_molotov"), 1, RewardTier.Common, Color.white));
            settings.StandardRewards.Add(Reward("grenade_m67", "M67 Grenade", FindSprite("ui_icon_render_cons_grenade_m67"), 1, RewardTier.Common, Color.white));
            settings.StandardRewards.Add(Reward("silver_chest", "Silver Chest", FindSprite("UI_icon_chest_silver_nolight"), 1, RewardTier.Rare, settings.Theme.safeZoneColor));

            settings.SafeRewards.Clear();
            settings.SafeRewards.Add(Reward("silver_chest", "Silver Chest", FindSprite("UI_icon_chest_silver_nolight"), 1, RewardTier.Rare, settings.Theme.safeZoneColor));
            settings.SafeRewards.Add(Reward("grenade_m67", "M67 Grenade", FindSprite("ui_icon_render_cons_grenade_m67"), 1, RewardTier.Common, Color.white));
            settings.SafeRewards.Add(Reward("neurostim", "Neurostim", FindSprite("ui_icon_render_cons_healthshot_2_neurostim"), 1, RewardTier.Common, Color.white));
            settings.SafeRewards.Add(Reward("cash_safe", "Cash", FindSprite("UI_icon_cash"), 300, RewardTier.Common, new Color(0.52f, 1f, 0.42f, 1f)));

            settings.SuperRewards.Clear();
            settings.SuperRewards.Add(Reward("gold_chest", "Gold Chest", FindSprite("UI_icon_chest_gold_nolight"), 1, RewardTier.Epic, settings.Theme.superZoneColor));
            settings.SuperRewards.Add(Reward("super_chest", "Super Chest", FindSprite("UI_icon_chest_super_nolight"), 1, RewardTier.Legendary, settings.Theme.superZoneColor));
            settings.SuperRewards.Add(Reward("big_chest", "Big Chest", FindSprite("UI_icon_chest_big_nolight"), 1, RewardTier.Epic, settings.Theme.superZoneColor));
            settings.SuperRewards.Add(Reward("gold", "Gold", FindSprite("UI_icon_gold"), 25, RewardTier.Legendary, settings.Theme.superZoneColor));

            WheelLayoutSettings layout = settings.Layout;
            layout.rewardPanelFrameSprite = FindSprite(layout.rewardPanelFrameSpriteName);
            layout.rewardCardFrameSprite = FindSprite(layout.rewardCardFrameSpriteName);
        }

        private static HeaderViews BuildHeader(Transform parent, WheelGameSettings settings, Material material)
        {
            WheelLayoutSettings layout = settings.Layout;
            WheelZoneProgressView zoneProgress = BuildZoneProgressRow(parent, settings, material);
            WheelMilestoneBadgesView milestoneBadges = BuildMilestoneBadges(parent, settings, material);
            WheelZonePanelView zonePanelView = BuildZonePanel(parent, settings, layout, material);
            WheelRewardPanelView lootPanelView = BuildLootStrip(parent, settings, layout, material);
            TextMeshProUGUI zoneText = CreateText(
                zonePanelView.transform,
                "ui_header_zone_value",
                "ZONE 1",
                Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(720f, 36f)),
                26,
                TextAlignmentOptions.Center,
                settings.Theme.primaryTextColor);
            TextMeshProUGUI zoneTypeText = CreateText(
                zonePanelView.transform,
                "ui_header_zone_type_value",
                "STANDARD",
                Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(720f, 24f)),
                16,
                TextAlignmentOptions.Center,
                settings.Theme.standardZoneColor);
            zoneText.fontStyle = FontStyles.Bold;
            zoneTypeText.fontStyle = FontStyles.Bold;
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
            var progressRoot = new GameObject("ui_zone_progress_root");
            progressRoot.transform.SetParent(parent, false);
            var progressRect = progressRoot.AddComponent<RectTransform>();
            Apply(progressRect, Anchored(new Vector2(0.5f, 1f), new Vector2(0f, -54f), new Vector2(920f, 70f)));

            int cellCount = 11;
            Image[] cellImages = new Image[cellCount];
            TextMeshProUGUI[] cellLabels = new TextMeshProUGUI[cellCount];
            float spacing = 78f;
            float startX = -spacing * ((cellCount - 1) * 0.5f);
            for (int i = 0; i < cellCount; i++)
            {
                var cellRoot = new GameObject("ui_zone_progress_cell_" + i);
                cellRoot.transform.SetParent(progressRoot.transform, false);
                var cellRect = cellRoot.AddComponent<RectTransform>();
                Apply(cellRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(startX + (i * spacing), 0f), new Vector2(62f, 62f)));

                Image cellImage = cellRoot.AddComponent<Image>();
                cellImage.sprite = FindSprite("ui_card_panel_zone_white");
                cellImage.material = material;
                cellImage.type = Image.Type.Sliced;
                cellImage.raycastTarget = false;
                cellImage.maskable = false;

                TextMeshProUGUI label = CreateText(cellRoot.transform, "ui_zone_progress_cell_" + i + "_value", "1", FullStretch(), 24, TextAlignmentOptions.Center, settings.Theme.secondaryTextColor);
                label.fontStyle = FontStyles.Bold;
                cellImages[i] = cellImage;
                cellLabels[i] = label;
            }

            WheelZoneProgressView view = progressRoot.AddComponent<WheelZoneProgressView>();
            WheelEditorWiring.SetObjectArray(view, "_cellImages", cellImages);
            WheelEditorWiring.SetObjectArray(view, "_cellLabels", cellLabels);
            WheelEditorWiring.SetReference(view, "_standardSprite", FindSprite("ui_card_panel_zone_white"));
            WheelEditorWiring.SetReference(view, "_currentSprite", FindSprite("ui_card_panel_zone_current_white"));
            WheelEditorWiring.SetReference(view, "_safeSprite", FindSprite("ui_card_panel_zone_current"));
            WheelEditorWiring.SetReference(view, "_superSprite", FindSprite("ui_card_panel_zone_super"));
            return view;
        }

        private static WheelMilestoneBadgesView BuildMilestoneBadges(Transform parent, WheelGameSettings settings, Material material)
        {
            var badgesRoot = new GameObject("ui_zone_milestone_badges_root");
            badgesRoot.transform.SetParent(parent, false);
            var badgesRect = badgesRoot.AddComponent<RectTransform>();
            Apply(badgesRect, Anchored(new Vector2(1f, 0.5f), new Vector2(-258f, 178f), new Vector2(328f, 244f)));

            Image superBadge = CreateMilestoneBadge(badgesRoot.transform, "ui_zone_badge_super", new Vector2(0f, 56f), new Color(0.2f, 0.65f, 0.08f, 0.86f), material);
            Image safeBadge = CreateMilestoneBadge(badgesRoot.transform, "ui_zone_badge_safe", new Vector2(0f, -56f), new Color(0.2f, 0.74f, 0.92f, 0.78f), material);
            TextMeshProUGUI superText = CreateText(superBadge.transform, "ui_zone_badge_super_value", "SUPER\nZONE\n30", FullStretch(), 30, TextAlignmentOptions.Center, settings.Theme.superZoneColor);
            TextMeshProUGUI safeText = CreateText(safeBadge.transform, "ui_zone_badge_safe_value", "SAFE\nZONE\n5", FullStretch(), 30, TextAlignmentOptions.Center, settings.Theme.safeZoneColor);
            superText.fontStyle = FontStyles.Bold;
            safeText.fontStyle = FontStyles.Bold;

            WheelMilestoneBadgesView view = badgesRoot.AddComponent<WheelMilestoneBadgesView>();
            WheelEditorWiring.SetReference(view, "_superBadgeImage", superBadge);
            WheelEditorWiring.SetReference(view, "_safeBadgeImage", safeBadge);
            WheelEditorWiring.SetReference(view, "_superBadgeText", superText);
            WheelEditorWiring.SetReference(view, "_safeBadgeText", safeText);
            return view;
        }

        private static Image CreateMilestoneBadge(Transform parent, string name, Vector2 position, Color color, Material material)
        {
            Image image = CreateImage(parent, name, FindSprite("ui_card_panel_zone_current"), Anchored(new Vector2(0.5f, 0.5f), position, new Vector2(306f, 92f)), material);
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = color;
            return image;
        }

        private static WheelZonePanelView BuildZonePanel(Transform parent, WheelGameSettings settings, WheelLayoutSettings layout, Material material)
        {
            var zonePanelRoot = new GameObject("ui_zone_panel_root");
            zonePanelRoot.transform.SetParent(parent, false);
            var zonePanelRect = zonePanelRoot.AddComponent<RectTransform>();
            Apply(zonePanelRect, Anchored(new Vector2(0.5f, 1f), layout.zonePanelPosition, layout.zonePanelSize));

            Image panelBg = CreateImage(zonePanelRoot.transform, "ui_zone_panel_bg", null, FullStretch(), material);
            panelBg.type = Image.Type.Sliced;
            panelBg.preserveAspect = false;

            Image mapFrame = CreateImage(zonePanelRoot.transform, "ui_zone_map_frame", null, FullStretch(), material);
            mapFrame.type = Image.Type.Sliced;
            mapFrame.preserveAspect = false;
            var mapFrameRect = mapFrame.rectTransform;
            mapFrameRect.offsetMin = new Vector2(10f, 8f);
            mapFrameRect.offsetMax = new Vector2(-10f, -8f);

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
            Apply(lootRect, Anchored(new Vector2(0.5f, 0.5f), layout.rewardPanelPosition, layout.rewardPanelSize));

            Image stripFrame = CreateImage(lootRoot.transform, "ui_loot_strip_frame", layout.rewardPanelFrameSprite, FullStretch(), material);
            stripFrame.type = Image.Type.Sliced;
            stripFrame.preserveAspect = false;

            var cardsRoot = new GameObject("ui_loot_cards_row");
            cardsRoot.transform.SetParent(lootRoot.transform, false);
            var cardsRect = cardsRoot.AddComponent<RectTransform>();
            Apply(cardsRect, FullStretch());
            cardsRect.offsetMin = new Vector2(24f, 24f);
            cardsRect.offsetMax = new Vector2(-24f, -112f);

            WheelRewardCardView[] cardViews = CreateRewardCardPool(cardsRoot.transform, layout, material);
            WheelRewardPanelView lootPanelView = lootRoot.AddComponent<WheelRewardPanelView>();
            WheelEditorWiring.SetReference(lootPanelView, "_panelFrameImage", stripFrame);
            WheelEditorWiring.SetObjectArray(lootPanelView, "_cardViews", cardViews);
            return lootPanelView;
        }

        private static WheelRewardCardView[] CreateRewardCardPool(Transform parent, WheelLayoutSettings layout, Material material)
        {
            int cardCount = layout.maxRewardCards;
            var cardViews = new WheelRewardCardView[cardCount];
            float cardWidth = layout.rewardCardSize.x;
            float cardHeight = layout.rewardCardSize.y;
            float spacing = 12f;
            bool vertical = layout.rewardPanelSize.y > layout.rewardPanelSize.x;
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
                Apply(cardRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(x, y), layout.rewardCardSize));

                Image frame = CreateImage(cardObject.transform, "ui_loot_card_" + i + "_frame", layout.rewardCardFrameSprite, FullStretch(), material);
                frame.type = Image.Type.Sliced;
                frame.preserveAspect = false;

                Image icon = CreateImage(
                    cardObject.transform,
                    "ui_loot_card_" + i + "_icon",
                    null,
                    Anchored(new Vector2(0.5f, 0.5f), ResolveRewardCardIconPosition(vertical), ResolveRewardCardIconSize(cardWidth, cardHeight, vertical)),
                    material);

                TextMeshProUGUI amount = CreateText(
                    cardObject.transform,
                    "ui_loot_card_" + i + "_amount",
                    string.Empty,
                    Anchored(new Vector2(0.5f, 0.5f), ResolveRewardCardAmountPosition(vertical), ResolveRewardCardAmountSize(cardWidth, cardHeight, vertical)),
                    18,
                    TextAlignmentOptions.Center,
                    Color.white);
                amount.fontStyle = FontStyles.Bold;
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

        private static WheelOutcomePopupView BuildOutcomePopup(Transform parent, WheelGameSettings settings, Material material)
        {
            Image overlay = CreateImage(parent, "ui_outcome_overlay", null, FullStretch(), material);
            overlay.color = new Color(0f, 0f, 0f, 0.68f);
            overlay.preserveAspect = false;
            CanvasGroup canvasGroup = overlay.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var contentObject = new GameObject("ui_outcome_content_root");
            contentObject.transform.SetParent(overlay.transform, false);
            var contentRect = contentObject.AddComponent<RectTransform>();
            Apply(contentRect, FullStretch());

            TextMeshProUGUI title = CreateText(contentObject.transform, "ui_outcome_title_value", "REWARD", Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 154f), new Vector2(620f, 68f)), 48, TextAlignmentOptions.Center, settings.Theme.primaryTextColor);
            Image icon = CreateImage(contentObject.transform, "ui_outcome_icon_value", null, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(160f, 160f)), material);
            TextMeshProUGUI result = CreateText(contentObject.transform, "ui_outcome_result_value", "Reward", Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -104f), new Vector2(760f, 62f)), 36, TextAlignmentOptions.Center, settings.Theme.secondaryTextColor);
            TextMeshProUGUI summary = CreateText(contentObject.transform, "ui_outcome_summary_value", "Added to your run rewards", Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, -158f), new Vector2(760f, 40f)), 22, TextAlignmentOptions.Center, settings.Theme.secondaryTextColor);
            title.fontStyle = FontStyles.Bold;

            WheelOutcomePopupView view = overlay.gameObject.AddComponent<WheelOutcomePopupView>();
            WheelEditorWiring.SetReference(view, "_root", overlay.gameObject);
            WheelEditorWiring.SetReference(view, "_iconImage", icon);
            WheelEditorWiring.SetReference(view, "_titleText", title);
            WheelEditorWiring.SetReference(view, "_resultText", result);
            WheelEditorWiring.SetReference(view, "_summaryText", summary);
            WheelEditorWiring.SetReference(view, "_canvasGroup", canvasGroup);
            WheelEditorWiring.SetReference(view, "_contentRoot", contentRect);
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
                settings.Theme.primaryTextColor);
            title.fontStyle = FontStyles.Bold;

            var cardsRoot = new GameObject("ui_reward_opening_cards_root");
            cardsRoot.transform.SetParent(overlayObject.transform, false);
            var cardsRect = cardsRoot.AddComponent<RectTransform>();
            Apply(cardsRect, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 12f), new Vector2(1780f, 550f)));
            WheelRewardCardView[] cardViews = CreateOpeningRewardCards(cardsRoot.transform, settings, material);

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
            WheelEditorWiring.SetReference(opening, "_cardFrameSource", cardViews[0].GetComponentInChildren<Image>());
            WheelEditorWiring.SetObjectArray(opening, "_cardViews", cardViews);
            overlayObject.SetActive(false);

            return new RewardOpeningViews
            {
                opening = opening,
                restart = restart
            };
        }

        private static WheelRewardCardView[] CreateOpeningRewardCards(Transform parent, WheelGameSettings settings, Material material)
        {
            int cardCount = settings.Layout.maxRewardCards;
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

                Image frame = CreateImage(cardObject.transform, "ui_reward_opening_card_" + i + "_frame", settings.Layout.rewardCardFrameSprite, FullStretch(), material);
                frame.type = Image.Type.Sliced;
                frame.preserveAspect = false;

                Image glow = CreateImage(cardObject.transform, "ui_reward_opening_card_" + i + "_glow", null, Anchored(new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(182f, 182f)), material);
                glow.color = new Color(1f, 1f, 1f, 0.12f);

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
            TextMeshProUGUI text = CreateText(parent, "ui_footer_status_value", "Spin to risk your loot and advance toward safe zones.", Anchored(new Vector2(0.5f, 0f), settings.Layout.statusPosition, settings.Layout.statusSize), 20, TextAlignmentOptions.Center, settings.Theme.secondaryTextColor);
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
                Anchored(new Vector2(0.5f, 0.5f), settings.Layout.wheelPosition, new Vector2(170f, 170f)),
                FindSprite("ui_spin_generic_button"),
                material);
            Button restartButton = CreateButton(
                parent,
                "ui_button_restart",
                "RETRY",
                Anchored(new Vector2(0.5f, 0f), settings.Layout.buttonRowPosition, settings.Layout.buttonSize),
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
                rect.sizeDelta = new Vector2(52f, 52f);

                var icon = item.AddComponent<Image>();
                icon.material = material;
                icon.raycastTarget = false;
                icon.maskable = false;
                icon.preserveAspect = true;

                var labelObject = new GameObject("ui_wheel_slice_" + i + "_amount_value");
                labelObject.transform.SetParent(item.transform, false);
                var labelRect = labelObject.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0f);
                labelRect.pivot = new Vector2(0.5f, 1f);
                labelRect.anchoredPosition = new Vector2(0f, -6f);
                labelRect.sizeDelta = new Vector2(50f, 24f);

                var label = labelObject.AddComponent<TextMeshProUGUI>();
                label.fontSize = 16f;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                label.fontStyle = FontStyles.Bold;

                var view = item.AddComponent<WheelSliceView>();
                WheelEditorWiring.SetReference(view, "_root", rect);
                WheelEditorWiring.SetReference(view, "_icon", icon);
                WheelEditorWiring.SetReference(view, "_amount", label);
                views[i] = view;
            }

            return views;
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
            TextMeshProUGUI labelText = CreateText(image.transform, name + "_label_value", label, FullStretch(), 25, TextAlignmentOptions.Center, Color.white);
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

        private static RewardDefinition Reward(string id, string displayName, Sprite icon, int amount, RewardTier tier, Color accentColor)
        {
            return new RewardDefinition
            {
                id = id,
                displayName = displayName,
                icon = icon,
                amount = amount,
                tier = tier,
                accentColor = accentColor
            };
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
