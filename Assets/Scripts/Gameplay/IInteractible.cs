using UnityEngine;

namespace Gameplay
{
    public interface IInteractible
    {
        public void OnHover();
        public void OnClick();
        public void OnExit();
        
        public bool IsCursorOnObject();
        public bool WorldPointOnObject(Vector3 point);
    }
}
