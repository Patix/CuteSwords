using UnityEngine;
using UnityEngine.AI;

namespace Content.Scripts.Game.Navigation
{
    public class NavMeshAgentAndObstacleHost : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent    m_NavmeshAgent;
        [SerializeField] private NavMeshObstacle m_NavMeshObstacle;
        [SerializeField] private Vector2         Offset;
        
        private float   lastSwitch = float.MinValue;
        
        public Vector2 Position        { get => (Vector2)transform.position + Offset; set => transform.position = value - Offset; }
        public bool    IsOnOffMeshLink => m_NavmeshAgent.isOnOffMeshLink;
        
        private void FixedUpdate()
        {
            ChangeAgentToObstacleIfStationary();
        }

        private void ChangeAgentToObstacleIfStationary()
        {
            if (Time.realtimeSinceStartup - lastSwitch > m_NavMeshObstacle.carvingTimeToStationary)
            {
                lastSwitch                = Time.realtimeSinceStartup;
                m_NavMeshObstacle.enabled = (!m_NavmeshAgent.enabled || m_NavmeshAgent.isStopped || !m_NavmeshAgent.hasPath);
                m_NavmeshAgent.enabled    = !m_NavMeshObstacle.enabled;
            }
        }

        private void Awake()
        {
            m_NavmeshAgent.updateUpAxis = false;
            ChangeAgentToObstacleIfStationary();
        }

        public void SetDestination(Vector2 destination)
        {
            m_NavMeshObstacle.enabled  = false;
            m_NavmeshAgent.enabled     = true;
            m_NavmeshAgent.destination = destination;
        }
        


        public void WriteTo(Rigidbody2D rigidbody2D) => rigidbody2D.MovePosition(Position);
        public void WriteTo(Transform transform) => transform.position = Position;

    }
}