using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFollower : MonoBehaviour
{
    [SerializeField] private Transform m_CharacterTransform;
    [SerializeField] private Vector3   m_offset;
    private void LateUpdate()
    {
        transform.position = m_CharacterTransform.transform.position+m_offset;
    }
}
