using UnityEngine;

namespace Hex.Hex_Generation
{
    public class HexSpriteGenerator : HexGenerator
    {
        public int res = 256;
        
        private SpriteRenderer _sr;

        [ContextMenu("Awake")]
        protected void Awake()
        {
            renderType = HexRenderType.Sprite;
            LoadComponents();
        }
        
        private void LoadComponents()
        {
            _sr = GetComponentInChildren<SpriteRenderer>();

            if (_sr != null) return;
            
            GameObject child = new("Hex Renderer");
            child.transform.SetParent(transform);
            _sr = child.AddComponent<SpriteRenderer>();
        }
        
    
        public override void GenerateHex()
        {
            int imgWidth = Mathf.CeilToInt(hexagon.Width * res);
            int imgHeight = Mathf.CeilToInt(hexagon.Height * res);
            Rect imgRect = new(0, 0, imgWidth, imgHeight);
            
            Texture2D tex = new(imgWidth, imgHeight) { filterMode = FilterMode.Point, alphaIsTransparency = true };
            Color[] colors = new Color[imgWidth * imgHeight];
        
            for (var y = 0; y < imgHeight; y++)
            for (var x = 0; x < imgWidth; x++)
            {
                bool isOn = hexagon.PointOnHex(new Vector2(x, y) / imgRect.size * hexagon.Rect.size - hexagon.Rect.center);
                colors[y * imgWidth + x] = isOn ? Color.white : Color.clear;
            }
    
            tex.SetPixels(colors);
            tex.Apply();
        
            _sr.sprite = Sprite.Create(tex, imgRect, Vector2.one * 0.5f, res, 0);
        }
        
        
        #region ASSET SAVING

        public override void SaveAsset() => 
            SaveSpriteAsset(_sr.sprite, $"Hex {hexagon.size}m [{res}x{res}]");

        private static void SaveSpriteAsset(Sprite sprite, string name = null)
        {
            string path = "Assets/Resources/Sprites/Hexagon/" + (name ?? sprite.name) + ".asset";
            UnityEditor.AssetDatabase.CreateAsset(sprite, path);
        }

        #endregion
    }
}
