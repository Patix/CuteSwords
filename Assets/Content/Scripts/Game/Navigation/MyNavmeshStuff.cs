using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MyNavmeshStuff : MonoBehaviour
{
    public List <NavMeshAgentController> AllNavmeshAgents;
    public NavMeshAgentController        m_SelectedNavmeshAgent;

   
    
    public void SetDestination (Vector2 destionation)
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            for (var i = 0; i < AllNavmeshAgents.Count; i++)
            {
                AllNavmeshAgents[i].SetDestination(destionation);
            }
        }

        else
        {
            m_SelectedNavmeshAgent.SetDestination(destionation);
        }
    }
}
