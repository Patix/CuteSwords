using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Interaction
{
    public class PhysicsInteractiveBehaviour : MonoBehaviour
    {
      
       
        [SerializeField] private UnityEvent OnInteractionStart;
        [SerializeField] private UnityEvent OnInteractionEnd;

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnInteractionStart?.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            OnInteractionEnd?.Invoke();
            
        }
    }
}