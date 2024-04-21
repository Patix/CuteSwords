using Content.Scripts.Game.Navigation;
using Sirenix.OdinInspector;
using UnityEngine;

public class NavMeshAgentController : MonoBehaviour
{
    [SerializeField]                  private NavMeshAgentAndObstacleHost m_NavmeshAgentPrefab;
    [SerializeField, HideInInspector] private NavMeshAgentAndObstacleHost m_NavMeshAgentAndObstacleHost;
    [SerializeField]                  private Rigidbody2D                 m_SelfTransform;
    [SerializeField]                  private Animator                    m_animator;
    [SerializeField]                  private string                      m_OffMeshLinkAnimationParameter;

    private void Awake()
    {
        UpdateMirror();
    }

    private void FixedUpdate()
    {
        m_NavMeshAgentAndObstacleHost.WriteTo(m_SelfTransform);
        if (m_animator      && m_animator.enabled) m_animator.SetBool(m_OffMeshLinkAnimationParameter, m_NavMeshAgentAndObstacleHost.IsOnOffMeshLink);
    }

    public void SetDestination(Vector2 destination) => m_NavMeshAgentAndObstacleHost.SetDestination(destination);

    [Button]
    private void UpdateMirror()
    {
        if (!m_NavMeshAgentAndObstacleHost)
        {
            var navigationAgentsContainer = GameObject.FindGameObjectWithTag("NavigationAgents").transform;
            m_NavMeshAgentAndObstacleHost = Instantiate(m_NavmeshAgentPrefab.gameObject, navigationAgentsContainer).GetComponent <NavMeshAgentAndObstacleHost>();
        }

        m_NavMeshAgentAndObstacleHost.Position = m_SelfTransform.position;
    }
}