using System;
using System.Linq;
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
        
        public bool checkWedges = true;
        public Hexagon.Orientation wedgeOrientation;
        [FormerlySerializedAs("wedgePoints")] public Vector3[] wedgeTri;
        
        private bool isHovered;
        private bool isClicked;

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
            bool cursorOnObject = checkWedges
                ? IsCursorOnWedge()
                : IsCursorOnObject();
            
            bool hover = cursorOnObject;
            bool click = Input.GetMouseButton(0);
            
            if (hover && !isHovered) OnHover();
            if (!hover && isHovered) OnExit();
            if (click && !isClicked) OnClick();
            if (!click && isClicked) OnRelease();
            
            isHovered = hover;
            isClicked = click;
        }

        public void OnHover() => Material = hoverMaterial;

        public void OnExit() => Material = baseMaterial;
        
        public void OnClick() => Material = isHovered ? clickMaterial : baseMaterial;

        public void OnRelease() => Material = isHovered ? hoverMaterial : baseMaterial;

        public bool IsCursorOnObject() => 
            RaycastCursor_HexPlane(out Vector3 p) && WorldPointOnObject(p);

        public bool WorldPointOnObject(Vector3 point) => 
            hexagon.PointOnHex(transform.ToLocal(point));

        public bool IsCursorOnWedge()
        {
            Vector2[] localEdge = Array.Empty<Vector2>();
            bool onWedge = RaycastCursor_HexPlane(out Vector3 p) &&
                hexagon.PointOnWedge(transform.ToLocal(p), out wedgeOrientation, out localEdge);

            wedgeTri = localEdge.Select(e => transform.ToWorld(e)).Append(transform.position).Reverse().ToArray();
            
            return onWedge;
        }
        

        #region APPEARANCE
        
        public Material baseMaterial;
        public Material hoverMaterial;
        public Material clickMaterial;

        private Material Material
        {
            get => mr.material;
            set => mr.material = value;
        }
        
        #endregion
        
        
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


        #region DEBUG

        private const float PointSize = 0.05f;

        private void OnDrawGizmos()
        {
            if (!RaycastCursor_HexPlane(out Vector3 p)) return;
            
            DrawGizmosCursorRaycastInHexPlane(p, PointSize);
            
            if (checkWedges) DrawGizmosWedges();
        }
        
        private void DrawGizmosCursorRaycastInHexPlane(Vector3 cursorInWorld, float pointSize = 0.05f)
        {
            Gizmos.color = isHovered ? Color.green : Color.red;
            Gizmos.DrawSphere(cursorInWorld, pointSize);
            Gizmos.DrawLine(MouseRay.origin, MouseRay.origin + MouseRay.direction * 10);
        }
        
        private void DrawGizmosWedges()
        {
            Handles.color = Color.yellow;
            Handles.DrawAAConvexPolygon(wedgeTri);

            if (!isHovered) return;
            
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(5, wedgeTri.Append(wedgeTri.First()).ToArray());
        }

        #endregion
    }
}
