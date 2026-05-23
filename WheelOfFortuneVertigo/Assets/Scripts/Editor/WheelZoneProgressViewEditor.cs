#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Collections;
using Vertigo.Collections.Editor;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.EditorTools
{
    [CustomEditor(typeof(WheelZoneProgressView))]
    public sealed class WheelZoneProgressViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6f);
            if (!GUILayout.Button("Collect Zone Cells"))
            {
                return;
            }

            CollectZoneCells((WheelZoneProgressView)target);
        }

        public static void CollectZoneCells(WheelZoneProgressView view)
        {
            Transform root = view.transform;
            var bindings = new List<ZoneProgressCellBinding>();
            var settings = new ChildCollectionSettings
            {
                Root = root,
                Scope = ChildCollectionScope.DirectChildren,
                IncludeInactive = true
            };

            ChildCollectionUtility.VisitOrdered(settings, child =>
            {
                if (!child.name.StartsWith("ui_zone_progress_cell_", StringComparison.Ordinal))
                {
                    return;
                }

                Image image = child.GetComponent<Image>();
                Image glow = null;
                Image[] childImages = child.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < childImages.Length; i++)
                {
                    if (childImages[i].name.Contains("_glow"))
                    {
                        glow = childImages[i];
                    }
                }

                TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>(true);
                if (image == null || label == null)
                {
                    throw new InvalidOperationException(
                        "Zone cell " + child.name + " requires Image on the cell and TextMeshProUGUI in descendants.");
                }

                bindings.Add(new ZoneProgressCellBinding { Root = (RectTransform)child, Image = image, Glow = glow, Label = label });
            });

            ChildCollectionUtility.ApplySerializedStructArray(
                view,
                "_cells",
                bindings.ToArray(),
                WriteCellBinding);
            ChildCollectionEditorUtility.FinalizeCollect(view);
        }

        private static void WriteCellBinding(SerializedProperty property, ZoneProgressCellBinding binding)
        {
            property.FindPropertyRelative("Root").objectReferenceValue = binding.Root;
            property.FindPropertyRelative("Image").objectReferenceValue = binding.Image;
            property.FindPropertyRelative("Glow").objectReferenceValue = binding.Glow;
            property.FindPropertyRelative("Label").objectReferenceValue = binding.Label;
        }

        private struct ZoneProgressCellBinding
        {
            public RectTransform Root;
            public Image Image;
            public Image Glow;
            public TextMeshProUGUI Label;
        }
    }
}
#endif
