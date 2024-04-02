using System;
using System.Collections;
using System.Collections.Generic;
using Imported;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimationEventListenerAndHandler : MonoBehaviour
{
    [SerializeField] private MarkerReachListeners m_listeners;
    private                  Animator             animator;
   
    private                  void                         Awake() { animator = GetComponent <Animator>(); }

    public void NotifyListenersMarkerReachedUsingStringParameter(AnimationEvent animationEvent)
    {
        
        if (m_listeners.TryGetValue(animationEvent.stringParameter , out var unityEvent))
        {
            unityEvent?.Invoke(animationEvent); 
        }
    }
    public                   void                        SelfSetBoolFromInt(AnimationEvent   animationEvent) { animator.SetBool(animationEvent.stringParameter, animationEvent.intParameter   == 1); }
    public                   void                        SelfSetBoolFromFloat(AnimationEvent animationEvent) { animator.SetBool(animationEvent.stringParameter, animationEvent.floatParameter == 1); }
    public                   void                        SelfSetTrigger(AnimationEvent       animationEvent) { animator.SetTrigger(animationEvent.stringParameter); }
    public                   void                        SelfSetInt(AnimationEvent           animationEvent) { animator.SetInteger(animationEvent.stringParameter, animationEvent.intParameter); }
    public                   void                        SelfSetFloat(AnimationEvent         animationEvent) { animator.SetFloat(animationEvent.stringParameter, animationEvent.floatParameter); }
    

    [Serializable]
    public class MarkerReachListeners : SerializedDictionary <string, UnityEvent<AnimationEvent>>{}
   
}