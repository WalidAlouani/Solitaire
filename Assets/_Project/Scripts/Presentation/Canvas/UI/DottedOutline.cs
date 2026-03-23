using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Presentation.Canvas.UI
{
    /// <summary>
    /// Draws a dotted/dashed rectangular outline using UI mesh generation.
    /// Add as a child Image-like component on any RectTransform.
    /// Optionally matches a sibling Image's preserve-aspect-ratio rect.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Dotted Outline")]
    public class DottedOutline : MaskableGraphic
    {
        [Header("Outline")]
        [SerializeField] private float _lineThickness = 2f;
        [SerializeField] private float _dashLength = 10f;
        [SerializeField] private float _gapLength = 6f;
        [SerializeField] private float _cornerRadius = 6f;

        [Header("Aspect Ratio")]
        [Tooltip("Reference Image to match when it uses Preserve Aspect. Leave null to fill the full rect.")]
        [SerializeField] private Image _referenceImage;

        public float LineThickness { get => _lineThickness; set { _lineThickness = value; SetVerticesDirty(); } }
        public float DashLength { get => _dashLength; set { _dashLength = value; SetVerticesDirty(); } }
        public float GapLength { get => _gapLength; set { _gapLength = value; SetVerticesDirty(); } }
        public float CornerRadius { get => _cornerRadius; set { _cornerRadius = value; SetVerticesDirty(); } }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            var rect = GetDrawRect();
            float w = rect.width;
            float h = rect.height;

            if (w <= 0 || h <= 0) return;

            float halfThick = _lineThickness * 0.5f;

            float left = rect.xMin + halfThick;
            float right = rect.xMax - halfThick;
            float bottom = rect.yMin + halfThick;
            float top = rect.yMax - halfThick;

            float r = Mathf.Min(_cornerRadius, Mathf.Min(w, h) * 0.5f);

            var points = new System.Collections.Generic.List<Vector2>();
            int cornerSegments = Mathf.Max(4, Mathf.CeilToInt(r * 0.5f));

            // Top-left corner
            for (int i = cornerSegments; i >= 0; i--)
            {
                float angle = Mathf.PI * 0.5f + (Mathf.PI * 0.5f) * ((float)i / cornerSegments);
                points.Add(new Vector2(left + r + Mathf.Cos(angle) * r, top - r + Mathf.Sin(angle) * r));
            }

            // Top-right corner
            for (int i = cornerSegments; i >= 0; i--)
            {
                float angle = (Mathf.PI * 0.5f) * ((float)i / cornerSegments);
                points.Add(new Vector2(right - r + Mathf.Cos(angle) * r, top - r + Mathf.Sin(angle) * r));
            }

            // Bottom-right corner
            for (int i = cornerSegments; i >= 0; i--)
            {
                float angle = -(Mathf.PI * 0.5f) * (1f - (float)i / cornerSegments);
                points.Add(new Vector2(right - r + Mathf.Cos(angle) * r, bottom + r + Mathf.Sin(angle) * r));
            }

            // Bottom-left corner
            for (int i = cornerSegments; i >= 0; i--)
            {
                float angle = Mathf.PI + (Mathf.PI * 0.5f) * ((float)i / cornerSegments);
                points.Add(new Vector2(left + r + Mathf.Cos(angle) * r, bottom + r + Mathf.Sin(angle) * r));
            }

            // Compute cumulative distances along the path
            float totalLength = 0f;
            var distances = new float[points.Count];
            distances[0] = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                totalLength += Vector2.Distance(points[i - 1], points[i]);
                distances[i] = totalLength;
            }

            float closeGap = Vector2.Distance(points[points.Count - 1], points[0]);
            totalLength += closeGap;

            float dashCycle = _dashLength + _gapLength;
            float cursor = 0f;

            while (cursor < totalLength)
            {
                float dashStart = cursor;
                float dashEnd = Mathf.Min(cursor + _dashLength, totalLength);

                if (dashEnd > dashStart + 0.1f)
                    EmitSegment(vh, points, distances, totalLength, dashStart, dashEnd, halfThick);

                cursor += dashCycle;
            }
        }

        /// <summary>
        /// Computes the drawing rect. If a reference Image with preserveAspect is assigned,
        /// fits the rect to match the Image's aspect-preserved area.
        /// </summary>
        private Rect GetDrawRect()
        {
            var fullRect = GetPixelAdjustedRect();

            if (_referenceImage == null || !_referenceImage.preserveAspect || _referenceImage.sprite == null)
                return fullRect;

            var spriteRect = _referenceImage.sprite.rect;
            float spriteAspect = spriteRect.width / spriteRect.height;
            float rectAspect = fullRect.width / fullRect.height;

            float fitW, fitH;

            if (spriteAspect > rectAspect)
            {
                // Sprite is wider than rect — fit by width, shrink height
                fitW = fullRect.width;
                fitH = fullRect.width / spriteAspect;
            }
            else
            {
                // Sprite is taller than rect — fit by height, shrink width
                fitH = fullRect.height;
                fitW = fullRect.height * spriteAspect;
            }

            float offsetX = (fullRect.width - fitW) * 0.5f;
            float offsetY = (fullRect.height - fitH) * 0.5f;

            return new Rect(
                fullRect.xMin + offsetX,
                fullRect.yMin + offsetY,
                fitW,
                fitH
            );
        }

        private void EmitSegment(VertexHelper vh, System.Collections.Generic.List<Vector2> points,
            float[] distances, float totalLength, float startDist, float endDist, float halfThick)
        {
            float step = Mathf.Max(1f, _lineThickness);
            float dist = startDist;

            Vector2 prevPos = GetPointAtDistance(points, distances, totalLength, dist);
            Vector2 prevDir = GetDirectionAtDistance(points, distances, totalLength, dist);
            Vector2 prevPerp = new Vector2(-prevDir.y, prevDir.x);

            while (dist < endDist)
            {
                float nextDist = Mathf.Min(dist + step, endDist);
                Vector2 nextPos = GetPointAtDistance(points, distances, totalLength, nextDist);
                Vector2 nextDir = GetDirectionAtDistance(points, distances, totalLength, nextDist);
                Vector2 nextPerp = new Vector2(-nextDir.y, nextDir.x);

                int idx = vh.currentVertCount;

                vh.AddVert(prevPos + prevPerp * halfThick, color, Vector4.zero);
                vh.AddVert(prevPos - prevPerp * halfThick, color, Vector4.zero);
                vh.AddVert(nextPos - nextPerp * halfThick, color, Vector4.zero);
                vh.AddVert(nextPos + nextPerp * halfThick, color, Vector4.zero);

                vh.AddTriangle(idx, idx + 1, idx + 2);
                vh.AddTriangle(idx, idx + 2, idx + 3);

                prevPos = nextPos;
                prevDir = nextDir;
                prevPerp = nextPerp;
                dist = nextDist;
            }
        }

        private Vector2 GetPointAtDistance(System.Collections.Generic.List<Vector2> points,
            float[] distances, float totalLength, float dist)
        {
            dist = dist % totalLength;

            if (dist >= distances[distances.Length - 1])
            {
                float segLen = totalLength - distances[distances.Length - 1];
                if (segLen < 0.001f) return points[0];
                float t = (dist - distances[distances.Length - 1]) / segLen;
                return Vector2.Lerp(points[points.Count - 1], points[0], t);
            }

            for (int i = 1; i < distances.Length; i++)
            {
                if (dist <= distances[i])
                {
                    float segLen = distances[i] - distances[i - 1];
                    if (segLen < 0.001f) return points[i];
                    float t = (dist - distances[i - 1]) / segLen;
                    return Vector2.Lerp(points[i - 1], points[i], t);
                }
            }

            return points[points.Count - 1];
        }

        private Vector2 GetDirectionAtDistance(System.Collections.Generic.List<Vector2> points,
            float[] distances, float totalLength, float dist)
        {
            dist = dist % totalLength;

            if (dist >= distances[distances.Length - 1])
                return (points[0] - points[points.Count - 1]).normalized;

            for (int i = 1; i < distances.Length; i++)
            {
                if (dist <= distances[i])
                    return (points[i] - points[i - 1]).normalized;
            }

            return Vector2.right;
        }
    }
}
