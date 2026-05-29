#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.EditorTools
{
    public sealed class VertigoWheelDesignerWindow : EditorWindow
    {
        private const float CardSpacing = 10f;
        private const float CardPadding = 12f;

        private enum DesignerTab
        {
            Overview,
            Gameplay,
            Rewards,
            Visuals,
            AdvancedAssets
        }

        private readonly string[] _tabLabels =
        {
            "Overview",
            "Gameplay",
            "Rewards",
            "Visuals",
            "Advanced"
        };

        private WheelGameSettings _settings;
        private WheelUiCopyCatalog _uiCopy;
        private WheelSkinCatalog _skinCatalog;
        private WheelOutcomePopupMotionCatalog _popupMotionCatalog;
        private WheelSpinResolveCatalog _spinResolveCatalog;

        private SerializedObject _settingsObject;
        private SerializedObject _uiCopyObject;
        private SerializedObject _skinObject;
        private SerializedObject _popupMotionObject;
        private SerializedObject _spinResolveObject;

        private DesignerTab _selectedTab;
        private Vector2 _scrollPosition;
        private bool _showAdvancedSettings;
        private bool _showAdvancedUiCopy;
        private bool _showAdvancedSkins;
        private bool _showAdvancedPopupMotion;
        private bool _showAdvancedResolveRules;

        private GUIStyle _headerTitleStyle;
        private GUIStyle _headerSubtitleStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _cardTitleStyle;
        private GUIStyle _pillStyle;
        private GUIStyle _mutedLabelStyle;
        private GUIStyle _sectionHintStyle;
        private GUIStyle _primaryButtonStyle;
        private GUIStyle _secondaryButtonStyle;
        private GUIStyle _statusOkStyle;
        private GUIStyle _statusMissingStyle;
        private Texture2D _cardTexture;
        private Texture2D _fieldTexture;
        private Texture2D _primaryButtonTexture;
        private Texture2D _secondaryButtonTexture;
        private Texture2D _okTexture;
        private Texture2D _missingTexture;

        [MenuItem("Vertigo Case/Wheel Designer")]
        public static void Open()
        {
            VertigoWheelDesignerWindow window = GetWindow<VertigoWheelDesignerWindow>("Wheel Designer");
            window.minSize = new Vector2(560f, 680f);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Wheel Designer");
            minSize = new Vector2(560f, 680f);
            ResolveAssets(true);
        }

        private void OnDisable()
        {
            DestroyCachedTextures();
        }

        private void OnFocus()
        {
            ResolveAssets(true);
        }

        private void OnGUI()
        {
            float previousLabelWidth = EditorGUIUtility.labelWidth;
            bool previousEnabled = GUI.enabled;
            Color previousColor = GUI.color;
            Color previousContentColor = GUI.contentColor;
            Color previousBackgroundColor = GUI.backgroundColor;
            try
            {
                GUI.enabled = true;
                GUI.color = Color.white;
                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.white;

                BuildStyles();
                ResolveAssets(false);

                EditorGUIUtility.labelWidth = Mathf.Clamp(position.width * 0.30f, 128f, 190f);

                DrawHeader();

                if (_settings == null)
                {
                    DrawCard("Missing Wheel Settings", "Stored in: Assets/Config/WheelGameSettings.asset", "The designer needs a main settings asset before it can edit the case.", () =>
                    {
                        if (GUILayout.Button("Create / Repair Project Assets", _primaryButtonStyle, GUILayout.Height(34f)))
                        {
                            VertigoWheelBootstrapper.EnsureProjectAssets();
                            ResolveAssets(true);
                        }
                    });
                    return;
                }

                int nextTab = GUILayout.Toolbar((int)_selectedTab, _tabLabels, GUILayout.Height(32f));
                if (nextTab != (int)_selectedTab)
                {
                    _selectedTab = (DesignerTab)nextTab;
                    _scrollPosition = Vector2.zero;
                    GUI.FocusControl(null);
                }

                UpdateSerializedObjects();

                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
                switch (_selectedTab)
                {
                    case DesignerTab.Overview:
                        DrawOverviewTab();
                        break;
                    case DesignerTab.Gameplay:
                        DrawGameplayTab();
                        break;
                    case DesignerTab.Rewards:
                        DrawRewardsTab();
                        break;
                    case DesignerTab.Visuals:
                        DrawVisualsTab();
                        break;
                    case DesignerTab.AdvancedAssets:
                        DrawAdvancedAssetsTab();
                        break;
                }

                EditorGUILayout.Space(14f);
                GUILayout.EndScrollView();

                ApplySerializedObjects();
            }
            finally
            {
                EditorGUIUtility.labelWidth = previousLabelWidth;
                GUI.enabled = previousEnabled;
                GUI.color = previousColor;
                GUI.contentColor = previousContentColor;
                GUI.backgroundColor = previousBackgroundColor;
            }
        }

        private void DrawHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 116f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.07f, 0.12f, 0.15f, 1f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 4f, rect.width, 4f), new Color(0.12f, 0.62f, 0.76f, 1f));

            Rect content = new Rect(rect.x + 18f, rect.y + 16f, rect.width - 36f, rect.height - 26f);
            GUI.Label(new Rect(content.x, content.y, content.width, 30f), "Vertigo Wheel Designer", _headerTitleStyle);
            GUI.Label(new Rect(content.x, content.y + 32f, content.width, 24f), "Guided dashboard for gameplay, rewards, visuals and linked ScriptableObjects.", _headerSubtitleStyle);
            GUI.Label(new Rect(content.x, content.y + 66f, 146f, 24f), "20:9 / 16:9 / 4:3", _pillStyle);
            GUI.Label(new Rect(content.x + 156f, content.y + 66f, Mathf.Max(170f, content.width - 156f), 24f), "Main asset: WheelGameSettings.asset", _pillStyle);
        }

        private void DrawOverviewTab()
        {
            DrawMetricGrid();

            DrawCard("Quick Actions", "Stored in: editor tools", "Use these after changing project assets.", () =>
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Apply Android Setup", _secondaryButtonStyle, GUILayout.Height(34f)))
                    {
                        RunAfterGui(() =>
                        {
                            VertigoWheelBootstrapper.ApplyAndroidCaseSetup();
                            ResolveAssets(true);
                        });
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Repair Linked Assets", _secondaryButtonStyle, GUILayout.Height(28f)))
                    {
                        RunAfterGui(() =>
                        {
                            VertigoWheelBootstrapper.EnsureProjectAssets();
                            ResolveAssets(true);
                        });
                    }

                    if (GUILayout.Button("Select Main Settings", _secondaryButtonStyle, GUILayout.Height(28f))) SelectObject(_settings);
                }
            });

            DrawCard("Play Mode Commands", "Runtime debug only", "Available while the scene is playing. These commands push deterministic reward states through the same runtime publisher used by gameplay.", () =>
            {
                if (!Application.isPlaying) EditorGUILayout.HelpBox("Enter Play Mode to use reward-panel and force-outcome commands.", MessageType.Info);
                else if (!WheelRuntimeLocator.IsReady)
                {
                    EditorGUILayout.HelpBox("Wheel runtime is not ready yet.", MessageType.Warning);
                }

                using (new EditorGUI.DisabledScope(!VertigoWheelDesignerPlayModeCommands.CanRun))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Reward Panel: 15 Loot", _secondaryButtonStyle, GUILayout.Height(28f))) RunAfterGui(VertigoWheelDesignerPlayModeCommands.AddFifteenLootToRewardPanel);

                        if (GUILayout.Button("Open 15 Cards", _secondaryButtonStyle, GUILayout.Height(28f))) RunAfterGui(VertigoWheelDesignerPlayModeCommands.OpenFifteenRewardCards);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Force Bomb", _secondaryButtonStyle, GUILayout.Height(28f))) RunAfterGui(VertigoWheelDesignerPlayModeCommands.ForceBombOutcome);

                        if (GUILayout.Button("Force Random Product", _secondaryButtonStyle, GUILayout.Height(28f))) RunAfterGui(VertigoWheelDesignerPlayModeCommands.ForceRandomProductOutcome);
                    }
                }
            });

            DrawCard("Linked Asset Health", "Stored in: Assets/Config", "These assets stay separate, but this dashboard edits their curated fields.", () =>
            {
                DrawAssetStatusRow("Main Settings", _settings, "Zone progression, rewards, colors and layout.");
                DrawAssetStatusRow("Zone Texts", _uiCopy, "Zone labels, status hints and outcome popup text.");
                DrawAssetStatusRow("Wheel Skins", _skinCatalog, "Bronze, silver and golden wheel/indicator sprites.");
                DrawAssetStatusRow("Popup Motion", _popupMotionCatalog, "Outcome popup fade, scale and ease.");
                DrawAssetStatusRow("Resolve Rules", _spinResolveCatalog, "What a reward or bomb hit does to state.");
            });

            DrawCard("What This Window Controls", "Dashboard guide", "Use this map instead of reading raw ScriptableObject names.", () =>
            {
                DrawGuideRow("Gameplay", "Zone cadence, bomb slot behavior and spin timing.");
                DrawGuideRow("Rewards", "Reward pools shown on wheel slices plus the bomb result data.");
                DrawGuideRow("Visuals", "Colors, screen layout, wheel skins, zone text and popup motion.");
                DrawGuideRow("Advanced", "Raw linked assets for debugging or uncommon fields.");
            });
        }

        private void DrawGameplayTab()
        {
            DrawCard("Zone Progression", "Stored in: WheelGameSettings.asset", "Defines when a zone becomes Standard, Safe or Super.", () =>
            {
                DrawProperty(_settingsObject, "_sliceCount", "Wheel Slice Count");
                DrawProperty(_settingsObject, "_safeZoneInterval", "Safe Zone Every");
                DrawProperty(_settingsObject, "_superZoneInterval", "Super Zone Every");
            });

            DrawCard("Zone Behavior", "Stored in: WheelGameSettings.asset", "This is behavior, not reward content. It decides whether the zone has a bomb slot and whether the player can leave.", () =>
            {
                DrawZoneBehavior("Standard Zone", 0);
                DrawZoneBehavior("Safe Zone", 1);
                DrawZoneBehavior("Super Zone", 2);
            });

            DrawCard("Spin Motion", "Stored in: WheelGameSettings.asset", "Controls the baseline wheel spin duration and easing.", () =>
            {
                DrawProperty(_settingsObject, "_spinDuration", "Spin Duration");
                DrawProperty(_settingsObject, "_minimumSpinRounds", "Minimum Spin Rounds");
                DrawProperty(_settingsObject, "_spinEase", "Spin Ease");
            });

            DrawCard("Resolve Rules", "Stored in: WheelSpinResolveCatalog.asset", "Controls what happens after the wheel resolves a reward or bomb.", () =>
            {
                SerializedProperty profiles = FindProperty(_spinResolveObject, "_profilesByOutcome");
                DrawResolveProfile(profiles, 0, "Reward Hit");
                DrawResolveProfile(profiles, 1, "Bomb Hit");
            });
        }

        private void DrawRewardsTab()
        {
            DrawCard("Bomb Result Reward", "Stored in: WheelGameSettings.asset", "This is the bomb outcome data shown when the player hits a bomb. Bomb slot behavior lives in Gameplay.", () =>
            {
                DrawProperty(_settingsObject, "_bombReward", "Bomb Result Reward", true);
            });

            DrawCard("Standard Rewards", "Stored in: WheelGameSettings.asset", "Rewards used by Standard zones.", () =>
            {
                DrawProperty(_settingsObject, "_standardRewards", "Standard Reward Pool", true);
            });

            DrawCard("Safe Rewards", "Stored in: WheelGameSettings.asset", "Rewards used by Safe zones.", () =>
            {
                DrawProperty(_settingsObject, "_safeRewards", "Safe Reward Pool", true);
            });

            DrawCard("Super Rewards", "Stored in: WheelGameSettings.asset", "Rewards used by Super zones.", () =>
            {
                DrawProperty(_settingsObject, "_superRewards", "Super Reward Pool", true);
            });
        }

        private void DrawVisualsTab()
        {
            DrawCard("Colors", "Stored in: WheelGameSettings.asset", "Controls core UI colors used by zone labels, popup text, badges and background.", () =>
            {
                SerializedProperty theme = FindProperty(_settingsObject, "_theme");
                DrawRelative(theme, "_backgroundColor", "Background");
                DrawRelative(theme, "_primaryTextColor", "Primary Text");
                DrawRelative(theme, "_secondaryTextColor", "Secondary Text");
                DrawRelative(theme, "_standardZoneColor", "Standard Zone");
                DrawRelative(theme, "_safeZoneColor", "Safe Zone");
                DrawRelative(theme, "_superZoneColor", "Super Zone");
                DrawRelative(theme, "_safeMilestoneBadgeBackground", "Safe Badge");
                DrawRelative(theme, "_superMilestoneBadgeBackground", "Super Badge");
                DrawRelative(theme, "_dangerColor", "Danger");
                DrawRelative(theme, "_successColor", "Success");
            });

            DrawCard("Runtime UI Sprites", "Stored in: WheelGameSettings.asset", "Sprites from this block are read by runtime reward panels. Scene RectTransforms own canvas positions and sizes.", () =>
            {
                SerializedProperty layout = FindProperty(_settingsObject, "_layout");
                DrawRelative(layout, "_rewardCardFrameSprite", "Reward Card Frame");
            });

            DrawCard("Zone Texts", "Stored in: WheelUiCopyCatalog.asset", "Controls labels, status hints, zone colors and which wheel skin tier each zone uses.", () =>
            {
                DrawProperty(_uiCopyObject, "_winLabelFormat", "Win Label Format");
                DrawProperty(_uiCopyObject, "_safeMilestoneBadgeFormat", "Safe Badge Format");
                DrawProperty(_uiCopyObject, "_superMilestoneBadgeFormat", "Super Badge Format");
                DrawZoneTextRows();
            });

            DrawCard("Panel Texts", "Stored in: WheelUiCopyCatalog.asset", "Controls baked panel and button copy applied at runtime.", () =>
            {
                DrawProperty(_uiCopyObject, "_spinButtonLabel", "Spin Button");
                DrawProperty(_uiCopyObject, "_leaveButtonLabel", "Leave Button");
                DrawProperty(_uiCopyObject, "_restartButtonLabel", "Restart Button");
                DrawProperty(_uiCopyObject, "_rewardOpeningTitle", "Reward Opening Title");
                DrawProperty(_uiCopyObject, "_defaultRewardTitle", "Default Reward Title");
                DrawProperty(_uiCopyObject, "_inventoryEmptySummary", "Empty Inventory Summary");
                DrawProperty(_uiCopyObject, "_inventoryFallbackRewardName", "Fallback Reward Name");
                DrawProperty(_uiCopyObject, "_exitConfirmationTitle", "Exit Popup Title");
                DrawProperty(_uiCopyObject, "_exitConfirmationBody", "Exit Popup Body");
                DrawProperty(_uiCopyObject, "_exitCollectButtonLabel", "Exit Collect Button");
                DrawProperty(_uiCopyObject, "_exitComeBackButtonLabel", "Exit Come Back Button");
            });

            DrawCard("Outcome Texts", "Stored in: WheelUiCopyCatalog.asset", "Controls reward, bomb and cashout popup copy.", () =>
            {
                DrawOutcomeTextRows();
            });

            DrawCard("Wheel Skins", "Stored in: WheelSkinCatalog.asset", "Maps Bronze, Silver and Golden tiers to wheel base and indicator sprites.", () =>
            {
                DrawProperty(_skinObject, "_spritesByTier", "Skin Tier Sprites", true);
            });

            DrawCard("Popup Motion", "Stored in: WheelOutcomePopupMotionCatalog.asset", "Controls the main outcome popup reveal animation.", () =>
            {
                DrawProperty(_popupMotionObject, "_motion", "Outcome Popup Motion", true);
            });

        }

        private void DrawAdvancedAssetsTab()
        {
            DrawCard("Advanced Linked Assets", "Stored in: Assets/Config", "Use this when you need raw SO inspection. Normal editing should happen in the other tabs.", () =>
            {
                DrawLinkedAssetControls("Main Settings", _settings, ref _showAdvancedSettings, _settingsObject);
                DrawLinkedAssetControls("Zone Texts", _uiCopy, ref _showAdvancedUiCopy, _uiCopyObject);
                DrawLinkedAssetControls("Wheel Skins", _skinCatalog, ref _showAdvancedSkins, _skinObject);
                DrawLinkedAssetControls("Popup Motion", _popupMotionCatalog, ref _showAdvancedPopupMotion, _popupMotionObject);
                DrawLinkedAssetControls("Resolve Rules", _spinResolveCatalog, ref _showAdvancedResolveRules, _spinResolveObject);
            });
        }

        private void DrawMetricGrid()
        {
            EditorGUILayout.Space(CardSpacing);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawMetric("Slices", GetInt(_settingsObject, "_sliceCount").ToString(), new Color(0.11f, 0.37f, 0.46f, 1f));
                DrawMetric("Safe Every", GetInt(_settingsObject, "_safeZoneInterval").ToString(), new Color(0.10f, 0.42f, 0.55f, 1f));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawMetric("Super Every", GetInt(_settingsObject, "_superZoneInterval").ToString(), new Color(0.36f, 0.44f, 0.13f, 1f));
                DrawMetric("Rewards", GetRewardTotal().ToString(), new Color(0.26f, 0.30f, 0.36f, 1f));
            }
        }

        private void DrawZoneBehavior(string label, int index)
        {
            SerializedProperty profiles = FindProperty(_settingsObject, "_zoneGameplayProfiles");
            if (profiles == null || profiles.arraySize <= index)
            {
                EditorGUILayout.HelpBox(label + " behavior is missing.", MessageType.Warning);
                return;
            }

            SerializedProperty profile = profiles.GetArrayElementAtIndex(index);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, _cardTitleStyle);
            DrawRelative(profile, "_includesBombSlot", "Includes Bomb Slot");
            DrawRelative(profile, "_allowLeave", "Cashout / Leave Allowed");
            EditorGUILayout.EndVertical();
        }

        private void DrawResolveProfile(SerializedProperty profiles, int index, string label)
        {
            if (profiles == null || profiles.arraySize <= index)
            {
                EditorGUILayout.HelpBox(label + " resolve rule is missing.", MessageType.Warning);
                return;
            }

            SerializedProperty profile = profiles.GetArrayElementAtIndex(index);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, _cardTitleStyle);
            DrawRelative(profile, "_targetPhase", "Target Phase");
            DrawRelative(profile, "_clearInventory", "Clear Inventory");
            DrawRelative(profile, "_advanceZone", "Advance Zone");
            DrawRelative(profile, "_markSlicesDirty", "Refresh Wheel Slices");
            DrawRelative(profile, "_addResultToInventory", "Add Result To Rewards");
            EditorGUILayout.EndVertical();
        }

        private void DrawZoneTextRows()
        {
            SerializedProperty zones = FindProperty(_uiCopyObject, "_zones");
            if (zones == null) return;

            for (int i = 0; i < zones.arraySize; i++)
            {
                SerializedProperty zone = zones.GetArrayElementAtIndex(i);
                string label = zone.FindPropertyRelative("_zoneType").enumDisplayNames[zone.FindPropertyRelative("_zoneType").enumValueIndex] + " Zone";
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(label, _cardTitleStyle);
                DrawRelative(zone, "_label", "Label");
                DrawRelative(zone, "_statusHint", "Status Hint");
                DrawRelative(zone, "_labelColorKey", "Color Key");
                DrawRelative(zone, "_skinTier", "Wheel Skin Tier");
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawOutcomeTextRows()
        {
            SerializedProperty outcomes = FindProperty(_uiCopyObject, "_outcomes");
            if (outcomes == null) return;

            for (int i = 0; i < outcomes.arraySize; i++)
            {
                SerializedProperty outcome = outcomes.GetArrayElementAtIndex(i);
                SerializedProperty phase = outcome.FindPropertyRelative("_phase");
                string label = phase.enumDisplayNames[phase.enumValueIndex] + " Outcome";
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(label, _cardTitleStyle);
                DrawRelative(outcome, "_title", "Title");
                DrawRelative(outcome, "_resultFallback", "Result Fallback");
                DrawRelative(outcome, "_resultColorKey", "Result Color");
                DrawRelative(outcome, "_showIcon", "Show Icon");
                DrawRelative(outcome, "_resultSource", "Result Source");
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawLinkedAssetControls(string label, UnityEngine.Object asset, ref bool rawExpanded, SerializedObject serializedObject)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, _cardTitleStyle, GUILayout.Width(132f));
                GUILayout.Label(asset == null ? "Missing" : AssetDatabase.GetAssetPath(asset), asset == null ? _statusMissingStyle : _statusOkStyle);
                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(asset == null);
                if (GUILayout.Button("Select", _secondaryButtonStyle, GUILayout.Width(68f), GUILayout.Height(22f))) SelectObject(asset);

                if (GUILayout.Button("Ping", _secondaryButtonStyle, GUILayout.Width(56f), GUILayout.Height(22f))) EditorGUIUtility.PingObject(asset);
                EditorGUI.EndDisabledGroup();
            }

            rawExpanded = EditorGUILayout.Foldout(rawExpanded, "Raw Inspector Fields", true);
            if (rawExpanded && serializedObject != null) DrawRawSerializedObject(serializedObject);

            EditorGUILayout.EndVertical();
        }

        private void DrawRawSerializedObject(SerializedObject serializedObject)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            EditorGUI.indentLevel++;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                using (new EditorGUI.DisabledScope(iterator.propertyPath == "m_Script"))
                {
                    EditorGUILayout.PropertyField(iterator, true, GUILayout.MaxWidth(ContentWidth));
                }
            }
            EditorGUI.indentLevel--;
        }

        private void DrawCard(string title, string storage, string hint, Action content)
        {
            EditorGUILayout.Space(CardSpacing);
            EditorGUILayout.BeginVertical(_cardStyle, GUILayout.MaxWidth(ContentWidth));
            try
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(title, _cardTitleStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(storage, _pillStyle, GUILayout.MaxWidth(Mathf.Min(260f, ContentWidth * 0.45f)));
                }

                if (!string.IsNullOrEmpty(hint)) EditorGUILayout.LabelField(hint, _sectionHintStyle);

                EditorGUILayout.Space(7f);
                content();
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }

        private void RunAfterGui(Action action)
        {
            EditorApplication.delayCall += () =>
            {
                action();
                Repaint();
            };
        }

        private void DrawMetric(string label, string value, Color accent)
        {
            EditorGUILayout.BeginVertical(_cardStyle, GUILayout.MinHeight(70f), GUILayout.MaxWidth(ContentWidth * 0.5f));
            Rect strip = GUILayoutUtility.GetRect(1f, 3f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(strip, accent);
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(label, _mutedLabelStyle);
            EditorGUILayout.LabelField(value, _cardTitleStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawAssetStatusRow(string label, UnityEngine.Object asset, string hint)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, _cardTitleStyle, GUILayout.Width(130f));
                GUILayout.Label(asset == null ? "Missing" : "Linked", asset == null ? _statusMissingStyle : _statusOkStyle, GUILayout.Width(72f));
                GUILayout.Label(hint, _mutedLabelStyle);
            }
        }

        private void DrawGuideRow(string label, string description)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, _cardTitleStyle, GUILayout.Width(92f));
                GUILayout.Label(description, _mutedLabelStyle);
            }
        }

        private void DrawProperty(SerializedObject serializedObject, string propertyName, string label, bool includeChildren = false)
        {
            SerializedProperty property = FindProperty(serializedObject, propertyName);
            if (property != null) EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren, GUILayout.MaxWidth(ContentWidth));
        }

        private void DrawRelative(SerializedProperty parent, string propertyName, string label)
        {
            if (parent == null) return;

            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property != null) EditorGUILayout.PropertyField(property, new GUIContent(label), true, GUILayout.MaxWidth(ContentWidth));
        }

        private static SerializedProperty FindProperty(SerializedObject serializedObject, string propertyName) => serializedObject == null ? null : serializedObject.FindProperty(propertyName);

        private int GetRewardTotal()
        {
            return GetArraySize(_settingsObject, "_standardRewards")
                + GetArraySize(_settingsObject, "_safeRewards")
                + GetArraySize(_settingsObject, "_superRewards");
        }

        private static int GetArraySize(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = FindProperty(serializedObject, propertyName);
            return property == null || !property.isArray ? 0 : property.arraySize;
        }

        private static int GetInt(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = FindProperty(serializedObject, propertyName);
            return property == null ? 0 : property.intValue;
        }

        private float ContentWidth => Mathf.Max(320f, position.width - 54f);

        private void ResolveAssets(bool force)
        {
            if (!force && _settings != null && _settingsObject != null) return;

            _settings = AssetDatabase.LoadAssetAtPath<WheelGameSettings>(VertigoWheelPaths.GameSettingsPath);
            _settings ??= VertigoWheelAssetPipeline.EnsureGameSettings();

            if (_settings != null)
            {
                _uiCopy = _settings.UiCopy != null ? _settings.UiCopy : VertigoWheelAssetPipeline.EnsureUiCopyCatalog();
                _skinCatalog = _settings.SkinCatalog != null ? _settings.SkinCatalog : VertigoWheelAssetPipeline.EnsureSkinCatalog();
                _popupMotionCatalog = _settings.OutcomePopupMotionCatalog != null ? _settings.OutcomePopupMotionCatalog : VertigoWheelAssetPipeline.EnsureOutcomePopupMotionCatalog();
                _spinResolveCatalog = _settings.SpinResolveCatalog != null ? _settings.SpinResolveCatalog : VertigoWheelAssetPipeline.EnsureSpinResolveCatalog();
                WireMissingCatalogReferences();
            }

            RebuildSerializedObjects();
        }

        private void WireMissingCatalogReferences()
        {
            bool dirty = false;
            Undo.RecordObject(_settings, "Wire Wheel Designer Assets");
            if (_settings.UiCopy == null)
            {
                _settings.ConfigureUiCopy(_uiCopy);
                dirty = true;
            }

            if (_settings.SkinCatalog == null)
            {
                _settings.ConfigureSkinCatalog(_skinCatalog);
                dirty = true;
            }

            if (_settings.OutcomePopupMotionCatalog == null)
            {
                _settings.ConfigureOutcomePopupMotionCatalog(_popupMotionCatalog);
                dirty = true;
            }

            if (_settings.SpinResolveCatalog == null)
            {
                _settings.ConfigureSpinResolveCatalog(_spinResolveCatalog);
                dirty = true;
            }

            if (dirty)
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
            }
        }

        private void RebuildSerializedObjects()
        {
            _settingsObject = _settings == null ? null : new SerializedObject(_settings);
            _uiCopyObject = _uiCopy == null ? null : new SerializedObject(_uiCopy);
            _skinObject = _skinCatalog == null ? null : new SerializedObject(_skinCatalog);
            _popupMotionObject = _popupMotionCatalog == null ? null : new SerializedObject(_popupMotionCatalog);
            _spinResolveObject = _spinResolveCatalog == null ? null : new SerializedObject(_spinResolveCatalog);
        }

        private void UpdateSerializedObjects()
        {
            UpdateSerializedObject(_settingsObject);
            UpdateSerializedObject(_uiCopyObject);
            UpdateSerializedObject(_skinObject);
            UpdateSerializedObject(_popupMotionObject);
            UpdateSerializedObject(_spinResolveObject);
        }

        private static void UpdateSerializedObject(SerializedObject serializedObject)
        {
            serializedObject?.Update();
        }

        private void ApplySerializedObjects()
        {
            ApplySerializedObject(_settingsObject);
            ApplySerializedObject(_uiCopyObject);
            ApplySerializedObject(_skinObject);
            ApplySerializedObject(_popupMotionObject);
            ApplySerializedObject(_spinResolveObject);
        }

        private static void ApplySerializedObject(SerializedObject serializedObject)
        {
            if (serializedObject == null) return;

            if (serializedObject.ApplyModifiedProperties()) EditorUtility.SetDirty(serializedObject.targetObject);
        }

        private static void SelectObject(UnityEngine.Object target)
        {
            if (target == null) return;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private void BuildStyles()
        {
            EnsureCachedTextures();
            _headerTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                normal = { textColor = new Color(0.92f, 0.98f, 1f, 1f) }
            };

            _headerSubtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.66f, 0.76f, 0.81f, 1f) }
            };

            _cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset((int)CardPadding, (int)CardPadding, 10, 12),
                margin = new RectOffset(10, 10, 0, 0),
                normal = { background = _cardTexture }
            };

            _cardTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.91f, 0.95f, 0.96f, 1f) }
            };

            _mutedLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal = { textColor = new Color(0.62f, 0.70f, 0.74f, 1f) }
            };

            _sectionHintStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal = { textColor = new Color(0.61f, 0.70f, 0.73f, 1f) }
            };

            _pillStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(8, 8, 3, 3),
                normal =
                {
                    textColor = new Color(0.80f, 0.93f, 0.96f, 1f),
                    background = _fieldTexture
                }
            };

            _primaryButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white,
                    background = _primaryButtonTexture
                },
                hover =
                {
                    textColor = Color.white,
                    background = _primaryButtonTexture
                }
            };

            _secondaryButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                normal =
                {
                    textColor = new Color(0.88f, 0.94f, 0.96f, 1f),
                    background = _secondaryButtonTexture
                },
                hover =
                {
                    textColor = Color.white,
                    background = _secondaryButtonTexture
                }
            };

            _statusOkStyle = new GUIStyle(_pillStyle)
            {
                normal =
                {
                    textColor = new Color(0.82f, 1f, 0.90f, 1f),
                    background = _okTexture
                }
            };

            _statusMissingStyle = new GUIStyle(_pillStyle)
            {
                normal =
                {
                    textColor = new Color(1f, 0.86f, 0.82f, 1f),
                    background = _missingTexture
                }
            };
        }

        private void EnsureCachedTextures()
        {
            _cardTexture ??= MakeTexture(new Color(0.15f, 0.18f, 0.21f, 1f));
            _fieldTexture ??= MakeTexture(new Color(0.10f, 0.13f, 0.16f, 1f));
            _primaryButtonTexture ??= MakeTexture(new Color(0.10f, 0.53f, 0.66f, 1f));
            _secondaryButtonTexture ??= MakeTexture(new Color(0.20f, 0.24f, 0.28f, 1f));
            _okTexture ??= MakeTexture(new Color(0.12f, 0.38f, 0.30f, 1f));
            _missingTexture ??= MakeTexture(new Color(0.50f, 0.15f, 0.13f, 1f));
        }

        private void DestroyCachedTextures()
        {
            DestroyTexture(ref _cardTexture);
            DestroyTexture(ref _fieldTexture);
            DestroyTexture(ref _primaryButtonTexture);
            DestroyTexture(ref _secondaryButtonTexture);
            DestroyTexture(ref _okTexture);
            DestroyTexture(ref _missingTexture);
        }

        private static void DestroyTexture(ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            DestroyImmediate(texture);
            texture = null;
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
#endif
