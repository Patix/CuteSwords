using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class CameraCustomProjection : MonoBehaviour
{
    private Camera cam;
   

    private Matrix4x4 projectionMatrixOriginal;
    void OnEnable()
    {
        cam = GetComponent<Camera>();
        SetCustomProjectionMatrix();
    }

    void OnDisable()
    {
        if (cam != null) ResetToOriginalMatrix();
    }
  
    private void SetCustomProjectionMatrix()
    { 
        projectionMatrixOriginal = cam.projectionMatrix; // Save Original Projection To Restore Later
        Matrix4x4 projectionMatrix = cam.projectionMatrix;// Create a custom projection matrix
        
        // Add Z value to Y value in the projection matrix
        projectionMatrix.m11 = 1/cam.orthographicSize * CalculateAdditionalStretch (cam.transform.eulerAngles.x); // m11 is the Y scale, m13 is the Z to Y component
        // Assign the custom projection matrix to the camera
        cam.projectionMatrix = projectionMatrix;
    }
    
    void ResetToOriginalMatrix()
    {
        cam.projectionMatrix = projectionMatrixOriginal;
        cam.ResetProjectionMatrix();
    }
    
    private float CalculateAdditionalStretch(float rotation)
    {
        return 1 + MathF.Tan(rotation *0.5f*Mathf.Deg2Rad);
    }
}
