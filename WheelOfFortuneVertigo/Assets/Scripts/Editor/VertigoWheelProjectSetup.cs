#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Vertigo.Wheel.EditorTools
{
    internal static class VertigoWheelProjectSetup
    {
        public static void ConfigureAndroidProject()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.companyName = "Vertigo";
            PlayerSettings.productName = "Vertigo Wheel Demo";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.handleraigames.vertigowheeldemo");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel35;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.stripEngineCode = true;
            EditorUserBuildSettings.buildAppBundle = true;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(VertigoWheelPaths.ScenePath, true) };
        }

        public static void EnsureGameViewSizes()
        {
            try
            {
                AddGameViewSize("Vertigo 2:1 Landscape 1118x558", 1118, 558);
                AddGameViewSize("Vertigo 16:9 Landscape 1920x1080", 1920, 1080);
                AddGameViewSize("Vertigo 20:9 Landscape 2400x1080", 2400, 1080);
                AddGameViewSize("Vertigo 21:9 Landscape 2520x1080", 2520, 1080);
                AddGameViewSize("Vertigo 4:3 Landscape 2048x1536", 2048, 1536);
                AddGameViewSize("Vertigo 20:9 1080x2400", 1080, 2400);
                AddGameViewSize("Vertigo 16:9 1080x1920", 1080, 1920);
                AddGameViewSize("Vertigo 4:3 1536x2048", 1536, 2048);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not install custom Game view sizes automatically: " + exception.Message);
            }
        }

        private static void AddGameViewSize(string label, int width, int height)
        {
            Type sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            Type singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            object instance = singleType.GetProperty("instance").GetValue(null, null);
            MethodInfo getGroup = sizesType.GetMethod("GetGroup");
            object group = getGroup.Invoke(instance, new object[] { GameViewSizeGroupType.Android });

            MethodInfo getCount = group.GetType().GetMethod("GetBuiltinCount");
            MethodInfo getCustomCount = group.GetType().GetMethod("GetCustomCount");
            MethodInfo getGameViewSize = group.GetType().GetMethod("GetGameViewSize");
            int total = (int)getCount.Invoke(group, null) + (int)getCustomCount.Invoke(group, null);
            for (int i = 0; i < total; i++)
            {
                object size = getGameViewSize.Invoke(group, new object[] { i });
                string baseText = ReadGameViewSizeText(size);
                if (baseText == label)
                {
                    return;
                }
            }

            Type sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
            Type sizeTypeEnum = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
            object fixedResolution = Enum.Parse(sizeTypeEnum, "FixedResolution");
            ConstructorInfo constructor = sizeType.GetConstructor(new[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });
            object newSize = constructor.Invoke(new[] { fixedResolution, width, height, label });
            group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
        }

        private static string ReadGameViewSizeText(object size)
        {
            if (size == null)
            {
                return string.Empty;
            }

            Type type = size.GetType();
            PropertyInfo property = type.GetProperty("baseText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                return property.GetValue(size, null) as string;
            }

            MethodInfo method = type.GetMethod("baseText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                return method.Invoke(size, null) as string;
            }

            return string.Empty;
        }
    }
}
#endif
