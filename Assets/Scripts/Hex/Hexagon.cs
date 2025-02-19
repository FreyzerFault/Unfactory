using System;
using System.Linq;
using DavidUtils.ExtensionMethods;
using UnityEngine;

namespace Hex
{
    [Serializable]
    public class Hexagon
    {
        public bool flat = false;
        public float size = 0.5f;

        private Vector2[] _vertices;
        
        public Vector2[] Vertices => _vertices ??= BuildVertices_PivotCenter(size, flat);
        public Vector3[] VerticesXY => Vertices.ToV3xy().ToArray();
        public Vector3[] VerticesXZ => Vertices.ToV3xz().ToArray();

        
        #region HEX PROPERTIES

        public Rect Rect => GetHexRect(size, flat);
        public float Width => Rect.width;
        public float Height => Rect.height;

        private static Rect GetHexRect(float size, bool flat = false)
        {
            float width = flat ? size * 2f : Mathf.Sqrt(3) * size;
            float height = flat ? Mathf.Sqrt(3) * size : size * 2f;
            return new Rect(-width / 2f, -height / 2f, width, height);
        }

        #endregion
        
        
        #region WORLD TRANSFORMATIONS

        public Vector3[] WorldVertices(Transform transform) => Vertices.Select(v => transform.localToWorldMatrix.MultiplyPoint(v.ToV3xy())).ToArray();

        #endregion
        
        
        #region VERTICES

        public void UpdateVertices() => _vertices = BuildVertices_PivotCenter(size, flat);

        private static Vector2[] BuildVertices_PivotCenter(float size = 1, bool flat = false) => 
            flat ? BuildVertices_Flat_PivotCorner(size) : BuildVertices_Pointy_PivotCorner(size);

        private static Vector2[] BuildVertices_Flat_PivotCorner(float size = 1)
        {
            Rect rect = GetHexRect(size, true);
            float height = rect.height;
            return new Vector2[]
            {
                new(-size, 0),
                new(-size / 2f, -height / 2f),
                new( size / 2f, -height / 2f),
                new( size, 0),
                new( size / 2f, height / 2f),
                new(-size / 2f, height / 2f),
            };
        }
        
        private static Vector2[] BuildVertices_Pointy_PivotCorner(float size = 1)
        {
            Rect rect = GetHexRect(size, false);
            float width = rect.width;
            return new Vector2[]
            {
                new(0, -size),
                new(width / 2f, -size / 2f),
                new(width / 2f, size / 2f),
                new(0, size),
                new(-width / 2f, size / 2f),
                new(-width / 2f, -size / 2f),
            };
        }

        #endregion
        
        
        #region POINT ON HEX
    
        // p relativo al centro
        public bool PointOnHex(Vector2 p)
        {
            // Bounding circle
            if (p.magnitude > size) return false;
            
            // Bounding rectangle
            if (!Rect.Contains(p))
                return false;
            
            // TODO Profile which method is faster + check bugs
            return true
                ? PointOnHex_LeftOfEdge(p, size, Vertices, flat) 
                : PointOnHex_Barycentric(p, size, flat);
        }

        private static bool PointOnHex_LeftOfEdge(Vector2 p, float size = 1, Vector2[] vertices = null, bool flat = false)
        {
            vertices ??= BuildVertices_PivotCenter(size, flat);
        
            for (var i = 0; i < 6; i++)
            {
                Vector2 a = vertices[i];
                Vector2 b = vertices[(i + 1) % 6];
            
                Vector2 ab = (b - a).normalized;
                Vector2 ap = (p - a).normalized;
                
                // Cross product < 0 => RIGHT
                if (ab.x * ap.y - ab.y * ap.x < 0)
                    return false;
            }

            return true;
        }

        private static bool PointOnHex_Barycentric(Vector2 p, float size = 1, bool flat = false, bool skipFlatTriangles = true)
        {
            // Lateral triangles
            for (var i = 0; i < 6; i++)
            {
                // Skip flat triangles (useless if checked Bounding rectangle)
                if (skipFlatTriangles && i is 1 or 4) continue;
            
                // Build Triangle Barycentric Coordinates
                Vector2 radius = (flat ? Vector2.right : Vector2.up).Rotate(i * 60) * size;
                Vector2 nextRadius = radius.Rotate(60);

                Vector2 
                    a = Vector2.zero, 
                    b = a + radius, 
                    c = a + nextRadius;
                
                Vector2 baryCoords = BarycentricCoords(p, a, b, c);
                
                // w1 + w2 = [0, 1] => Inside Triangle
                // w1 + w2 > 1 => Outside Hexagon
                if (baryCoords.x + baryCoords.y > 1)
                    return false;
            }

            return true;
        }
        
        private static Vector2 BarycentricCoords(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float w1 = (a.x * (c.y - a.y) + (p.y - a.y) * (c.x - a.x) - p.x * (c.y - a.y)) /
                       ((b.y - a.y) * (c.x - a.x) - (b.x - a.x) * (c.y - a.y));
            float w2 = (p.y - a.y - w1 * (b.y - a.y)) / (c.y - a.y);
            return new Vector2(w1, w2);
        }
        
        #endregion
    }
}
