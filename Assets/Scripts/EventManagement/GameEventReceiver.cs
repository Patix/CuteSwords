using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EventManagement
{
    
    public class GameEventReceiver: MonoBehaviour
    {
        [SerializeField] private List <Data> Triggers;
        
        private                  EventListeners           eventListeners;
        private void OnEnable()
        {
            eventListeners ??= new EventListeners(Triggers.Select(x => x.AsEventListener));
            eventListeners.SubscribeAll();
        }

        private void OnDisable()
        {
            eventListeners?.UnsubscribeAll();
        }

        [Serializable]
        class Data
        {
            
            [SerializeField] public  GameEvents           GameEvent;
            [SerializeField] private UnityEvent           UnityAction;
            
            public                  (GameEvents, Action) AsEventListener => (GameEvent, UnityAction.Invoke);}
    }
}