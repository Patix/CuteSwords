using Sirenix.OdinInspector;
using UnityEngine;

public class Create3DColliderFromSpriteRenderer : MonoBehaviour
{
    [Button]
    void Create()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size   = spriteRenderer.bounds.size;
        boxCollider.center = spriteRenderer.bounds.center;
        gameObject.name    = "Decoration " + spriteRenderer.sprite.name;
    }
}
