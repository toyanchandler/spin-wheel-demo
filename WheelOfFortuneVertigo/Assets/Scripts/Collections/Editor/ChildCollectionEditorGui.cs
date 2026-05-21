#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Vertigo.Collections.Editor
{
    internal static class ChildCollectionEditorGui
    {
        private const float ButtonHeight = 28f;
        private const float ButtonGap = 8f;
        private const float ShadowDepth = 2f;

        private static GUIStyle _collectStyle;
        private static GUIStyle _resetStyle;
        private static GUIStyle _infoIconStyle;
        private static GUIStyle _infoTitleStyle;
        private static GUIStyle _infoBodyStyle;
        private static GUIStyle _infoArrowStyle;

        private static readonly Color CollectBase = new Color(0.20f, 0.60f, 0.36f, 1f);
        private static readonly Color CollectHover = new Color(0.26f, 0.74f, 0.44f, 1f);
        private static readonly Color CollectActive = new Color(0.14f, 0.46f, 0.28f, 1f);
        private static readonly Color CollectShadow = new Color(0.08f, 0.22f, 0.12f, 1f);

        private static readonly Color ResetBase = new Color(0.74f, 0.24f, 0.22f, 1f);
        private static readonly Color ResetHover = new Color(0.90f, 0.32f, 0.28f, 1f);
        private static readonly Color ResetActive = new Color(0.54f, 0.16f, 0.14f, 1f);
        private static readonly Color ResetShadow = new Color(0.28f, 0.08f, 0.08f, 1f);

        private static readonly Color InfoHeader = new Color(0.16f, 0.42f, 0.78f, 1f);
        private static readonly Color InfoIconBox = new Color(0.10f, 0.30f, 0.62f, 1f);
        private static readonly Color InfoBody = new Color(0.12f, 0.16f, 0.22f, 1f);

        public static void DrawSectionHeader()
        {
            EnsureStyles();
            EditorGUILayout.Space(4f);
            Rect line = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(line, new Color(0.35f, 0.55f, 0.78f, 0.45f));
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Child Collection", EditorStyles.boldLabel);
        }

        public static void DrawActionButtons(out bool collectClicked, out bool resetClicked)
        {
            EnsureStyles();
            Rect row = EditorGUILayout.GetControlRect(false, ButtonHeight + ShadowDepth);
            float buttonWidth = (row.width - ButtonGap) * 0.5f;

            Rect collectRect = new Rect(row.x, row.y, buttonWidth, ButtonHeight);
            Rect resetRect = new Rect(row.x + buttonWidth + ButtonGap, row.y, buttonWidth, ButtonHeight);

            collectClicked = DrawActionButton(collectRect, "Collect Children", _collectStyle, CollectBase, CollectHover, CollectActive, CollectShadow, false);
            resetClicked = DrawActionButton(resetRect, "Reset Children", _resetStyle, ResetBase, ResetHover, ResetActive, ResetShadow, true);
        }

        public static bool DrawInfoFoldout(bool expanded, string helpText)
        {
            EnsureStyles();
            EditorGUILayout.Space(4f);

            EditorGUILayout.BeginVertical();
            Rect headerRect = EditorGUILayout.GetControlRect(false, 24f);
            bool next = DrawInfoHeader(headerRect, expanded);

            if (next)
            {
                float textHeight = _infoBodyStyle.CalcHeight(new GUIContent(helpText), headerRect.width - 20f);
                Rect bodyRect = EditorGUILayout.GetControlRect(false, textHeight + 14f);
                EditorGUI.DrawRect(bodyRect, InfoBody);
                Rect textRect = new Rect(bodyRect.x + 10f, bodyRect.y + 7f, bodyRect.width - 20f, bodyRect.height - 14f);
                GUI.Label(textRect, helpText, _infoBodyStyle);
            }

            EditorGUILayout.EndVertical();
            return next;
        }

        private static bool DrawActionButton(
            Rect rect,
            string label,
            GUIStyle style,
            Color normal,
            Color hover,
            Color active,
            Color shadow,
            bool emphasize3d)
        {
            Rect faceRect = new Rect(rect.x, rect.y, rect.width, rect.height - ShadowDepth);
            Rect shadowRect = new Rect(rect.x, rect.y + faceRect.height, rect.width, ShadowDepth);
            EditorGUI.DrawRect(shadowRect, shadow);

            if (Event.current.type == EventType.Repaint)
            {
                Color face = normal;
                if (faceRect.Contains(Event.current.mousePosition))
                {
                    face = hover;
                }

                if (Event.current.type == EventType.MouseDown && faceRect.Contains(Event.current.mousePosition))
                {
                    face = active;
                }

                EditorGUI.DrawRect(faceRect, face);

                if (emphasize3d)
                {
                    EditorGUI.DrawRect(new Rect(faceRect.x, faceRect.y, faceRect.width, 1f), new Color(1f, 1f, 1f, 0.22f));
                    EditorGUI.DrawRect(new Rect(faceRect.x, faceRect.yMax - 1f, faceRect.width, 1f), new Color(0f, 0f, 0f, 0.28f));
                }
                else
                {
                    EditorGUI.DrawRect(new Rect(faceRect.x, faceRect.y, faceRect.width, 1f), new Color(1f, 1f, 1f, 0.14f));
                }
            }

            return GUI.Button(faceRect, label, style);
        }

        private static bool DrawInfoHeader(Rect headerRect, bool expanded)
        {
            EditorGUI.DrawRect(headerRect, InfoHeader);

            Rect iconBox = new Rect(headerRect.x + 6f, headerRect.y + 4f, 16f, 16f);
            EditorGUI.DrawRect(iconBox, InfoIconBox);
            GUI.Label(iconBox, "i", _infoIconStyle);

            Rect arrowRect = new Rect(headerRect.x + 26f, headerRect.y + 7f, 10f, 10f);
            GUI.Label(arrowRect, expanded ? "▼" : "▶", _infoArrowStyle);

            Rect labelRect = new Rect(headerRect.x + 40f, headerRect.y, headerRect.width - 46f, headerRect.height);
            GUI.Label(labelRect, "How child collection works", _infoTitleStyle);

            EditorGUIUtility.AddCursorRect(headerRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
            {
                expanded = !expanded;
                GUI.changed = true;
                Event.current.Use();
            }

            return expanded;
        }

        private static void EnsureStyles()
        {
            if (_collectStyle != null)
            {
                return;
            }

            _collectStyle = CreateButtonStyle();
            _resetStyle = CreateButtonStyle();

            _infoIconStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            _infoTitleStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            _infoArrowStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.88f, 0.94f, 1f, 1f) }
            };

            _infoBodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.82f, 0.88f, 0.94f, 1f) }
            };
        }

        private static GUIStyle CreateButtonStyle()
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 4, 4),
                normal = { textColor = Color.white, background = null },
                hover = { textColor = Color.white, background = null },
                active = { textColor = Color.white, background = null },
                focused = { textColor = Color.white, background = null }
            };
        }
    }
}
#endif
