using DavidUtils.ExtensionMethods;
using UnityEngine;

namespace Hex.Hex_Generation
{
    [ExecuteAlways]
    public abstract class HexGenerator : MonoBehaviour
    {
        public enum HexRenderType { Sprite, Mesh } 
        public HexRenderType renderType = HexRenderType.Mesh;

        public Hexagon hexagon;

        protected Vector2[] Vertices => hexagon.Vertices;
        private Vector3[] WorldVertices => hexagon.WorldVertices(transform);

        private bool _needRegenerate = true;

        private void OnValidate() => _needRegenerate = true;

        private void Update()
        {
            if (!_needRegenerate) return;
            
            hexagon.UpdateVertices();
            GenerateHex();
            _needRegenerate = false;
        }
        
        public virtual void GenerateHex() => _needRegenerate = false;

        public abstract void SaveAsset();

        
        
        
        
        #region DEBUG
        
        private void OnDrawGizmos()
        {
            const float pointSize = 0.02f;
            
            DrawGizmosDiagonals(pointSize);
            DrawGizmosVertices(pointSize);
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
        
        #endregion
    }
}
