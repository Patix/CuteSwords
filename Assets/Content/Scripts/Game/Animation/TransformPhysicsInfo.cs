using UnityEngine;

namespace Content.Scripts.Game.Animation
{
    public class TransformPhysicsInfo
    {
        private Transform m_TargetTransform;
        private Vector3 positionInPreviousFrame;
        private Vector3 rotationInPreviousFrame;

        private float   lastSampleTimeStamp;
        
        public  Vector3 Velocity        => (m_TargetTransform.position    - positionInPreviousFrame) / SamplingDeltaTime;
        public  Vector3 AngularVelocity => m_TargetTransform.eulerAngles - rotationInPreviousFrame  / SamplingDeltaTime;

        private float SamplingDeltaTime => Time.realtimeSinceStartup - lastSampleTimeStamp;
        
        public TransformPhysicsInfo(Transform mTargetTransform)
        {
            m_TargetTransform = mTargetTransform;
        }

        public void Update()
        {
            lastSampleTimeStamp     = Time.realtimeSinceStartup;
            positionInPreviousFrame = m_TargetTransform.position;
            rotationInPreviousFrame = m_TargetTransform.eulerAngles;
        }
    }
}