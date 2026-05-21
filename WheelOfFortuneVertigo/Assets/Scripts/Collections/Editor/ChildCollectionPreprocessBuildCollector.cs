#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Vertigo.Collections.Editor
{
    public sealed class ChildCollectionPreprocessBuildCollector : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            MonoBehaviour[] behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (!behaviours[i].gameObject.scene.IsValid())
                {
                    continue;
                }

                ChildCollectionEditorService.CollectAllMarkedFields(behaviours[i], preprocessOnly: true);
            }
        }
    }
}
#endif
