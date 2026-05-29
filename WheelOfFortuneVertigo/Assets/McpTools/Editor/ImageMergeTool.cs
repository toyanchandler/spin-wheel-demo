#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Vertigo.Wheel.McpTools
{
    [McpForUnityTool(
        "image_merge",
        Description = "Merge sibling Unity UI Image layers under a target GameObject into one PNG sprite, optionally replacing the source layers and adding the sprite to a SpriteAtlas.",
        Group = "core")]
    public static class ImageMergeTool
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null) return new ErrorResponse("Parameters cannot be null.");

            string targetQuery = @params["target"]?.ToString();
            if (string.IsNullOrWhiteSpace(targetQuery)) return new ErrorResponse("'target' is required. Use a GameObject name or hierarchy path.");

            GameObject target = FindSceneObject(targetQuery);
            if (target == null) return new ErrorResponse($"Target GameObject '{targetQuery}' was not found in the open scene.");

            var targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null) return new ErrorResponse($"Target '{target.name}' is not a RectTransform UI object.");

            bool recursive = @params["recursive"]?.ToObject<bool>() ?? false;
            bool includeInactive = @params["includeInactive"]?.ToObject<bool>() ?? true;
            bool replaceSources = @params["replaceSources"]?.ToObject<bool>() ?? false;
            string mergedObjectName = @params["mergedObjectName"]?.ToString();
            if (string.IsNullOrWhiteSpace(mergedObjectName)) mergedObjectName = "MergedImage";

            string outputPath = @params["outputPath"]?.ToString();
            if (string.IsNullOrWhiteSpace(outputPath)) outputPath = "Assets/Art/demo_content/" + SanitizeAssetName(target.name) + "_merged.png";

            string atlasPath = @params["atlasPath"]?.ToString();
            if (string.IsNullOrWhiteSpace(atlasPath)) atlasPath = "Assets/Art/ui_demo_content.spriteatlas";

            int outputWidth = @params["width"]?.ToObject<int>() ?? Mathf.RoundToInt(Mathf.Abs(targetRect.rect.width));
            int outputHeight = @params["height"]?.ToObject<int>() ?? Mathf.RoundToInt(Mathf.Abs(targetRect.rect.height));
            if (outputWidth <= 0 || outputHeight <= 0) return new ErrorResponse("Could not infer output size. Pass positive 'width' and 'height'.");

            HashSet<string> includeNames = ParseNameSet(@params["includeNames"]);
            HashSet<string> excludeNames = ParseNameSet(@params["excludeNames"]);
            List<Image> layers = CollectLayers(target.transform, recursive, includeInactive, mergedObjectName, includeNames, excludeNames);
            if (layers.Count == 0) return new ErrorResponse($"No mergeable child Image layers found under '{target.name}'.");

            try
            {
                string absoluteOutputPath = Path.GetFullPath(outputPath);
                string manifestPath = Path.Combine("Temp", "McpImageMerge", SanitizeAssetName(target.name) + "_manifest.json");
                Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
                JObject manifest = BuildManifest(targetRect, layers, outputWidth, outputHeight, absoluteOutputPath);
                File.WriteAllText(manifestPath, manifest.ToString(), System.Text.Encoding.UTF8);

                string scriptPath = Path.GetFullPath("Assets/McpTools/Python/merge_ui_images.py");
                string pythonOutput = RunPython(scriptPath, Path.GetFullPath(manifestPath));
                ImportSprite(outputPath);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(outputPath);
                if (sprite == null) return new ErrorResponse($"Merged PNG was written but Unity did not import a Sprite at '{outputPath}'.");

                Image mergedImage = CreateOrUpdateMergedImage(targetRect, mergedObjectName, sprite);
                if (replaceSources) RemoveSourceLayers(layers, mergedImage);

                AddSpriteToAtlas(sprite, atlasPath);
                EditorSceneManager.MarkSceneDirty(target.scene);
                AssetDatabase.SaveAssets();

                return new SuccessResponse(
                    $"Merged {layers.Count} Image layers into '{outputPath}'.",
                    new
                    {
                        target = GetPath(target.transform),
                        outputPath,
                        atlasPath,
                        mergedObject = GetPath(mergedImage.transform),
                        sourceLayerCount = layers.Count,
                        pythonOutput
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Image merge failed: {ex.Message}");
            }
        }

        private static JObject BuildManifest(RectTransform targetRect, List<Image> layers, int outputWidth, int outputHeight, string outputPath)
        {
            Rect parentRect = targetRect.rect;
            float scaleX = outputWidth / Mathf.Max(1f, Mathf.Abs(parentRect.width));
            float scaleY = outputHeight / Mathf.Max(1f, Mathf.Abs(parentRect.height));
            var manifest = new JObject
            {
                ["width"] = outputWidth,
                ["height"] = outputHeight,
                ["output"] = outputPath,
                ["layers"] = new JArray()
            };

            var layerArray = (JArray)manifest["layers"];
            for (int i = 0; i < layers.Count; i++)
            {
                Image image = layers[i];
                Sprite sprite = image.sprite;
                string assetPath = sprite == null ? string.Empty : AssetDatabase.GetAssetPath(sprite);
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(targetRect, image.rectTransform);
                Color color = image.color;
                var layer = new JObject
                {
                    ["name"] = GetPath(image.transform),
                    ["targetRect"] = RectJson(
                        (bounds.min.x - parentRect.xMin) * scaleX,
                        (parentRect.yMax - bounds.max.y) * scaleY,
                        bounds.size.x * scaleX,
                        bounds.size.y * scaleY),
                    ["color"] = new JArray(color.r, color.g, color.b, color.a)
                };

                if (!string.IsNullOrWhiteSpace(assetPath))
                {
                    Rect textureRect = sprite.textureRect;
                    layer["source"] = Path.GetFullPath(assetPath);
                    layer["textureRect"] = RectJson(textureRect.x, textureRect.y, textureRect.width, textureRect.height);
                }
                else
                {
                    layer["solid"] = true;
                }

                layerArray.Add(layer);
            }

            return manifest;
        }

        private static JObject RectJson(float x, float y, float width, float height)
        {
            return new JObject
            {
                ["x"] = float.Parse(x.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                ["y"] = float.Parse(y.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                ["width"] = float.Parse(width.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture),
                ["height"] = float.Parse(height.ToString("0.###", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
            };
        }

        private static string RunPython(string scriptPath, string manifestPath)
        {
            var info = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = Quote(scriptPath) + " " + Quote(manifestPath),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0) throw new InvalidOperationException(error);

                return output.Trim();
            }
        }

        private static Image CreateOrUpdateMergedImage(RectTransform targetRect, string mergedObjectName, Sprite sprite)
        {
            Transform child = targetRect.Find(mergedObjectName);
            GameObject mergedObject = child != null ? child.gameObject : new GameObject(mergedObjectName, typeof(RectTransform));
            if (child == null) mergedObject.transform.SetParent(targetRect, false);

            var rect = mergedObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            Image image = mergedObject.GetComponent<Image>();
            image ??= mergedObject.AddComponent<Image>();

            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = false;
            image.maskable = true;
            return image;
        }

        private static void RemoveSourceLayers(List<Image> layers, Image mergedImage)
        {
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                Image image = layers[i];
                if (image == null || image == mergedImage) continue;

                Object.DestroyImmediate(image.gameObject);
            }
        }

        private static List<Image> CollectLayers(
            Transform root,
            bool recursive,
            bool includeInactive,
            string mergedObjectName,
            HashSet<string> includeNames,
            HashSet<string> excludeNames)
        {
            var result = new List<Image>();
            if (recursive)
            {
                Image[] images = root.GetComponentsInChildren<Image>(includeInactive);
                for (int i = 0; i < images.Length; i++)
                {
                    AddIfMergeable(images[i], root, includeInactive, mergedObjectName, includeNames, excludeNames, result);
                }
            }
            else
            {
                for (int i = 0; i < root.childCount; i++)
                {
                    Image image = root.GetChild(i).GetComponent<Image>();
                    AddIfMergeable(image, root, includeInactive, mergedObjectName, includeNames, excludeNames, result);
                }
            }

            result.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            return result;
        }

        private static void AddIfMergeable(
            Image image,
            Transform root,
            bool includeInactive,
            string mergedObjectName,
            HashSet<string> includeNames,
            HashSet<string> excludeNames,
            List<Image> result)
        {
            if (image == null || image.transform == root || image.name == mergedObjectName) return;

            if (!includeInactive && !image.gameObject.activeInHierarchy) return;

            if (includeNames.Count > 0 && !includeNames.Contains(image.name)) return;

            if (excludeNames.Contains(image.name)) return;

            if (image.sprite == null && image.color.a <= 0f) return;

            result.Add(image);
        }

        private static HashSet<string> ParseNameSet(JToken token)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            if (token == null || token.Type == JTokenType.Null) return result;

            if (token.Type == JTokenType.Array)
            {
                foreach (JToken entry in token)
                {
                    AddName(result, entry?.ToString());
                }

                return result;
            }

            string value = token.ToString();
            string[] names = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < names.Length; i++)
            {
                AddName(result, names[i]);
            }

            return result;
        }

        private static void AddName(HashSet<string> result, string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) result.Add(value.Trim());
        }

        private static void ImportSprite(string assetPath)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void AddSpriteToAtlas(Sprite sprite, string atlasPath)
        {
            if (sprite == null || string.IsNullOrWhiteSpace(atlasPath)) return;

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if (atlas == null) return;

            atlas.Add(new Object[] { sprite });
            atlas.SetIncludeInBuild(true);
            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
            SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
        }

        private static GameObject FindSceneObject(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform transform = transforms[i];
                if (EditorUtility.IsPersistent(transform.gameObject) || !transform.gameObject.scene.IsValid()) continue;

                if (transform.name == query || GetPath(transform) == query) return transform.gameObject;
            }

            return null;
        }

        private static string GetPath(Transform transform)
        {
            if (transform == null) return string.Empty;

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        private static string SanitizeAssetName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }

            return value.Replace(' ', '_');
        }

        private static string Quote(string value) => "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}
#endif
