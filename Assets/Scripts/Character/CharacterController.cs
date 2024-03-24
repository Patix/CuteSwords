using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float       MaxMovementSpeed=1;
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private Animator    animator;
    
    private CharacterControls input;

    public Vector2 MovementVector2;
    
    private void OnEnable()
    {
        input = new CharacterControls();
        input.Enable();
        RegisterEvents();

    }

    private void FixedUpdate()
    {
       
        rigidbody2D.position+=(MaxMovementSpeed*MovementVector2*Time.fixedDeltaTime);
    }

    private void RegisterEvents()
    {
        input.Character.Movement.performed += OnInputEvent;
        input.Character.Movement.canceled += OnInputCanceled;
        
    }

    private void OnInputCanceled(InputAction.CallbackContext obj)
    {
        MovementVector2 = obj.ReadValue <Vector2>();
        Debug.Log("Canceled");
    }

    private void OnInputEvent(InputAction.CallbackContext inputContext)
    {
        MovementVector2 = inputContext.ReadValue <Vector2>();
        Debug.Log("Read");
    }
}
