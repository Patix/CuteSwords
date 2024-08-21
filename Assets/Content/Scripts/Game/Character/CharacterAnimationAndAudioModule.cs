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
            
            if(movementModule.Velocity.x  > 0) LookRight(); //Rotate Right
            if (movementModule.Velocity.x < 0) LookLeft(); // Rotate Left

            m_FootstepAudio.mute = movementModule.Velocity.magnitude < 0.01f;
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

        private void LookRight() { m_Animator.transform.localEulerAngles = Vector3.zero; }
        private void LookLeft()  { m_Animator.transform.localEulerAngles = 180 * Vector3.up; }
        public void LookAt(Transform target)
        {
            if (target.transform.position.x > m_Animator.transform.position.x) LookRight(); 
            else if (target.transform.position.x < m_Animator.transform.position.x) LookLeft();
            // if same position -do nothing
        }
    }
}