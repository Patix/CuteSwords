using System;
using Content.Scripts.Game.Navigation;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class NavMeshAgentController : MonoBehaviour
{
    [SerializeField]                  private NavMeshAgentAndObstacleHost m_NavmeshAgentPrefab;
    [SerializeField, HideInInspector] private NavMeshAgentAndObstacleHost m_NavMeshAgentAndObstacleHost;
    [SerializeField]                  private Rigidbody2D                 m_SelfTransform;
    [SerializeField]                  private Animator                    m_animator;
    [SerializeField]                  private string                      m_OffMeshLinkAnimationParameter;

    private RigidbodyType2D originalTypeForRigidbody;
    
    private void Awake()
    {
        originalTypeForRigidbody = m_SelfTransform.bodyType;
        UpdateMirror();
    }

    private void FixedUpdate()
    {
        m_NavMeshAgentAndObstacleHost.ControlRigidbody(m_SelfTransform, originalTypeForRigidbody);
        m_NavMeshAgentAndObstacleHost.Sync(m_SelfTransform);
        if (m_animator      && m_animator.enabled) m_animator.SetBool(m_OffMeshLinkAnimationParameter, m_NavMeshAgentAndObstacleHost.IsOnOffMeshLink);
    }

    public void SetDestination(Vector2 destination) => m_NavMeshAgentAndObstacleHost.SetDestination(destination);

    [Button]
    private void UpdateMirror()
    {
        if (!m_NavMeshAgentAndObstacleHost)
        {
            var navigationAgentsContainer = GameObject.FindGameObjectWithTag("NavigationAgents").transform;
            m_NavMeshAgentAndObstacleHost      = Instantiate(m_NavmeshAgentPrefab.gameObject, navigationAgentsContainer).GetComponent <NavMeshAgentAndObstacleHost>();
            m_NavMeshAgentAndObstacleHost.name = "Agent For " + transform.name;
        }

        m_NavMeshAgentAndObstacleHost.Position = m_SelfTransform.position;
    }

    private void OnDestroy()
    {
        if(m_NavMeshAgentAndObstacleHost) Destroy(m_NavMeshAgentAndObstacleHost.gameObject);
    }
}