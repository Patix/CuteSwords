using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Content.Scripts.Game.Navigation
{
    public class MouseClickPosition : MonoBehaviour
    {
        private                  Camera         camera;
        private                  ParticleSystem ps;

        public UnityEvent <RaycastHit> OnMouseLeftClick;
        public UnityEvent <RaycastHit> OnMouseLeftHold;
        public UnityEvent <RaycastHit> OnMouseRightClick;
        public UnityEvent <RaycastHit> OnMouseRightHold;
        private void Update()
        {
            if(float.IsInfinity(Input.mousePosition.x)) return;
            
            if(!camera || !camera.enabled) camera = Camera.main;
            if (!ps) ps                           = GetComponent <ParticleSystem>();
            
            if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out var result, 10000)) return;
        
            
            Debug.DrawLine(camera.transform.position, result.point, Color.red);
       
            var clickPosition = result.point;
                
            if(Input.GetKeyDown(KeyCode.Mouse0)) OnMouseLeftClick?.Invoke(result);
            else if(Input.GetKey(KeyCode.Mouse0)) OnMouseLeftHold?.Invoke(result);

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                transform.position = clickPosition;
                ps.Emit(4);
                OnMouseRightClick?.Invoke(result);
            }
            else if(Input.GetKey(KeyCode.Mouse1)) OnMouseRightHold?.Invoke(result);
        }
    }
}