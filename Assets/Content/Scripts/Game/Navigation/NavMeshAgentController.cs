using Content.Scripts.Game.Navigation;
using Sirenix.OdinInspector;
using UnityEngine;

public class NavMeshAgentController : MonoBehaviour
{
    [SerializeField]                  private NavMeshAgentAndObstacleHost m_NavmeshAgentPrefab;
    [SerializeField, HideInInspector] private NavMeshAgentAndObstacleHost m_NavMeshAgentAndObstacleHost;
    [SerializeField]                  private Rigidbody                   m_SelfTransform;
    [SerializeField]                  private Animator                    m_animator;
    [SerializeField]                  private string                      m_OffMeshLinkAnimationParameter;

    private bool originallyWasKinematic;

    public void SetDestination(Vector3 destination)
    {
        m_NavMeshAgentAndObstacleHost.SetDestination(destination);
    }

    private void Awake()
    {
        originallyWasKinematic = m_SelfTransform.isKinematic;
        UpdateMirror();
    }

    private void FixedUpdate()
    {
        Sync();
    }
    
    private void Sync()
    {
        m_NavMeshAgentAndObstacleHost.SwitchKinematicState(m_SelfTransform, originallyWasKinematic);
        
        if (m_NavMeshAgentAndObstacleHost.NavigationIsActive)
        {
            m_NavMeshAgentAndObstacleHost.SyncRigidBodyToNavmeshAgent(m_SelfTransform);
        }
        else
        {
            m_NavMeshAgentAndObstacleHost.SyncNavmeshAgentToRigidbody(m_SelfTransform);
        }
      
        if (m_animator      && m_animator.enabled) m_animator.SetBool(m_OffMeshLinkAnimationParameter, m_NavMeshAgentAndObstacleHost.IsOnOffMeshLink);
    }

    [Button]
    private void UpdateMirror()
    {
        if (!m_NavMeshAgentAndObstacleHost)
        {
            CreateMirror();
        }

        m_NavMeshAgentAndObstacleHost.Position = m_SelfTransform.position;
    }

    private void CreateMirror()
    {
        var navigationAgentsContainer = GameObject.FindGameObjectWithTag("NavigationAgents").transform;
        m_NavMeshAgentAndObstacleHost      = Instantiate(m_NavmeshAgentPrefab.gameObject, navigationAgentsContainer).GetComponent <NavMeshAgentAndObstacleHost>();
        m_NavMeshAgentAndObstacleHost.name = "Agent For " + transform.name;
    }

    private void OnDestroy()
    {
        if(m_NavMeshAgentAndObstacleHost) Destroy(m_NavMeshAgentAndObstacleHost.gameObject);
    }
}