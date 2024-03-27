using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InventoryAndEquipment
{
    [Serializable]
    public class CharacterMovementModule
    {
        [SerializeField] private float             movementSpeedtSpeed = 1;
        private                  Character         character;
        private                  float             currentSpeed;
        [SerializeField] private Rigidbody2D       rigidbody2D;
        private                  CharacterControls input;
        public                   Vector2           MovementVector { get; private set; }

        public void Initialize(Character character, Rigidbody2D rigidbody2D)
        {
            this.rigidbody2D = rigidbody2D;
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
            if (character.State != Character.StateTypes.Attacking)
            {
                if (MovementVector.magnitude > 0)
                {
                    character.State = Character.StateTypes.Moving;
                }
                else
                {
                    character.State = Character.StateTypes.Idle;
                }

                rigidbody2D.position += movementSpeedtSpeed * MovementVector * Time.fixedDeltaTime;
            }
        }
    }
}