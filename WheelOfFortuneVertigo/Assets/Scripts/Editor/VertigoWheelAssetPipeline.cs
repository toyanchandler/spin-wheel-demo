#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
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
                WireLayoutSprites(settings);
                return settings;
            }

            settings = ScriptableObject.CreateInstance<WheelGameSettings>();
            settings.ResetToDefaults();

            WireDefaultCatalogs(settings);
            WireLayoutSprites(settings);
            AssetDatabase.CreateAsset(settings, VertigoWheelPaths.GameSettingsPath);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static void WireDefaultCatalogs(WheelGameSettings settings)
        {
            settings.ConfigureUiCopy(EnsureUiCopyCatalog());
            settings.ConfigureSkinCatalog(EnsureSkinCatalog());
            settings.ConfigureOutcomePopupMotionCatalog(EnsureOutcomePopupMotionCatalog());
            settings.ConfigureSpinResolveCatalog(EnsureSpinResolveCatalog());
        }

        private static void WireLayoutSprites(WheelGameSettings settings)
        {
            if (settings.Layout == null || settings.Layout.RewardCardFrameSprite != null) return;

            settings.Layout.BindRewardCardFrameSprite(FindSprite("ui_loot_row_frame_blue"));
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        public static WheelUiCopyCatalog EnsureUiCopyCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelUiCopyCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelUiCopyCatalog>(VertigoWheelPaths.UiCopyCatalogPath);
            catalog ??= ScriptableObject.CreateInstance<WheelUiCopyCatalog>();
                catalog.ResetToDefaults();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.UiCopyCatalogPath);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        public static WheelSkinCatalog EnsureSkinCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelSkinCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelSkinCatalog>(VertigoWheelPaths.SkinCatalogPath);
            catalog ??= ScriptableObject.CreateInstance<WheelSkinCatalog>();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.SkinCatalogPath);
            catalog.Configure(
                WheelSkinTierSprites.Create(
                    FindSprite("ui_spin_bronze_base"),
                    FindSprite("ui_spin_bronze_indicator")),
                WheelSkinTierSprites.Create(
                    FindSprite("ui_spin_silver_base"),
                    FindSprite("ui_spin_silver_indicator")),
                WheelSkinTierSprites.Create(
                    FindSprite("ui_spin_golden_base"),
                    FindSprite("ui_spin_golden_indicator")));

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        public static WheelOutcomePopupMotionCatalog EnsureOutcomePopupMotionCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelOutcomePopupMotionCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelOutcomePopupMotionCatalog>(VertigoWheelPaths.OutcomePopupMotionCatalogPath);
            catalog ??= ScriptableObject.CreateInstance<WheelOutcomePopupMotionCatalog>();
                catalog.ResetToDefaults();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.OutcomePopupMotionCatalogPath);
                AssetDatabase.SaveAssets();
            return catalog;
        }

        public static WheelSpinResolveCatalog EnsureSpinResolveCatalog()
        {
            Directory.CreateDirectory("Assets/Config");
            WheelSpinResolveCatalog catalog = AssetDatabase.LoadAssetAtPath<WheelSpinResolveCatalog>(VertigoWheelPaths.SpinResolveCatalogPath);
            catalog ??= ScriptableObject.CreateInstance<WheelSpinResolveCatalog>();
                catalog.ResetToDefaults();
                AssetDatabase.CreateAsset(catalog, VertigoWheelPaths.SpinResolveCatalogPath);
                AssetDatabase.SaveAssets();
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
                    if (importer == null) continue;

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

                    if (importer.filterMode != FilterMode.Bilinear)
                    {
                        importer.filterMode = FilterMode.Bilinear;
                        dirty = true;
                    }

                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        dirty = true;
                    }

                    int maxTextureSize = 2048;
                    var defaultPlatform = importer.GetDefaultPlatformTextureSettings();
                    if (defaultPlatform.maxTextureSize != maxTextureSize
                        || defaultPlatform.textureCompression != TextureImporterCompression.Uncompressed
                        || defaultPlatform.format != TextureImporterFormat.RGBA32)
                    {
                        defaultPlatform.maxTextureSize = maxTextureSize;
                        defaultPlatform.textureCompression = TextureImporterCompression.Uncompressed;
                        defaultPlatform.format = TextureImporterFormat.RGBA32;
                        importer.SetPlatformTextureSettings(defaultPlatform);
                        dirty = true;
                    }

                    var androidPlatform = importer.GetPlatformTextureSettings("Android");
                    if (!androidPlatform.overridden
                        || androidPlatform.maxTextureSize != maxTextureSize
                        || androidPlatform.textureCompression != TextureImporterCompression.Uncompressed
                        || androidPlatform.format != TextureImporterFormat.RGBA32)
                    {
                        androidPlatform.name = "Android";
                        androidPlatform.overridden = true;
                        androidPlatform.maxTextureSize = maxTextureSize;
                        androidPlatform.textureCompression = TextureImporterCompression.Uncompressed;
                        androidPlatform.format = TextureImporterFormat.RGBA32;
                        importer.SetPlatformTextureSettings(androidPlatform);
                        dirty = true;
                    }

                    Vector4 border = IsSlicedSprite(path) ? new Vector4(12f, 12f, 12f, 12f) : Vector4.zero;
                    if (importer.spriteBorder != border)
                    {
                        importer.spriteBorder = border;
                        dirty = true;
                    }

                    if (dirty) importer.SaveAndReimport();
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
            if (material != null) return material;

            Shader shader = Shader.Find("UI/Default");
            material = new Material(shader) { name = "UI_Default_Batched" };
            AssetDatabase.CreateAsset(material, VertigoWheelPaths.MaterialPath);
            return material;
        }

        public static TMP_FontAsset EnsureHudFontAsset()
        {
            Directory.CreateDirectory("Assets/Fonts");
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(VertigoWheelPaths.HudFontAssetPath);
            if (fontAsset != null)
            {
                if (fontAsset.atlasTexture != null) return fontAsset;

                AssetDatabase.DeleteAsset(VertigoWheelPaths.HudFontAssetPath);
            }

            AssetDatabase.ImportAsset(VertigoWheelPaths.HudFontPath, ImportAssetOptions.ForceSynchronousImport);
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(VertigoWheelPaths.HudFontPath);
            if (sourceFont == null)
            {
                Debug.LogWarning("HUD font source missing at " + VertigoWheelPaths.HudFontPath + ". Falling back to TMP default.");
                return TMP_Settings.defaultFontAsset;
            }

            fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            fontAsset.name = "Rajdhani-Bold SDF";
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.material.name = "Rajdhani-Bold SDF Material";
            fontAsset.TryAddCharacters("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ");

            AssetDatabase.CreateAsset(fontAsset, VertigoWheelPaths.HudFontAssetPath);
            Texture2D atlasTexture = fontAsset.atlasTexture;
            if (atlasTexture != null)
            {
                atlasTexture.name = "Rajdhani-Bold SDF Atlas";
                AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
                fontAsset.material.SetTexture(ShaderUtilities.ID_MainTex, atlasTexture);
            }

            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(VertigoWheelPaths.HudFontAssetPath, ImportAssetOptions.ForceSynchronousImport);
            return fontAsset;
        }

        public static void EnsureAtlas()
        {
            Directory.CreateDirectory("Assets/Art");
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(VertigoWheelPaths.AtlasPath);
            atlas ??= new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, VertigoWheelPaths.AtlasPath);
            Object[] existingPackables = atlas.GetPackables();
            if (existingPackables.Length > 0) atlas.Remove(existingPackables);

            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { VertigoWheelPaths.DemoContentPath });
            var sprites = new List<Object>(spriteGuids.Length);
            for (int i = 0; i < spriteGuids.Length; i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(spriteGuids[i]));
                if (sprite != null) sprites.Add(sprite);
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
                maxTextureSize = 8192,
                format = TextureImporterFormat.RGBA32,
                textureCompression = TextureImporterCompression.Uncompressed,
                compressionQuality = 100
            });
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
        }

        public static Sprite FindSprite(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:Sprite", new[] { VertigoWheelPaths.DemoContentPath });
            if (guids.Length == 0) return null;

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
