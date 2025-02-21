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
        
        //  ↖   ↗
        // ←     → 0
        //  ↙   ↘
        private static Vector2[] BuildVertices_Flat_PivotCorner(float size = 1)
        {
            Rect rect = GetHexRect(size, true);
            float height = rect.height;
            return new Vector2[]
            {
                new( size, 0),
                new( size / 2f, height / 2f),
                new(-size / 2f, height / 2f),
                new(-size, 0),
                new(-size / 2f, -height / 2f),
                new( size / 2f, -height / 2f),
            };
        }
        
        //     ↑
        //  ↖     ↗ 0
        //  ↙     ↘ 
        //     ↓ 
        private static Vector2[] BuildVertices_Pointy_PivotCorner(float size = 1)
        {
            Rect rect = GetHexRect(size, false);
            float width = rect.width;
            return new Vector2[]
            {
                new(width / 2f, size / 2f),
                new(0, size),
                new(-width / 2f, size / 2f),
                new(-width / 2f, -size / 2f),
                new(0, -size),
                new(width / 2f, -size / 2f),
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

        private static bool PointOnHex_LeftOfEdge(Vector2 p, float size = 1, Vector2[] vertices = null, bool flat = false, bool skipFlatTriangles = true)
        {
            vertices ??= BuildVertices_PivotCenter(size, flat);
        
            // Skip flat triangles (useless if checked Bounding rectangle)
            int skipped1 = flat ? 0 : 1;
            int skipped2 = flat ? 3 : 4;
            
            for (var i = 0; i < 6; i++)
            {
                if (skipFlatTriangles && (i == skipped1 || i == skipped2)) continue;
                
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
            // Skip flat triangles (useless if checked Bounding rectangle)
            int skipped1 = flat ? 0 : 1;
            int skipped2 = flat ? 3 : 4;
            
            // Lateral triangles
            for (var i = 0; i < 6; i++)
            {
                if (skipFlatTriangles && (i == skipped1 || i == skipped2)) continue;
            
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
        
        
        #region ORIENTATION

        // Start from East
        // CCW [ → ↗ ↑ ↖ ← ↙ ↓ ↘ ]
        public enum Orientation { East, NorthEast, North, NorthWest, West, SouthWest, South, SouthEast, Invalid }

        #region WEDGE ORIENTATION

        // WEDGE Orientation = Triangular Sectors
        private static Orientation[] _wedgeOrientationsFlat = {
            Orientation.NorthEast,  //     ↑
            Orientation.North,      //  ↖     ↗ 0
            Orientation.NorthWest,  //  ↙     ↘ 
            Orientation.SouthWest,  //     ↓  
            Orientation.South,
            Orientation.SouthEast
        };
        private static Orientation[] _wedgeOrientationsPointy = {
            Orientation.NorthEast,  //  ↖   ↗ 0
            Orientation.NorthWest,  // ←     → 
            Orientation.West,       //  ↙   ↘
            Orientation.SouthWest,
            Orientation.SouthEast,
            Orientation.East
        };
        
        public Orientation[] WedgeOrientations => flat ? _wedgeOrientationsFlat : _wedgeOrientationsPointy;
        
        public bool PointOnWedge(Vector2 p, out Orientation orientation, out Vector2[] edge) => PointOnWedge(p, size, out orientation, out edge, Vertices, flat);
        public Orientation PointOrientation(Vector2 p, out Vector2[] edge) => PointOrientation(p, size, out edge, Vertices, flat);
        
        // POINT is on WEDGE (Triangle and Orientation)
        // Wedge EDGE is given for drawing the Triangle, for example
        public static bool PointOnWedge(Vector2 p, float size, out Orientation orientation, out Vector2[] edge, 
            Vector2[] vertices = null, bool flat = false)
        {
            orientation = PointOrientation(p, size, out edge, vertices, flat);
            if (orientation == Orientation.Invalid) return false;
            
            // POINT on CIRCLE
            if (p.magnitude > size) return false;
            
            // POINT on HEX (LEFT of edge)
            Vector2 ab = (edge[1] - edge[0]).normalized;
            Vector2 ap = (p - edge[0]).normalized;
            return ab.x * ap.y - ab.y * ap.x >= 0;
        }
        
        // POINT Orientation (Wedge)
        // Wedge EDGE is given for further optimization of the POINT on HEX calculation
        public static Orientation PointOrientation(Vector2 p, float size, out Vector2[] edge, Vector2[] vertices = null, bool flat = false)
        {
            vertices ??= BuildVertices_PivotCenter(size, flat);
            
            edge = new Vector2[2];
            
            for (var i = 0; i < 6; i++)
            {
                Vector2 a = vertices[i];
                Vector2 b = vertices[(i + 1) % 6];
                Vector2 c = Vector2.zero;
                
                
                Vector2 ca = (a - c).normalized;
                Vector2 cb = (b - c).normalized;
                Vector2 cp = (p - c).normalized;

                float aAngle = Vector2.Angle(cp, ca);
                float bAngle = Vector2.Angle(cp, cb);
                
                // TODO: Contemplar caso en que el Angulo es 0 y 60. El punto está en 2 wedges a la vez...
                // Si el angulo [cp, ca] y [cp, cb] >= 60º => Fuera del wedge (extendido al infinito) 
                if (aAngle >= 60 || bAngle >= 60) continue;
                
                // Guarda el edge para adelantar su cálculo para después
                edge = new[] {a, b};
                return (flat ? _wedgeOrientationsFlat : _wedgeOrientationsPointy)[i];
            }

            return Orientation.Invalid;
        }
        
        public static Vector2 GetEdge(bool flat, float size, Orientation orientation, Vector2[] vertices = null)
        {
            Orientation[] orientations = flat ? _wedgeOrientationsFlat : _wedgeOrientationsPointy;
            
            // Orientation invalid for this Hexagon Type
            if (!orientations.Contains(orientation))
                return Vector2.zero;
            
            vertices ??= BuildVertices_PivotCenter(size, flat);
            
            // Vertices and Orientations are in the same order
            int index = Array.IndexOf(orientations, orientation);
            return vertices[(index + 1) % 6] - vertices[index];
        }
        
        #endregion
        
        #endregion

    }
}
