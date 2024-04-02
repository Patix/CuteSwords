using System;
using Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace InventoryAndEquipment
{
    [Serializable]
    public class CharacterAnimationAndAudioModule
    {
        [SerializeField] private Animator    m_Animator;
        [SerializeField] private AudioSource m_FootstepAudio;
        [SerializeField] private AudioSource m_EventAudio;
            
        
        private                Character               character;
        private                CharacterMovementModule movementModule;
        

        public void Initialize(Character character, CharacterMovementModule movementModule)
        {
            this.character      = character;
            this.movementModule = movementModule;
        }

        public void Update()
        {
            
            if(movementModule.Velocity.x  > 0) m_Animator.transform.localEulerAngles = Vector3.zero; //Rotate Left
            if (movementModule.Velocity.x < 0) m_Animator.transform.localEulerAngles = 180 * Vector3.up; // Rotate Right
            
            m_FootstepAudio.mute = character.State != Character.StateTypes.Moving;
            if(!m_FootstepAudio.isPlaying) m_FootstepAudio.Play();
        
            m_Animator.SetFloat("MovementSpeed", Mathf.Max(movementModule.Velocity.magnitude));
        }

        public void PlayAttackAnimation()
        {
            m_Animator.SetTrigger("Attack");
        }

        public void ProcessAnimationMarker(AnimationEvent animationEvent)
        {
            if (animationEvent.stringParameter == "Attack Finished")
            {
                character.State = Character.StateTypes.Idle;
                
            }
        }
    }
}