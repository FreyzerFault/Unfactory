using System.Linq;
using DavidUtils.ExtensionMethods;
using UnityEngine;

namespace Hex.Hex_Generation
{
    public class HexMeshGenerator : HexGenerator
    {
        private MeshRenderer _mr;
        private MeshFilter _mf;
        
        [ContextMenu("Awake")]
        protected void Awake()
        {
            renderType = HexRenderType.Mesh;
            LoadComponents();
        }
        
        private void LoadComponents()
        {
            _mr = GetComponentInChildren<MeshRenderer>();
            _mf = GetComponentInChildren<MeshFilter>();

            if (_mr != null) return;
            
            GameObject child = new("Hex Renderer");
            child.transform.SetParent(transform);
            _mr = child.AddComponent<MeshRenderer>();
            _mf = child.AddComponent<MeshFilter>();
        }

        public override void GenerateHex()
        {
            Vector3 center = Vector3.zero;
            _mf.sharedMesh = new Mesh
            {
                vertices = Vertices.ToV3xy().Append(center).ToArray(),
                triangles = new[]
                {
                    6, 5, 4,
                    6, 4, 3,
                    6, 3, 2,
                    6, 2, 1,
                    6, 1, 0,
                    6, 0, 5,
                },
                uv = Vertices.Select(v => v / hexagon.Rect.size + Vector2.one * 0.5f).Append(Vector2.one * 0.5f).ToArray(),
                normals = Vector3.back.ToFilledArray(7).ToArray(),
                name = $"Hex {hexagon.size}m"
            };
        }

        public override void SaveAsset() => SaveMeshAsset(_mf.sharedMesh);

        private void SaveMeshAsset(Mesh mesh)
        {
            var path = $"Assets/Resources/Models/Hexagon/{mesh.name}.asset";
            UnityEditor.AssetDatabase.CreateAsset(mesh, path);
            UnityEditor.AssetDatabase.SaveAssets();
        }

    }
}
