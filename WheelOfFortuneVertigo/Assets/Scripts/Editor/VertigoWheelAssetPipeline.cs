#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using Vertigo.Wheel.Data;
using Object = UnityEngine.Object;

namespace Vertigo.Wheel.EditorTools
{
    internal static class VertigoWheelAssetPipeline
    {
        public static WheelGameSettings EnsureGameSettings()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelGameSettings settings = AssetDatabase.LoadAssetAtPath<WheelGameSettings>(VertigoWheelPaths.GameSettingsPath);
            if (settings != null)
            {
                return settings;
            }

            Component legacySettings = FindLegacySceneSettings();
            settings = ScriptableObject.CreateInstance<WheelGameSettings>();
            if (legacySettings != null)
            {
                EditorUtility.CopySerialized(legacySettings, settings);
            }
            else
            {
                settings.ResetToDefaults();
            }

            WireDefaultCatalogs(settings);
            AssetDatabase.CreateAsset(settings, VertigoWheelPaths.GameSettingsPath);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static Component FindLegacySceneSettings()
        {
            GameObject legacyRoot = GameObject.Find("game_wheel_settings");
            if (legacyRoot == null)
            {
                return null;
            }

            Component[] components = legacyRoot.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    continue;
                }

                string typeName = components[i].GetType().Name;
                if (typeName == "WheelGameSettings")
                {
                    return components[i];
                }
            }

            return null;
        }

        private static void WireDefaultCatalogs(WheelGameSettings settings)
        {
            settings.ConfigureUiCopy(EnsureUiCopyCatalog());
            settings.ConfigureSkinCatalog(EnsureSkinCatalog());
            settings.ConfigureSliceLayoutCatalog(EnsureSliceLayoutCatalog());
            settings.ConfigureOutcomePopupMotionCatalog(EnsureOutcomePopupMotionCatalog());
            settings.ConfigureSpinResolveCatalog(EnsureSpinResolveCatalog());
        }

        public static WheelUiCopyCatalog EnsureUiCopyCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelUiCopyCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelUiCopyCatalog>(VertigoWheelPaths.UiCopyCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WheelUiCopyCatalog>();
                catalog.ResetToDefaults();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.UiCopyCatalogPath);
            }

            WireUiCopyCatalogSprites(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        public static void WireUiCopyCatalogSprites(WheelUiCopyCatalog catalog)
        {
            SerializedObject serializedCatalog = new SerializedObject(catalog);
            SerializedProperty zones = serializedCatalog.FindProperty("_zones");
            for (int i = 0; i < zones.arraySize; i++)
            {
                SerializedProperty zone = zones.GetArrayElementAtIndex(i);
                string panelSpriteName = zone.FindPropertyRelative("_panelSpriteName").stringValue;
                string mapFrameSpriteName = zone.FindPropertyRelative("_mapFrameSpriteName").stringValue;
                zone.FindPropertyRelative("_panelSprite").objectReferenceValue = FindSprite(panelSpriteName);
                zone.FindPropertyRelative("_mapFrameSprite").objectReferenceValue = FindSprite(mapFrameSpriteName);
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        }

        public static WheelSkinCatalog EnsureSkinCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelSkinCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelSkinCatalog>(VertigoWheelPaths.SkinCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WheelSkinCatalog>();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.SkinCatalogPath);
            }

            catalog.Configure(
                new WheelSkinTierSprites
                {
                    wheelBase = FindSprite("ui_spin_bronze_base"),
                    indicator = FindSprite("ui_spin_bronze_indicator")
                },
                new WheelSkinTierSprites
                {
                    wheelBase = FindSprite("ui_spin_silver_base"),
                    indicator = FindSprite("ui_spin_silver_indicator")
                },
                new WheelSkinTierSprites
                {
                    wheelBase = FindSprite("ui_spin_golden_base"),
                    indicator = FindSprite("ui_spin_golden_indicator")
                });

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        public static WheelSliceLayoutCatalog EnsureSliceLayoutCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelSliceLayoutCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelSliceLayoutCatalog>(VertigoWheelPaths.SliceLayoutCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WheelSliceLayoutCatalog>();
                catalog.ResetToDefaults();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.SliceLayoutCatalogPath);
                AssetDatabase.SaveAssets();
            }

            return catalog;
        }

        public static WheelOutcomePopupMotionCatalog EnsureOutcomePopupMotionCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelOutcomePopupMotionCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelOutcomePopupMotionCatalog>(VertigoWheelPaths.OutcomePopupMotionCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WheelOutcomePopupMotionCatalog>();
                catalog.ResetToDefaults();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.OutcomePopupMotionCatalogPath);
                AssetDatabase.SaveAssets();
            }

            return catalog;
        }

        public static WheelSpinResolveCatalog EnsureSpinResolveCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelSpinResolveCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelSpinResolveCatalog>(VertigoWheelPaths.SpinResolveCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WheelSpinResolveCatalog>();
                catalog.ResetToDefaults();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.SpinResolveCatalogPath);
                AssetDatabase.SaveAssets();
            }

            return catalog;
        }

        public static void ConfigureImportedSprites()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { VertigoWheelPaths.DemoContentPath });
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < textureGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(textureGuids[i]);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null)
                    {
                        continue;
                    }

                    bool dirty = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        dirty = true;
                    }

                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        dirty = true;
                    }

                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        dirty = true;
                    }

                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        dirty = true;
                    }

                    if (!Mathf.Approximately(importer.spritePixelsPerUnit, 100f))
                    {
                        importer.spritePixelsPerUnit = 100f;
                        dirty = true;
                    }

                    Vector4 border = IsSlicedSprite(path) ? new Vector4(12f, 12f, 12f, 12f) : Vector4.zero;
                    if (importer.spriteBorder != border)
                    {
                        importer.spriteBorder = border;
                        dirty = true;
                    }

                    if (dirty)
                    {
                        importer.SaveAndReimport();
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static Material EnsureUiMaterial()
        {
            Directory.CreateDirectory("Assets/Materials");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(VertigoWheelPaths.MaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("UI/Default");
            material = new Material(shader) { name = "UI_Default_Batched" };
            AssetDatabase.CreateAsset(material, VertigoWheelPaths.MaterialPath);
            return material;
        }

        public static void EnsureAtlas()
        {
            Directory.CreateDirectory("Assets/Art");
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(VertigoWheelPaths.AtlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, VertigoWheelPaths.AtlasPath);
            }

            Object[] existingPackables = atlas.GetPackables();
            if (existingPackables.Length > 0)
            {
                atlas.Remove(existingPackables);
            }

            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { VertigoWheelPaths.DemoContentPath });
            var sprites = new List<Object>(spriteGuids.Length);
            for (int i = 0; i < spriteGuids.Length; i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(spriteGuids[i]));
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            atlas.Add(sprites.ToArray());
            atlas.SetIncludeInBuild(true);
            atlas.SetPackingSettings(new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 4
            });
            atlas.SetTextureSettings(new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            });
            atlas.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.Automatic,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 50
            });
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
        }

        public static Sprite FindSprite(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:Sprite", new[] { VertigoWheelPaths.DemoContentPath });
            if (guids.Length == 0)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static bool IsSlicedSprite(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            return fileName.Contains("button") || fileName.Contains("frame") || fileName.Contains("panel");
        }
    }
}
#endif
