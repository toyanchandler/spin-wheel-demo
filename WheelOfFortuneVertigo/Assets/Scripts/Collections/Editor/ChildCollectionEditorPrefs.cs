#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vertigo.Collections;

namespace Vertigo.Collections.Editor
{
    internal static class ChildCollectionEditorPrefs
    {
        private const string HelpFoldoutKey = "Vertigo.Collections.HelpFoldout";
        private const string PreprocessBuildSuffix = "CollectOnPreprocessBuild";

        public static bool HelpFoldout
        {
            get { return EditorPrefs.GetBool(HelpFoldoutKey, false); }
            set { EditorPrefs.SetBool(HelpFoldoutKey, value); }
        }

        public static bool GetCollectOnPreprocessBuild(Object host, string fieldName, CollectChildrenAttribute attribute) => EditorPrefs.GetBool(BuildKey(host, fieldName, PreprocessBuildSuffix), attribute.CollectOnPreprocessBuild);

        public static void SetCollectOnPreprocessBuild(Object host, string fieldName, bool enabled)
        {
            EditorPrefs.SetBool(BuildKey(host, fieldName, PreprocessBuildSuffix), enabled);
        }

        private static string BuildKey(Object host, string fieldName, string suffix)
        {
            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(host);
            return "Vertigo.Collections." + id.ToString() + "." + fieldName + "." + suffix;
        }
    }
}
#endif
