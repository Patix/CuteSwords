using System;
using UnityEngine;

public class LadderBehaviour : MonoBehaviour
{
    private int         _enteredObjects;
   
    private void OnTriggerEnter(Collider other)
    {
        other.SendMessage("ReceiveMessage","Ladder_On",SendMessageOptions.DontRequireReceiver);
        CancelOutGravityForce(other);
        _enteredObjects++;
    }
   
    private void OnTriggerExit(Collider other)
    {
        other.SendMessage("ReceiveMessage","Ladder_Off",SendMessageOptions.DontRequireReceiver);
        PreventJumpWhenFinishedClimbing(other);
        _enteredObjects--;
    }

    private void OnTriggerStay(Collider other)
    {
        CancelOutGravityForce(other);
    }

    private void CancelOutGravityForce(Collider other)
    {
        if(!other) return;
        
        var otherRigidbody = other.attachedRigidbody;
        
        if (otherRigidbody && otherRigidbody.useGravity && !otherRigidbody.isKinematic)
        {
            otherRigidbody.AddForce(-Physics.gravity  * otherRigidbody.mass, ForceMode.Force); // Cancel Out Gravity
        }
    }

    private void PreventJumpWhenFinishedClimbing(Collider other)
    {
        var velocity = other.attachedRigidbody.linearVelocity;
        velocity.y                             = 0;
        other.attachedRigidbody.linearVelocity = velocity;
    }

    private void OnDrawGizmosSelected()
    {
        if (_enteredObjects > 0)
        {
            var collider = GetComponent <BoxCollider>();
            Gizmos.DrawWireCube(transform.TransformPoint(collider.center),Vector3.Scale(transform.lossyScale,collider.size));
        }
    }
}

