using System;
using System.Linq;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Content.Scripts.Game.Navigation
{
    public class MouseClickPosition : MonoBehaviour
    {
        private                  Camera         camera;
        private                  ParticleSystem ps;

        public UnityEvent <Vector2> OnMouseLeftClick;
        public UnityEvent <Vector2> OnMouseLeftHold;
        public UnityEvent <Vector2> OnMouseRightClick;
        public UnityEvent <Vector2> OnMouseRightHold;
        private void Update()
        {
            if(!camera || !camera.enabled) camera = Camera.main;
            if (!ps) ps                           = GetComponent <ParticleSystem>();
            
            var clickPosition = (Vector2) camera.ScreenToWorldPoint(Input.mousePosition);
        
            
            if(Input.GetKeyDown(KeyCode.Mouse0)) OnMouseLeftClick?.Invoke(clickPosition);
            else if(Input.GetKey(KeyCode.Mouse0)) OnMouseLeftHold?.Invoke(clickPosition);

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                transform.position = clickPosition;
                ps.Emit(4);
                OnMouseRightClick?.Invoke(clickPosition);
            }
            else if(Input.GetKey(KeyCode.Mouse1)) OnMouseRightHold?.Invoke(clickPosition); }
    }
}