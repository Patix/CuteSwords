using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class UnhideGameobjects : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button] void UnhideAll()
    {
        foreach (Transform tr in transform.GetComponentsInChildren<Transform>(true))
        {
            tr.gameObject.hideFlags = HideFlags.None;
        }
    }
    private void OnTransformChildrenChanged()
    {
      UnhideAll();
    }
}
