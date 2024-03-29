using System;
using System.Linq;
using InventoryAndEquipment;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EquipmentWindowUIController : MonoBehaviour
    {
        [SerializeField] private Transform m_Character;
        [SerializeField] private Image[]   Images;
        [SerializeField] private Image     prefab;

        [SerializeField] private EquipmentItemIcon Hood, Head, Weapon, Set, Torso, Pelvis;



        void OnEnable()
        {
            DisplayAll();
        }
        [Button]
        void DisplayAll()
        {
            
        }
    
    
        [ContextMenu("Fill")]
        void Fill()
        {
            var allSprites = m_Character.GetComponentsInChildren <SpriteRenderer>().Select(x => x.sprite).Distinct().ToList();
            for (var i = 0; i < allSprites.Count; i++)
            {
                Images[i].sprite = allSprites[i];
            }
        }

   

        private void DoOnPrefab()
        {
            var allrenderers = m_Character.GetComponentsInChildren <SpriteRenderer>();

            Array.Sort(allrenderers, (spriteRenderer, renderer1) => spriteRenderer.sortingOrder.CompareTo(renderer1.sortingOrder));

            for (var i = 0; i < allrenderers.Length; i++)
            {
                var clone = Instantiate(prefab, prefab.transform.parent, true);
                clone.name   = allrenderers[i].name;
                clone.sprite = allrenderers[i].sprite;
            }

            prefab.enabled = false;
        }
    }
}