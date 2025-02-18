using System;
using System.Linq;
using UnityEngine;
using Unfactory.Utils;
using DavidUtils.ExtensionMethods;

namespace Unfactory.Hex
{
    [ExecuteAlways]
    public abstract class HexGenerator : MonoBehaviour
    {
        public enum HexRenderType { Sprite, Mesh } 
        public HexRenderType renderType = HexRenderType.Mesh;
        
        public bool flat = false;
        public float size = 1;

        private Vector2[] _vertices;
        protected Vector2[] Vertices => _vertices ??= BuildVertices_PivotCenter(size, flat);
        protected Vector3[] WorldVertices => _vertices.Select(v => transform.localToWorldMatrix.MultiplyPoint(v.ToV3xy())).ToArray();

        private bool _needRegenerate = true;

        private void OnValidate() => _needRegenerate = true;

        private void Update()
        {
            if (!_needRegenerate) return;
            
            BuildVertices();
            GenerateHex();
            _needRegenerate = false;
        }
        
        public virtual void GenerateHex() => _needRegenerate = false;

        public abstract void SaveAsset();

        
        #region HEX PROPERTIES
        
        protected Rect HexRect => GetHexRect(size, flat);

        private static Rect GetHexRect(float size, bool flat = false)
        {
            float width = flat ? size * 2f : Mathf.Sqrt(3) * size;
            float height = flat ? Mathf.Sqrt(3) * size : size * 2f;
            return new Rect(0,0, width, height);
        }

        #endregion
    

        #region VERTICES

        private void BuildVertices() => _vertices = BuildVertices_PivotCenter(size, flat);

        protected static Vector2[] BuildVertices_PivotCenter(float size = 1, bool flat = false) => 
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
        protected bool PointOnHex(Vector2 p)
        {
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

        private static bool PointOnHex_Barycentric(Vector2 p, float size = 1, bool flat = false)
        {
            // Bounding circle
            if (p.magnitude > size)
                return false;
            
            // Bounding rectangle
            if (!GetHexRect(size, flat).Contains(p))
                return false;
        
            // Lateral triangles
            for (var i = 0; i < 6; i++)
            {
                // Skip flat triangles (useless if checked Bounding rectangle)
                if (i is 1 or 4) continue;
            
                // Build Triangle Barycentric Coordinates
                Vector2 radius = (flat ? Vector2.right : Vector2.up).RotateVector(i * 60) * size;
                Vector2 nextRadius = radius.RotateVector(60);

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
        
        
        #region CURSOR INTERACTION
        
        private static Vector3 RaycastCursor()
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector3.zero;
            
            Vector2 mousePos = Input.mousePosition;
            Ray ray = cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return Vector3.zero;
            
            Vector3 p = hit.point;
            return p;
        }

        #endregion
        
        
        
        #region DEBUG
        
        private void OnDrawGizmos()
        {
            const float pointSize = 0.02f;
            
            DrawGizmosDiagonals(pointSize);
            DrawGizmosVertices(pointSize);
            DrawGizmosCursorInHex(pointSize);
        }

        private void DrawGizmosDiagonals(float pointSize = 0.05f)
        {
            Vector3 center = transform.position;
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, pointSize);
        
            Gizmos.color = Color.green;
            foreach (Vector3 vertex in WorldVertices)
                Gizmos.DrawLine(center, center + vertex);
        }

        private void DrawGizmosVertices(float pointSize = 0.05f)
        {
            Vector3[] wVertices = WorldVertices;
            Gizmos.color = Color.blue;
            for (var i = 0; i < 6; i++)
            {
                Gizmos.DrawSphere(wVertices[i], pointSize);
                Gizmos.DrawLine(wVertices[i], wVertices[(i + 1) % 6]);
            }
        }

        private void DrawGizmosCursorInHex(float pointSize = 0.05f)
        {
            Vector3 cursorPoint = RaycastCursor();
            Vector3 localCursorPoint = transform.ToLocal(cursorPoint);
            Gizmos.color = PointOnHex(localCursorPoint) ? Color.green : Color.red;
            Gizmos.DrawSphere(cursorPoint, pointSize);
        }
        
        #endregion
    }
}
