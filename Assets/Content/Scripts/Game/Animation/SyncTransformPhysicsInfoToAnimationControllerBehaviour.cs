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