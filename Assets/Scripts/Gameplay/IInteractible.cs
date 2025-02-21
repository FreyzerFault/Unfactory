using UnityEngine;

namespace Gameplay
{
    public interface IInteractible
    {
        public void OnHover();
        public void OnExit();
        public void OnClick();
        public void OnRelease();
        
        public bool IsCursorOnObject();
        public bool WorldPointOnObject(Vector3 point);
    }
}
