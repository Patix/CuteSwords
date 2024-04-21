using System;
using System.Collections.Generic;
using Content.Scripts.Game.Architecture;
using Imported;
using TMPro;
using UnityEngine;

namespace Content.Scripts.Game.Animation
{

    public class SyncTransformPhysicsInfoToAnimationControllerBehaviour : MonoBehaviourContainerFor <SyncTransformPhysicsInfoToAnimationController>
    {
        private void Awake()       => InternalComponent.Initialize();
        private void FixedUpdate() => InternalComponent.Update();

        private void Update()
        {
            AnimationClip clip;

       
            
    
        }

        private void OnAnimatorMoves()
        {
            InternalComponent.MAnimator.applyRootMotion = true;
            InternalComponent.MAnimator.SetLookAtPosition(Vector3.left * 100);
            
            Debug.Log((InternalComponent.MAnimator.deltaRotation.eulerAngles));
            //InternalComponent.MAnimator.rootRotation = InternalComponent.MAnimator.deltaRotation;
            transform.rotation = InternalComponent.MAnimator.rootRotation;
            Debug.Log($"Delta {(InternalComponent.MAnimator.deltaRotation.eulerAngles)} : Root {InternalComponent.MAnimator.rootRotation.eulerAngles}");
            // Debug.Log(InternalComponent.MAnimator.rootRotation.eulerAngles += InternalComponent.MAnimator.deltaRotation.eulerAngles);
            
        }

        private void OnDidApplyAnimationProperties()
        {
            Debug.Log("Param Appy");
        }
    }
   
    [Serializable]
    public class SyncTransformPhysicsInfoToAnimationController
    {
        [SerializeField] private Transform m_Transform;
        [SerializeField] private Animator  m_Animator;

        [SerializeField, Range(1,10)] private int FixedUpdateSamplingInterval=1;

        [SerializeField] private SyncOptions          m_SyncOptions;
        private                  TransformPhysicsInfo m_TransformPhysicsInfo;

        private int      currentFrame;
        public  Animator MAnimator => m_Animator;

        public void Initialize()
        {
            m_TransformPhysicsInfo       = new TransformPhysicsInfo(m_Transform);
            m_TransformPhysicsInfo.Update();
        }

        public void OnAnimatorMove()
        {
            MAnimator.applyRootMotion          = false;
            MAnimator.ApplyBuiltinRootMotion();
            Debug.Log($"{MAnimator.rootRotation.eulerAngles} - {MAnimator.rootRotation.eulerAngles==m_Transform.eulerAngles}" ,m_Transform);
        }

        public void OnApply()
        {
            var x = MAnimator.rootRotation;
        }
        
        public void Update()
        {
            if (++currentFrame>=FixedUpdateSamplingInterval)
            {
                Sync();
                currentFrame = 0;
                m_TransformPhysicsInfo.Update();
                
            }
        }

        protected void Sync()
        {
            if(m_SyncOptions.HasFlag(SyncOptions.VelocityX)) MAnimator.SetFloat("VelocityX", m_TransformPhysicsInfo.Velocity.x);
            if(m_SyncOptions.HasFlag(SyncOptions.VelocityY)) MAnimator.SetFloat("VelocityY", m_TransformPhysicsInfo.Velocity.y);
        }
        
        
        [Flags]
        public enum SyncOptions
        {
            None=0,
            VelocityX=1,
            VelocityY=1<<1,
            VelocityZ=1<<2,
            AngularX=1<<3,
            AngularY=1<<4,
            AngularZ=1<<5
        }
        
    }
}