#if UNITY_EDITOR
using UnityEngine;
using Vertigo.Collections;

namespace Vertigo.Collections.Editor
{
    internal struct ChildCollectionSettings
    {
        public Transform Root;
        public ChildCollectionScope Scope;
        public bool IncludeInactive;

        public static ChildCollectionSettings FromHost(MonoBehaviour host, CollectChildrenAttribute attribute, Transform rootOverride)
        {
            Transform root = host.transform;
            if (rootOverride != null)
            {
                root = rootOverride;
            }
            else if (!string.IsNullOrEmpty(attribute.RootProperty))
            {
                var serializedObject = new UnityEditor.SerializedObject(host);
                UnityEditor.SerializedProperty rootProperty = serializedObject.FindProperty(attribute.RootProperty);
                if (rootProperty != null && rootProperty.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                {
                    root = rootProperty.objectReferenceValue as Transform;
                }

                if (root == null)
                {
                    throw new System.InvalidOperationException(
                        host.name + " is missing collection root on property " + attribute.RootProperty + ".");
                }
            }

            return new ChildCollectionSettings
            {
                Root = root,
                Scope = attribute.Scope,
                IncludeInactive = attribute.IncludeInactive
            };
        }
    }
}
#endif
