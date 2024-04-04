using System;
using EventManagement;
using Interaction;
using InventoryAndEquipment;
using Patik.CodeArchitecture.Patterns;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using Character = InventoryAndEquipment.Character;

public class CharacterController : SingletonMonoBehaviour <CharacterController>
{
    public                   Character                        Character => Character.Instance;
    [SerializeField] private CharacterMovementModule          movementModule;
    [SerializeField] private CharacterAnimationAndAudioModule animationModule;
    [SerializeField] private Rigidbody2D                      rigidbody2D;

    private EventListeners eventListeners;

    private void OnEnable()
    {
        eventListeners??= new EventListeners((GameEvents.Equipment_Updated, OnEquipmentUpdate));
        eventListeners.SubscribeAll();
    }
    private void OnDisable()
    {
        eventListeners.UnsubscribeAll();
    }
  
    protected override void Awake()
    {
        base.Awake();
        movementModule.Initialize(Character , rigidbody2D);
        animationModule.Initialize(Character, movementModule);
    }
    
    private void Update()      { animationModule.Update(); }
    private void FixedUpdate() { movementModule.FixedUpdate(); }

    
    public void Interact(ClickableInteractiveBehaviourBase clickableUnit)
    {
        if (clickableUnit.InteractionType != InteractionType.Speak) // Instant Interaction - Does not Wait For Anything 
        {
            Character.State = Character.StateTypes.Interacting;
        } 
       
        animationModule.LookAt(clickableUnit.transform);
        
        if (clickableUnit.InteractionType == InteractionType.Kill)
        {
            animationModule.PlayAttackAnimation();
        }
    }

    public void OnAnimationEvent(AnimationEvent animationEvent) => animationModule.ProcessAnimationMarker(animationEvent);
   
    
    private void OnEquipmentUpdate()
    {
        foreach (var equipment in Character.Equipment) equipment.Equip(transform);
    }
}