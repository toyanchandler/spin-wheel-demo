#if UNITY_EDITOR
using System;
using System.IO;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vertigo.Wheel.McpTools
{
    [McpForUnityTool(
        "make_gameobject_prefab",
        Description = "Save a scene GameObject as a prefab asset, optionally connecting the scene object to the prefab.",
        Group = "core")]
    public static class MakeGameObjectPrefabTool
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
            {
                return new ErrorResponse("Parameters cannot be null.");
            }

            string targetQuery = @params["target"]?.ToString();
            string prefabPath = @params["prefabPath"]?.ToString();
            bool connect = @params["connect"]?.ToObject<bool>() ?? true;

            if (string.IsNullOrWhiteSpace(targetQuery))
            {
                return new ErrorResponse("'target' is required. Use a GameObject name or hierarchy path.");
            }

            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                return new ErrorResponse("'prefabPath' is required and must be under Assets/.");
            }

            if (!prefabPath.StartsWith("Assets/", StringComparison.Ordinal) || !prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                return new ErrorResponse("'prefabPath' must be an Assets-relative .prefab path.");
            }

            GameObject target = FindSceneObject(targetQuery);
            if (target == null)
            {
                return new ErrorResponse($"Target GameObject '{targetQuery}' was not found in the open scene.");
            }

            try
            {
                string directory = Path.GetDirectoryName(prefabPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                bool success;
                GameObject prefab = connect
                    ? PrefabUtility.SaveAsPrefabAssetAndConnect(target, prefabPath, InteractionMode.AutomatedAction, out success)
                    : PrefabUtility.SaveAsPrefabAsset(target, prefabPath, out success);

                if (!success || prefab == null)
                {
                    return new ErrorResponse($"Unity failed to save prefab '{prefabPath}'.");
                }

                EditorSceneManager.MarkSceneDirty(target.scene);
                AssetDatabase.SaveAssets();
                return new SuccessResponse(
                    $"Saved '{target.name}' as prefab '{prefabPath}'.",
                    new
                    {
                        target = GetPath(target.transform),
                        prefabPath,
                        connected = connect
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Failed to create prefab: {ex.Message}");
            }
        }

        private static GameObject FindSceneObject(string query)
        {
            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform transform = transforms[i];
                if (EditorUtility.IsPersistent(transform.gameObject) || !transform.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (transform.name == query || GetPath(transform) == query)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
#endif
