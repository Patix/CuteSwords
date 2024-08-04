using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Content.Scripts.Game.Navigation
{
    public class MouseClickPosition : MonoBehaviour
    {
        private                  Camera         camera;
        private                  ParticleSystem ps;

        public UnityEvent <Vector3> OnMouseLeftClick;
        public UnityEvent <Vector3> OnMouseLeftHold;
        public UnityEvent <Vector3> OnMouseRightClick;
        public UnityEvent <Vector3> OnMouseRightHold;
        private void Update()
        {
            if(!camera || !camera.enabled) camera = Camera.main;
            if (!ps) ps                           = GetComponent <ParticleSystem>();
          
            if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out var result, 10000)) return;
        
            
            Debug.DrawLine(camera.transform.position, result.point, Color.red);
       
            var clickPosition = result.point;
                
            if(Input.GetKeyDown(KeyCode.Mouse0)) OnMouseLeftClick?.Invoke(clickPosition);
            else if(Input.GetKey(KeyCode.Mouse0)) OnMouseLeftHold?.Invoke(clickPosition);

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                transform.position = clickPosition;
                ps.Emit(4);
                OnMouseRightClick?.Invoke(clickPosition);
            }
            else if(Input.GetKey(KeyCode.Mouse1)) OnMouseRightHold?.Invoke(clickPosition);
        }
    }
}