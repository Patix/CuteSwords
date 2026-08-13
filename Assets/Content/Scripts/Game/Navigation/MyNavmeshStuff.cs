using System.Collections.Generic;
using UnityEngine;


public class MyNavmeshStuff : MonoBehaviour
{
    public List <NavMeshAgentController> AllNavmeshAgents;
    public NavMeshAgentController        m_SelectedNavmeshAgent;

    public void OnLeftClick(RaycastHit hit)
    {
        NavMeshAgentController navmeshAgent = null;
        
        if (hit.rigidbody)
        {
            navmeshAgent = hit.rigidbody.GetComponent <NavMeshAgentController>();
        }
        
        if (!Input.GetKey(KeyCode.LeftControl))
        {
            if(navmeshAgent) SelectNavmeshAgent(navmeshAgent);
        }
            
        else
        {
            if (navmeshAgent)
            {
                if (AllNavmeshAgents.Contains(navmeshAgent))
                {
                    AllNavmeshAgents.Remove(navmeshAgent);
                }
                else
                {
                    AllNavmeshAgents.Add(navmeshAgent);
                }
            }

            else
            {
                AllNavmeshAgents.Clear();
            }
        }
    }
    
    public void SelectNavmeshAgent(NavMeshAgentController navmeshAgent)
    {
        m_SelectedNavmeshAgent = navmeshAgent;
    }
    
    public void SetDestination (RaycastHit hit)
    {
        var destination = hit.point;
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            for (var i = 0; i < AllNavmeshAgents.Count; i++)
            {
                AllNavmeshAgents[i].SetDestination(destination);
            }
        }

        else
        {
            m_SelectedNavmeshAgent.SetDestination(destination);
        }
    }
}
