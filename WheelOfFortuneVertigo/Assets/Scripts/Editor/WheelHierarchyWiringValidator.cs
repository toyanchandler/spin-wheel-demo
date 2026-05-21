#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    internal static class WheelHierarchyWiringValidator
    {
        public static void ValidateScene()
        {
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                ValidateComponent(behaviours[i]);
            }
        }

        private static void ValidateComponent(MonoBehaviour behaviour)
        {
            if (behaviour == null || EditorUtility.IsPersistent(behaviour))
            {
                return;
            }

            string namespaceName = behaviour.GetType().Namespace;
            if (namespaceName == null || !namespaceName.StartsWith("Vertigo.Wheel", StringComparison.Ordinal))
            {
                return;
            }

            FieldInfo[] fields = behaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].IsDefined(typeof(SerializeField), false))
                {
                    continue;
                }

                object value = fields[i].GetValue(behaviour);
                bool required = fields[i].IsDefined(typeof(RequiredSceneReferenceAttribute), false);
                ValidateFieldValue(behaviour, fields[i].Name, value, required);
            }
        }

        private static void ValidateFieldValue(MonoBehaviour owner, string fieldName, object value, bool required)
        {
            if (value == null)
            {
                Require(!required, owner.GetType().Name + "." + fieldName + " is required but not assigned");
                return;
            }

            if (value is UnityEngine.Object[] array)
            {
                Require(!required || array.Length > 0, owner.GetType().Name + "." + fieldName + " is required but empty");
                for (int i = 0; i < array.Length; i++)
                {
                    Require(!required || array[i] != null, owner.GetType().Name + "." + fieldName + "[" + i + "] is required but not assigned");
                    ValidateReference(owner, fieldName + "[" + i + "]", array[i]);
                }

                return;
            }

            if (value is UnityEngine.Object unityObject)
            {
                Require(!required || unityObject != null, owner.GetType().Name + "." + fieldName + " is required but not assigned");
                ValidateReference(owner, fieldName, unityObject);
            }
        }

        private static void ValidateReference(MonoBehaviour owner, string fieldName, UnityEngine.Object reference)
        {
            if (reference == null)
            {
                return;
            }

            if (reference is ScriptableObject)
            {
                string assetPath = AssetDatabase.GetAssetPath(reference);
                Require(
                    assetPath.StartsWith("Assets/Config/", StringComparison.Ordinal),
                    owner.GetType().Name + "." + fieldName + " may only reference Assets/Config ScriptableObjects");
                return;
            }

            Component component = reference as Component;
            if (component == null)
            {
                return;
            }

            Transform ownerTransform = owner.transform;
            Transform targetTransform = component.transform;
            bool sameHierarchy = targetTransform == ownerTransform || targetTransform.IsChildOf(ownerTransform);
            Require(
                sameHierarchy,
                owner.GetType().Name + "." + fieldName + " must reference a component on the same GameObject or its children. Found "
                + targetTransform.name
                + " outside "
                + ownerTransform.name);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Hierarchy wiring rule failed: " + message);
            }
        }

    }
}
#endif
