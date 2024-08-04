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
        [SerializeField] private Rigidbody   m_RigidBody;
        
        private                  Character         character;
        private                  float             currentSpeed;
        private                  CharacterControls input;
        private                  Vector3           MovementVector { get; set; }

        public Vector3 Velocity=>m_RigidBody.velocity;
        
        public void Initialize(Character character, Rigidbody rigidbody)
        {
            this.m_RigidBody = rigidbody;
            input              = new CharacterControls();
            input.Enable();
            input.Character.Movement.performed += OnInputEvent;
            input.Character.Movement.canceled  += OnInputCanceled;

            this.character = character;
        }

        private void OnInputCanceled(InputAction.CallbackContext obj)
        {
            MovementVector = Vector3.zero;
        }

        private void OnInputEvent(InputAction.CallbackContext inputContext)
        {
            var inputVector = inputContext.ReadValue <Vector2>();
            MovementVector = new Vector3(inputVector.x,0,inputVector.y);
        }

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

                var newVelocity = m_RigidBody.velocity;
                (newVelocity.x, newVelocity.z) = (m_MovementSpeed*MovementVector.x, m_MovementSpeed* MovementVector.z);
                m_RigidBody.velocity           = newVelocity;
            }
        }
    }
}