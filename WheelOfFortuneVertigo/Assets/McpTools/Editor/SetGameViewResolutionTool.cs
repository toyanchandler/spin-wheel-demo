#if UNITY_EDITOR
using System;
using System.Reflection;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Vertigo.Wheel.McpTools
{
    [McpForUnityTool(
        "set_game_view_resolution",
        Description = "Set the Unity Game View to a fixed resolution before a screenshot capture.",
        Group = "core")]
    public static class SetGameViewResolutionTool
    {
        public static object HandleCommand(JObject @params)
        {
            if (@params == null) return new ErrorResponse("Parameters cannot be null.");

            int width = @params["width"]?.ToObject<int>() ?? 0;
            int height = @params["height"]?.ToObject<int>() ?? 0;
            string label = @params["label"]?.ToString();

            if (width <= 0 || height <= 0) return new ErrorResponse("'width' and 'height' must be positive integers.");

            try
            {
                int selectedIndex = EnsureGameViewSize(width, height, label);
                FocusGameView();
                return new SuccessResponse(
                    $"Game View resolution set to {width}x{height}.",
                    new
                    {
                        width,
                        height,
                        selectedIndex,
                        label = BuildLabel(width, height, label)
                    });
            }
            catch (Exception ex)
            {
                return new ErrorResponse($"Failed to set Game View resolution: {ex.Message}");
            }
        }

        private static int EnsureGameViewSize(int width, int height, string label)
        {
            Assembly editorAssembly = typeof(EditorWindow).Assembly;
            Type gameViewSizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
            Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
            Type gameViewSizeKindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");

            if (gameViewSizesType == null || gameViewSizeType == null || gameViewSizeKindType == null) throw new InvalidOperationException("Unity GameViewSize reflection types were not found.");

            object sizesInstance = FindProperty(gameViewSizesType, "instance", BindingFlags.Static)
                ?.GetValue(null);
            if (sizesInstance == null) throw new InvalidOperationException("Unity GameViewSizes instance was not available.");

            MethodInfo getGroup = gameViewSizesType.GetMethod(
                "GetGroup",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            GameViewSizeGroupType groupType = CurrentGameViewSizeGroup();
            object group = getGroup?.Invoke(sizesInstance, new object[] { groupType });
            if (group == null) throw new InvalidOperationException($"{groupType} Game View size group was not available.");

            int existingIndex = FindExistingSizeIndex(group, width, height);
            if (existingIndex < 0)
            {
                object fixedResolution = Enum.Parse(gameViewSizeKindType, "FixedResolution");
                object gameViewSize = Activator.CreateInstance(
                    gameViewSizeType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    binder: null,
                    args: new object[] { fixedResolution, width, height, BuildLabel(width, height, label) },
                    culture: null);
                MethodInfo addCustomSize = group.GetType().GetMethod(
                    "AddCustomSize",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                addCustomSize?.Invoke(group, new[] { gameViewSize });
                existingIndex = FindExistingSizeIndex(group, width, height);
            }

            if (existingIndex < 0) throw new InvalidOperationException($"Could not create or find Game View size {width}x{height}.");

            Type gameViewType = editorAssembly.GetType("UnityEditor.GameView");
            EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
            PropertyInfo selectedSizeIndex = gameViewType.GetProperty(
                "selectedSizeIndex",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            selectedSizeIndex?.SetValue(gameView, existingIndex);
            MethodInfo sizeSelectionCallback = gameViewType.GetMethod(
                "SizeSelectionCallback",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            sizeSelectionCallback?.Invoke(gameView, new object[] { existingIndex, null });
            gameView.Repaint();
            return existingIndex;
        }

        private static int FindExistingSizeIndex(object group, int width, int height)
        {
            MethodInfo getTotalCount = group.GetType().GetMethod(
                "GetTotalCount",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo getGameViewSize = group.GetType().GetMethod(
                "GetGameViewSize",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (getTotalCount == null || getGameViewSize == null) throw new InvalidOperationException("Game View size group reflection methods were not found.");

            int total = Convert.ToInt32(getTotalCount.Invoke(group, null));
            for (int i = 0; i < total; i++)
            {
                object size = getGameViewSize.Invoke(group, new object[] { i });
                int candidateWidth = ReadInt(size, "width");
                int candidateHeight = ReadInt(size, "height");
                if (candidateWidth == width && candidateHeight == height) return i;
            }

            return -1;
        }

        private static PropertyInfo FindProperty(Type type, string name, BindingFlags scope)
        {
            BindingFlags flags = scope | BindingFlags.Public | BindingFlags.NonPublic;
            for (Type current = type; current != null; current = current.BaseType)
            {
                PropertyInfo property = current.GetProperty(name, flags);
                if (property != null) return property;
            }

            return null;
        }

        private static int ReadInt(object source, string memberName)
        {
            if (source == null) return 0;

            Type type = source.GetType();
            PropertyInfo property = type.GetProperty(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null) return Convert.ToInt32(property.GetValue(source));

            FieldInfo field = type.GetField(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field != null ? Convert.ToInt32(field.GetValue(source)) : 0;
        }

        private static string BuildLabel(int width, int height, string label)
        {
            return string.IsNullOrWhiteSpace(label)
                ? $"MCP {width}x{height}"
                : $"MCP {label} {width}x{height}";
        }

        private static GameViewSizeGroupType CurrentGameViewSizeGroup()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return GameViewSizeGroupType.Android;
                case BuildTarget.iOS:
                    return GameViewSizeGroupType.iOS;
                default:
                    return GameViewSizeGroupType.Standalone;
            }
        }

        private static void FocusGameView()
        {
            Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            if (gameViewType == null) return;

            EditorWindow.GetWindow(gameViewType).Focus();
        }
    }
}
#endif
