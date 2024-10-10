using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;

namespace EventManagement
{
    public class GameEventListeners
    {
        private (GameEvents Event, Action Action) [] listeners;


        public GameEventListeners(IEnumerable <(GameEvents, Action)> eventListeners) : this(eventListeners.ToArray()){}
        
        public GameEventListeners(params (GameEvents,Action)[] eventListeners)
        {
            listeners = eventListeners;
        }
        
        public void SubscribeAll()
        {
            foreach (var listener in listeners)
            {
                listener.Event.Subscribe(listener.Action);
            }
        }
        
        public void UnsubscribeAll()
        {
            foreach (var listener in listeners)
            {
                listener.Event.Unsubscribe(listener.Action);
            }
        }
    }
}