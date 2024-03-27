using System;
using UnityEngine;

namespace InventoryAndEquipment
{
    [Serializable]
    public class CharacterAnimationAndAudioModule
    {
        [SerializeField] private Animator                animator;
        private                  Character               character;
        private                  CharacterMovementModule movementModule;

        public void Initialize(Character character, CharacterMovementModule movementModule)
        {
            this.character      = character;
            this.movementModule = movementModule;
        }

        public void Update()
        {
            
            if (character.State == Character.StateTypes.Moving)
            {
                if( movementModule.MovementVector.x > 0) animator.transform.localEulerAngles = Vector3.zero;
                if (movementModule.MovementVector.x < 0) animator.transform.localEulerAngles = 180 * Vector3.up;
                
            }
          
            animator.SetFloat("MovementSpeed", Mathf.Max(movementModule.MovementVector.magnitude));
        }
    }
}