#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Vertigo.Collections.Editor
{
    public static class ChildCollectionEditorUtility
    {
        public static void FinalizeCollect(Component host)
        {
            if (host == null)
            {
                return;
            }

            EditorUtility.SetDirty(host);
            if (host.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(host.gameObject.scene);
            }
        }
    }
}
#endif
