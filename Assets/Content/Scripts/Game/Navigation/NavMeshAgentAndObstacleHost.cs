using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Vector = System.Numerics.Vector3;

namespace Content.Scripts.Game.Navigation
{
    public class NavMeshAgentAndObstacleHost : MonoBehaviour
    {
        private static int Priority;
        
        [SerializeField] private NavMeshAgent    m_NavmeshAgent;
        [SerializeField] private NavMeshObstacle m_NavMeshObstacle;
        [SerializeField] private Vector3         Offset;
        
        private float   lastSwitch = float.MinValue;

        [SerializeField] private bool    SwitchToObstacleOnStop;
        [SerializeField] private bool    MakeKinematicOnMove;
        
        public                   Vector3 Position        { get => transform.position + Offset; set => transform.position = value - Offset; }
        public                   bool    IsOnOffMeshLink => m_NavmeshAgent.isOnOffMeshLink;
        
        private void FixedUpdate()
        {
            ChangeAgentToObstacleIfStationary();
        }

        private void ChangeAgentToObstacleIfStationary()
        {
            if(!SwitchToObstacleOnStop) return;
            
            if (Time.realtimeSinceStartup - lastSwitch > m_NavMeshObstacle.carvingTimeToStationary && !NavigationIsActive)
            {
                var navigationIsActive = NavigationIsActive;
                
                if (!navigationIsActive)
                {
                    lastSwitch             = Time.realtimeSinceStartup;
                }

                StartCoroutine(SetNavmeshActiveSafe(navigationIsActive));
            }
        }

        public bool NavigationIsActive => m_NavmeshAgent.enabled && !m_NavmeshAgent.isStopped && m_NavmeshAgent.hasPath;

        private void Awake()
        {
            m_NavmeshAgent.avoidancePriority = 99 -(++Priority % 99);
            ChangeAgentToObstacleIfStationary();
        }

        public void SetDestination(Vector3 destination)
        {
            if (m_NavmeshAgent.enabled) m_NavmeshAgent.SetDestination(destination);
            else StartCoroutine(Move());
            
            IEnumerator Move()
            {
                yield return SetNavmeshActiveSafe(true);
                m_NavmeshAgent.destination = destination;
            }
        }

   

        public void SyncNavmeshAgentToRigidbody(Rigidbody rigidbody)
        {
            m_NavmeshAgent.transform.position = rigidbody.position;
            
        }
        
        public void SyncRigidBodyToNavmeshAgent(Rigidbody rigidbody)
        {
            rigidbody.MovePosition(m_NavmeshAgent.transform.position);
            if(rigidbody.isKinematic) return;
            ClearVelocity(rigidbody);
        }

        public void SwitchKinematicState(Rigidbody mSelfTransform, bool originallyWasKinematic)
        {
            if(!MakeKinematicOnMove) return;
            
            if (NavigationIsActive) mSelfTransform.isKinematic = true;
            else mSelfTransform.isKinematic = originallyWasKinematic;

        }

        private IEnumerator SetNavmeshActiveSafe(bool active)
        {
            
            m_NavmeshAgent.enabled = m_NavMeshObstacle.enabled = false;
            yield return new WaitForEndOfFrame();
            m_NavmeshAgent.enabled    = active;
            m_NavMeshObstacle.enabled = !m_NavmeshAgent.enabled;
        }
        
       
        private void ClearVelocity(Rigidbody rigidbody)
        {
            rigidbody.velocity        = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}