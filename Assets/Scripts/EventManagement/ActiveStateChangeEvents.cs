using UnityEngine;
using UnityEngine.Events;

namespace EventManagement
{
    public class ActiveStateChangeEvents: MonoBehaviour
    {
        [SerializeField] private UnityEvent OnEnabled;
        [SerializeField] private UnityEvent OnDisabled;
        public                   void       OnEnable()  => OnEnabled?.Invoke();
        public                   void       OnDisable() => OnDisabled?.Invoke();
    }
}