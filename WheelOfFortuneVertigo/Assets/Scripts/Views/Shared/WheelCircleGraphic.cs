using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class WheelCircleGraphic : MaskableGraphic
    {
        [SerializeField, Range(24, 256)] private int _segments = 192;
        [SerializeField, Range(0f, 0.96f)] private float _innerRadius;
        [SerializeField, Range(0f, 4f)] private float _edgeFeather = 1.35f;

        public void Configure(float innerRadius)
        {
            _innerRadius = Mathf.Clamp01(innerRadius);
            SetVerticesDirty();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetVerticesDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            Rect rect = rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            if (radius <= 0f) return;
            int segments = Mathf.Max(24, _segments);
            float innerRadius = radius * _innerRadius;
            if (innerRadius <= 0.01f)
            {
                PopulateDisk(vertexHelper, radius, segments);
                return;
            }

            PopulateRing(vertexHelper, radius, innerRadius, segments);
        }

        private void PopulateDisk(VertexHelper vertexHelper, float radius, int segments)
        {
            float feather = Mathf.Min(_edgeFeather, radius * 0.25f);
            float solidRadius = Mathf.Max(0f, radius - feather);

            UIVertex center = Vertex(Vector2.zero, color);
            vertexHelper.AddVert(center);

            for (int i = 0; i <= segments; i++)
            {
                float angle = (Mathf.PI * 2f * i) / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vertexHelper.AddVert(Vertex(direction * solidRadius, color));
                vertexHelper.AddVert(Vertex(direction * radius, Transparent(color)));
            }

            for (int i = 0; i < segments; i++)
            {
                int index = 1 + (i * 2);
                vertexHelper.AddTriangle(0, index, index + 2);
                vertexHelper.AddTriangle(index, index + 1, index + 2);
                vertexHelper.AddTriangle(index + 1, index + 3, index + 2);
            }
        }

        private void PopulateRing(VertexHelper vertexHelper, float outerRadius, float innerRadius, int segments)
        {
            float feather = Mathf.Min(_edgeFeather, (outerRadius - innerRadius) * 0.45f);
            float outerSolidRadius = Mathf.Max(innerRadius, outerRadius - feather);
            float innerSolidRadius = Mathf.Min(outerSolidRadius, innerRadius + feather);
            Color transparent = Transparent(color);

            for (int i = 0; i <= segments; i++)
            {
                float angle = (Mathf.PI * 2f * i) / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                vertexHelper.AddVert(Vertex(direction * outerRadius, transparent));
                vertexHelper.AddVert(Vertex(direction * outerSolidRadius, color));
                vertexHelper.AddVert(Vertex(direction * innerSolidRadius, color));
                vertexHelper.AddVert(Vertex(direction * innerRadius, transparent));
            }

            for (int i = 0; i < segments; i++)
            {
                int index = i * 4;
                AddQuad(vertexHelper, index, index + 4, index + 1, index + 5);
                AddQuad(vertexHelper, index + 1, index + 5, index + 2, index + 6);
                AddQuad(vertexHelper, index + 2, index + 6, index + 3, index + 7);
            }
        }

        private static UIVertex Vertex(Vector2 position, Color32 vertexColor)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = vertexColor;
            vertex.position = position;
            return vertex;
        }

        private static Color32 Transparent(Color source)
        {
            Color transparent = source;
            transparent.a = 0f;
            return transparent;
        }

        private static void AddQuad(VertexHelper vertexHelper, int a, int b, int c, int d)
        {
            vertexHelper.AddTriangle(a, b, c);
            vertexHelper.AddTriangle(c, b, d);
        }
    }
}
