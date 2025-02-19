using Hex.Hex_Generation;
using UnityEditor;
using UnityEngine;

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
                gen.GenerateHex();
            
            if (GUILayout.Button($"Create {gen.renderType} Asset")) 
                gen.SaveAsset();
        }
    }
    
}
