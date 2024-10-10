using System;
using System.Collections.Generic;
using EventManagement;

namespace DefaultNamespace
{
    public static class GameEventManager
    {
        private static Dictionary <GameEvents, HashSet<Action>> _regisredListeners = new Dictionary <GameEvents, HashSet<Action>>();

        public static void Invoke(this GameEvents gameEvent)
        {
            if (_regisredListeners.TryGetValue(gameEvent,out var listeners))
            {
                foreach (var action in listeners)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }  
                }
            }
           
        }

        public static void Subscribe(this GameEvents gameEvent, Action action)
        {
            if(!_regisredListeners.ContainsKey(gameEvent))
                _regisredListeners.Add(gameEvent, new HashSet <Action>());

            _regisredListeners[gameEvent].Add(action);
        }

        public static void Unsubscribe(this GameEvents gameEvent, Action action)
        {
            if (_regisredListeners.TryGetValue(gameEvent, out var listeners))
            {
                listeners.Remove(action);
            }
        }
    }
}