using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class Create3DColliderFrom2D : MonoBehaviour
{
    // Start is called before the first frame update

    private MeshCollider m_MeshCollider;

    [Button]
    void Create(Transform roots)
    {
        m_MeshCollider ??= GetComponent <MeshCollider>();

        foreach (Collider2D collider2D in roots.GetComponentsInChildren<Collider2D>())
        {
            Debug.Log(collider2D.name + "Encapsulated");
            m_MeshCollider.bounds.Encapsulate(collider2D.bounds);
        }
        
        Debug.Log(m_MeshCollider.sharedMesh!=null);
    }

    [Button]
    void AddHere(Transform roots)
    {
        foreach (Collider2D collider2D in roots.GetComponentsInChildren<Collider2D>())
        {
            var newGameObject=Instantiate(new GameObject(collider2D.name), collider2D.gameObject.transform.position, collider2D.gameObject.transform.rotation, transform);
            var collider = newGameObject.AddComponent <BoxCollider>();
            collider.size = collider2D.bounds.size;

        }
    }
}
