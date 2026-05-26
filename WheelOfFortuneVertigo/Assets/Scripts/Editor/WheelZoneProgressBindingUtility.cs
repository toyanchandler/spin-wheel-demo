#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Collections.Editor;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    internal static class WheelZoneProgressBindingUtility
    {
        private const string Frame = "Frame";

        public static void CollectIntoView(WheelZoneProgressView view)
        {
            Transform cellRoot = RequireChild(view.transform, WheelZoneProgressHierarchy.CellsGroup);

            if (NeedsHierarchyRebuild(cellRoot))
            {
                throw new InvalidOperationException(
                    view.name + " zone progress hierarchy is outdated. Fix the scene hierarchy before collecting bindings.");
            }

            WireChildComponents(cellRoot, WireCellView);

            SerializedObject serializedObject = new SerializedObject(view);
            serializedObject.FindProperty("_cellRoot").objectReferenceValue = cellRoot;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            ChildCollectionEditorService.Collect(view, "_cells");
            ChildCollectionEditorUtility.FinalizeCollect(view);
        }

        public static void WireCellView(WheelZoneProgressCellView view)
        {
            SerializedObject serializedObject = new SerializedObject(view);
            serializedObject.FindProperty("_frameImage").objectReferenceValue =
                RequireChild(view.transform, Frame).GetComponent<Image>();
            serializedObject.FindProperty("_valueText").objectReferenceValue =
                RequireChild(view.transform, WheelZoneProgressHierarchy.ValueLabel).GetComponent<TextMeshProUGUI>();
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireChildComponents(Transform root, Action<WheelZoneProgressCellView> wire)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                WheelZoneProgressCellView component = child.GetComponent<WheelZoneProgressCellView>();
                if (component == null)
                {
                    component = child.gameObject.AddComponent<WheelZoneProgressCellView>();
                }

                wire(component);
            }
        }

        private static bool NeedsHierarchyRebuild(Transform cellRoot)
        {
            if (cellRoot.childCount == 0)
            {
                return true;
            }

            Transform firstCell = cellRoot.GetChild(0);
            return firstCell.Find(Frame) == null
                || firstCell.Find(WheelZoneProgressHierarchy.ValueLabel) == null
                || firstCell.Find(WheelZoneProgressHierarchy.StandardState) != null
                || firstCell.Find("Glow") != null
                || firstCell.Find("Marker") != null;
        }

        private static Transform RequireChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                throw new InvalidOperationException(parent.name + " requires child " + childName + ".");
            }

            return child;
        }
    }
}
#endif
