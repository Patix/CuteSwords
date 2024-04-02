using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    [SerializeField] private Transform m_TargetTransform;
    [SerializeField] private Vector3   m_offset;
    private void LateUpdate()
    {
        transform.position = m_TargetTransform.transform.position +m_offset;
    }
}
