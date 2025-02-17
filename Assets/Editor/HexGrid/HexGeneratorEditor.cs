using UnityEditor;
using UnityEngine;
using HexGrid;

namespace Editor.HexGrid
{
    [CustomEditor(typeof(HexGenerator), true)]
    public class HexGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            HexGenerator gen = (HexGenerator)target;

            DrawDefaultInspector();
            if (GUILayout.Button("Generate Hex")) 
                gen.RasterHexOnSprite();
        
            if (GUILayout.Button("Create Sprite")) 
                gen.CreateSpriteAsset();
        }
    }
}
