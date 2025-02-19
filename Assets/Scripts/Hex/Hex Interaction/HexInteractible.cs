using System;
using DavidUtils.ExtensionMethods;
using DavidUtils.Geometry;
using Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hex.Hex_Interaction
{
    public class HexInteractible : MonoBehaviour, IInteractible
    {
        public Hexagon hexagon;

        public Material baseMaterial;
        public Material hoverMaterial;
        [FormerlySerializedAs("selectMaterial")] public Material clickMaterial;

        private Material Material
        {
            get => mr.material;
            set => mr.material = value;
        }

        private MeshRenderer mr;
        private Camera cam;
        
        private void Awake()
        {
            mr = GetComponent<MeshRenderer>();
            cam = Camera.main;
        }


        private void OnEnable() => Material = baseMaterial;

        private void Update()
        {
            if (!IsCursorOnObject())
            {
                OnExit();
                return;
            }
            
            OnHover();
            
            if (Input.GetMouseButton(0))
                OnClick();
        }

        public void OnHover() => Material = hoverMaterial;
        public void OnClick() => Material = clickMaterial;
        public void OnExit() => Material = baseMaterial;
        
        public bool IsCursorOnObject() => 
            RaycastCursor_HexPlane(out Vector3 p) && WorldPointOnObject(p);

        public bool WorldPointOnObject(Vector3 point) => 
            hexagon.PointOnHex(transform.ToLocal(point));


        #region CURSOR RAYCASTING

        private Plane HexPlane => new(-transform.forward, transform.position);
        private Ray MouseRay => cam.ScreenPointToRay(Input.mousePosition);
        
        // Raycast with Hexagon Plane 
        private bool RaycastCursor_HexPlane(out Vector3 intersection)
        {
            intersection = Vector3.zero;
            cam ??= Camera.main;
            return cam && GeometryUtils.IntersectionRayPlane(MouseRay, HexPlane, out intersection);
        }
        
        #endregion
        
        

        private void OnDrawGizmos()
        {
            const float pointSize = 0.05f;
            DrawGizmosCursorInHexPlane(pointSize);
        }

        private void DrawGizmosCursorInHexPlane(float pointSize = 0.05f)
        {
            if (!RaycastCursor_HexPlane(out Vector3 p)) return;
            bool pointOnHex = WorldPointOnObject(p);
            Gizmos.color = pointOnHex ? Color.green : Color.red;
            Gizmos.DrawSphere(p, pointSize);
            
            Gizmos.color = pointOnHex ? Color.green : Color.red;
            Gizmos.DrawLine(MouseRay.origin, MouseRay.origin + MouseRay.direction * 10);
        }

    }
}
