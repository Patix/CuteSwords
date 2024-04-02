using System;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace InventoryAndEquipment
{
    [Serializable]
    public class CharacterMovementModule
    {
        [SerializeField] private float       m_MovementSpeed=12;
        [SerializeField] private Rigidbody2D m_RigidBody2D;
        
        private                  Character         character;
        private                  float             currentSpeed;
        private                  CharacterControls input;
        private                  Vector2           MovementVector { get; set; }

        public Vector2 Velocity=>m_RigidBody2D.velocity;
        
        public void Initialize(Character character, Rigidbody2D rigidbody2D)
        {
            this.m_RigidBody2D = rigidbody2D;
            input            = new CharacterControls();
            input.Enable();
            input.Character.Movement.performed += OnInputEvent;
            input.Character.Movement.canceled  += OnInputCanceled;

            this.character = character;
        }

        private void OnInputCanceled(InputAction.CallbackContext obj)          { MovementVector = obj.ReadValue <Vector2>(); }
        private void OnInputEvent(InputAction.CallbackContext    inputContext) { MovementVector = inputContext.ReadValue <Vector2>(); }

        public void FixedUpdate()
        {
            if (character.State != Character.StateTypes.Interacting)
            {
                if (MovementVector.magnitude > 0)
                {
                    character.State = Character.StateTypes.Moving;
                }
                else
                {
                    character.State = Character.StateTypes.Idle;
                }

                m_RigidBody2D.velocity = m_MovementSpeed * MovementVector;
            }
        }
    }
}