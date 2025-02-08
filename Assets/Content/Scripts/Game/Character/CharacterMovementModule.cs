using System;
using System.Text;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace InventoryAndEquipment
{
    [Serializable]
    public class CharacterMovementModule
    {
        [SerializeField] private float           m_MovementSpeed=12;
        [SerializeField] private Rigidbody       m_RigidBody;
        [SerializeField] private CapsuleCollider m_CapsuleCollider;
        [SerializeField] private float           m_GroundCheckDistance = 0.1f;
        [SerializeField] private  Vector3           m_GroundCheckOrigin = Vector3.zero;
        
        private Character         character;
        private float             currentSpeed;
        private CharacterControls input;
        private Vector2           MovementInput { get; set; }
        private Vector3           MovementInputAddedVelocityLastFrame;
        public  Vector3           Velocity   =>m_RigidBody.linearVelocity;
        public  bool              IsOnLadder { get; set; } = false;
        
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
            MovementInput = Vector3.zero;
        }

        private void OnInputEvent(InputAction.CallbackContext inputContext)
        {
            MovementInput = inputContext.ReadValue <Vector2>();
        }

        public void FixedUpdate()
        {
            if (character.State != Character.StateTypes.Interacting)
            {
                if (MovementInput.magnitude > 0)
                {
                    character.State = Character.StateTypes.Moving;
                }
                else
                {
                    character.State = Character.StateTypes.Idle;
                }
                ApplyInputAsForce();
            }
        }

        private void ApplyInputAsForce()
        {
            var verticalInput= MovementInput.y;
            
            var movementDirection = new Vector3(MovementInput.x, 0, MovementInput.y);
            var movementVelocity  = movementDirection * m_MovementSpeed;

            IsTouching(Vector3.forward);
            IsTouching(Vector3.down);
            
      
            if (IsOnLadder)
            {
                if (verticalInput > 0 && IsTouching(Vector3.forward) || verticalInput < 0 && !IsTouching(Vector3.down) &&IsTouching(Vector3.forward))
                {
                    movementVelocity.y = movementVelocity.z;
                    movementVelocity.z = 0; //Isometric Movement Half Forward , Half Up 
                }
            }
            else
            {
                if (movementVelocity.y != 0) movementVelocity.z = movementVelocity.y;
                movementVelocity.y = m_RigidBody.linearVelocity.y; // Original Gravity Affected Y 
            }
            
            m_RigidBody.linearVelocity = movementVelocity;
        }

        private bool IsTouching(Vector3 direction)
        { 
            var  capsuleColliderCenter   = m_RigidBody.transform.TransformPoint(m_CapsuleCollider.center);
            var  radius                  = m_CapsuleCollider.radius * m_CapsuleCollider.transform.lossyScale.z;
            var  rayOrigin               = capsuleColliderCenter + m_GroundCheckOrigin*radius;
        
          
            bool isTouching = Physics.Raycast(rayOrigin,direction, m_GroundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            Debug.DrawLine(rayOrigin,rayOrigin+direction,isTouching?Color.red:Color.green);
            return isTouching;
        }
        
        private void ApplyInputAsForce2(Vector3 movementDirection)
        {
            var linearVelocityBeforeApplyingInputForce = m_RigidBody.linearVelocity;
            
            var inputVelocity = movementDirection * m_MovementSpeed;

            var newVelocity = linearVelocityBeforeApplyingInputForce + inputVelocity;
            
            if (linearVelocityBeforeApplyingInputForce.magnitude > MovementInputAddedVelocityLastFrame.magnitude)
            {
                newVelocity -= MovementInputAddedVelocityLastFrame;
            }

            MovementInputAddedVelocityLastFrame = newVelocity - linearVelocityBeforeApplyingInputForce;

            m_RigidBody.linearVelocity = newVelocity;
        }
    }
}