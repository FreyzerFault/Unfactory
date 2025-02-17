using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace HexGrid
{
    [ExecuteAlways]
    public class HexGenerator : MonoBehaviour
    {
        public float size = 1;
        public int res = 256;
        public bool flat = false;

        public bool buildFromVertices = false;
        private Vector2[] _vertices;

        private SpriteRenderer _sr;
    
        [ContextMenu("Awake")]
        private void Awake()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();

            if (_sr == null)
            {
                var child = new GameObject("Hex Renderer");
                child.transform.SetParent(transform);
                _sr = child.AddComponent<SpriteRenderer>();
            }
        
            RasterHexOnSprite();
        }

        private void OnValidate() => RasterHexOnSprite();


        #region SPRITE
    
        public void RasterHexOnSprite()
        {
            if (buildFromVertices)
                _vertices = BuildVertices_PivotCorner(size, flat);
        
            Rect rect = GetHexRect(size, flat);
            int width = Mathf.CeilToInt(rect.width * res);
            int height = Mathf.CeilToInt(rect.height * res);
            
            Rect imgRect = new(0, 0, width, height);
            
            var tex = new Texture2D(width, height) { filterMode = FilterMode.Point, alphaIsTransparency = true };
            Color[] colors = new Color[width * height];
        
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                colors[y * width + x] = PointOnHex(new Vector2(x, y) / imgRect.size * rect.size) ? Color.white : Color.clear;
    
            tex.SetPixels(colors);
            tex.Apply();
        
            _sr.sprite = Sprite.Create(tex, imgRect, Vector2.one * 0.5f, res, 0);
        }

        public void CreateSpriteAsset()
        {
            CreateSpriteAsset(_sr.sprite, $"Hex {size}m [{res}x{res}]");
        }
    
        private static void CreateSpriteAsset(Sprite sprite, string name = null)
        {
            string path = "Assets/Resources/Sprites/Hexagon/" + (name ?? sprite.name) + ".asset";
            UnityEditor.AssetDatabase.CreateAsset(sprite, path);
        }

        #endregion


        #region HEX PROPERTIES

        private static Rect GetHexRect(float size, bool flat = false)
        {
            float width = flat ? size * 2f : Mathf.Sqrt(3) * size;
            float height = flat ? Mathf.Sqrt(3) * size : size * 2f;
            return new Rect(0, 0, width, height);
        }

        #endregion
    

        #region VERTICES

        public static Vector2[] BuildVertices_PivotCorner(float size = 1, bool flat = false)
        {
            Rect rect = GetHexRect(size, flat);
            float width = rect.width;
            float height = rect.height;

            Vector2[] vertices;
            if (flat)
            {
                vertices = new Vector2[]
                {
                    new(0, height / 2f),
                    new(width / 4f, 0),
                    new(width / 4f * 3, 0),
                    new(width, height / 2f),
                    new(width / 4f * 3, height),
                    new(width / 4f, height),
                };
            }
            else
            {
                vertices = new Vector2[]
                {
                    new(width / 2f, 0),
                    new(width, height / 4f),
                    new(width, height / 4f * 3),
                    new(width / 2f, height),
                    new(0, height / 4f * 3),
                    new(0, height / 4f),
                };
            }
        
            return vertices;
        }

        #endregion

    
        #region POINT ON HEX
    
        private bool PointOnHex(Vector2 p)
        {
            return buildFromVertices 
                ? PointOnHex_LeftOfEdge(p, size, _vertices, flat) 
                : PointOnHex_Barycentric(p, size, flat);
        }
    
        public static bool PointOnHex_LeftOfEdge(Vector2 p, float size = 1, Vector2[] vertices = null, bool flat = false)
        {
            vertices ??= BuildVertices_PivotCorner(size, flat);
        
            for (var i = 0; i < 6; i++)
            {
                Vector2 a = vertices[i];
                Vector2 b = vertices[(i + 1) % 6];
            
                Vector2 ab = (b - a).normalized;
                Vector2 ap = (p - a).normalized;
            
                if (-ab.x * ap.y + ab.y * ap.x > 0)
                    return false;
            }

            return true;
        }

        public static bool PointOnHex_Barycentric(Vector2 p, float size = 1, bool flat = false)
        {
            Rect rect = GetHexRect(size, flat);
            Vector2 center = rect.center;
        
            // Check if point is outside bounding circle
            if (Vector2.Distance(center, p) > size)
                return false;
        
            // Check if point is outside bounding rectangle
            if (p.y > rect.height || p.y < 0 || p.x > rect.width || p.x < 0)
                return false;
        
            // Check if point is outside lateral triangles
            for (var i = 0; i < 6; i++)
            {
                // Skip top and flat triangles
                if (i is 1 or 4) continue;
            
                Vector2 radius = (flat ? Vector2.right : Vector2.up).RotateVector(i * 60) * size;
                Vector2 nextRadius = radius.RotateVector(60);

                Vector2 a = center, b = center + radius, c = center + nextRadius;
            
                float w1 = (a.x * (c.y - a.y) + (p.y - a.y) * (c.x - a.x) - p.x * (c.y - a.y)) /
                           ((b.y - a.y) * (c.x - a.x) - (b.x - a.x) * (c.y - a.y));
                float w2 = (p.y - a.y - w1 * (b.y - a.y)) / (c.y - a.y);

                if (w1 + w2 > 1)
                    return false;
            }

            return true;
        }
        
        #endregion

        
        #region DEBUG
        
        private void OnDrawGizmos()
        {
            Rect rect = GetHexRect(size, flat);
            Vector2 center = Vector2.zero;
        
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(center, 0.02f);
        
            Gizmos.color = Color.green;
            for (var i = 0; i < 6; i++)
                Gizmos.DrawLine(center, center + ((flat ? Vector2.right : Vector2.up) * size).RotateVector(i * 60));
        
            var vertices = BuildVertices_PivotCorner(size, flat);
            center = rect.center;
            Gizmos.color = Color.blue;
            for (var i = 0; i < 6; i++)
            {
                Gizmos.DrawSphere(vertices[i] - center, 0.02f);;
                Gizmos.DrawLine(vertices[i] - center, vertices[(i + 1) % 6] - center);
            }
        }

        #endregion
    }
}
