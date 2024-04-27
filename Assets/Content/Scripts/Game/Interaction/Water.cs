using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class Water: MonoBehaviour
    {
        [SerializeField] private float WaterHeight;



        float CalculateSinkBound()
        {
            var bounds = GetComponent <SpriteRenderer>().bounds;
            Debug.DrawLine(bounds.min, bounds.max,Color.red, 2);

            return bounds.min.y + WaterHeight;
        }
        
        
        [Button]
        void Sink()
        {

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            GetComponent <SpriteRenderer>().GetPropertyBlock(propertyBlock);
                
            propertyBlock.SetFloat("_ClipUnderY", CalculateSinkBound());
            propertyBlock.SetFloat("_Rotation", -transform.eulerAngles.z);
            GetComponent <SpriteRenderer>().SetPropertyBlock(propertyBlock);
            
        }
    }
}