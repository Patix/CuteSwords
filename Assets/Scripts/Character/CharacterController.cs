using System;
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

    protected override void Awake()
    {
        base.Awake();
        movementModule.Initialize(Character , rigidbody2D);
        animationModule.Initialize(Character, movementModule);
    }

    private void Update()      { animationModule.Update(); }
    private void FixedUpdate() { movementModule.FixedUpdate(); }
}