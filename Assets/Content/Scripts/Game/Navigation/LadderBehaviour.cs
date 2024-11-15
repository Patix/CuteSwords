using System;
using UnityEngine;

public class LadderBehaviour : MonoBehaviour
{
    
    public  float    m_Level;
   
    [SerializeField] private Collider m_TriggerReference;
    
    private void OnTriggerEnter(Collider other)
    {
       
        var RelativePosition = new Vector3(0, m_Level, -m_Level);
        
        var entranceDirection        = (other.attachedRigidbody.position - m_TriggerReference.bounds.center).normalized;
     
        bool isGoingUp = entranceDirection.z < 0;
        bool canGo     = m_Level > 0 ? isGoingUp : !isGoingUp;
      
        //Debug.Log($" { (canGo?"Can":"Can't")} Go {(isGoingUp?"Up":"Down")} : ");
      
        if(!canGo) return;
        
        var teleportDestination               = other.attachedRigidbody.position + RelativePosition;
        
        other.attachedRigidbody.Move(teleportDestination,other.attachedRigidbody.rotation);
       
    }
}
