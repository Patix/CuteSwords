using EventManagement;
using Interaction;
using InventoryAndEquipment;
using Patik.CodeArchitecture.Patterns;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;
using Character = InventoryAndEquipment.Character;

public class CharacterController : SingletonMonoBehaviour <CharacterController>
{
    public                   Character                        Character => Character.Instance;
    [SerializeField] private CharacterMovementModule          movementModule;
    [SerializeField] private CharacterAnimationAndAudioModule animationModule;
    [SerializeField] private Rigidbody                        m_Rigidbody;

    private GameEventListeners gameEventListeners;
 
    private void OnEnable()
    {
        gameEventListeners ??= new GameEventListeners((GameEvents.Equipment_Updated, OnEquipmentUpdate));
        gameEventListeners.SubscribeAll();

        FindAnyObjectByType <TerrainSampler>().OnTileUpdated += animationModule.UpdateFootStepAudio;
    }

    private void OnDisable()
    {
        gameEventListeners.UnsubscribeAll();
        FindAnyObjectByType <TerrainSampler>().OnTileUpdated -= animationModule.UpdateFootStepAudio;
    }

    protected override void Awake()
    {
        base.Awake();
        movementModule.Initialize(Character, m_Rigidbody);
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
