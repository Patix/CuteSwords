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

        public  bool IsBeingInteracted                     { get; private set; }
        
        private void OnCollisionEnter(Collision other) => OnTriggerEnter(other.collider);
        private void OnCollisionExit(Collision  other) => OnTriggerExit(other.collider);
     

        private void OnTriggerEnter(Collider other)
        {
            IsBeingInteracted = true;
            OnInteractionStart?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            IsBeingInteracted = false;
            OnInteractionEnd?.Invoke();
        }
    }
}