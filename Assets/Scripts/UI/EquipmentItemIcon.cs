using System;
using System.Collections.Generic;
using InventoryAndEquipment;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EquipmentItemIcon:MonoBehaviour
    {
        [SerializeField] private Image[]                    imageRendererReferences;
        [SerializeField] public  EquipmentItem.GearslotType SlotType;
        
        public IEnumerable <Sprite> ExtractUISprites(EquipmentItem item) => item.UIIcons;

        public void Display(EquipmentItem item)
        {
            var extractedImages = ExtractUISprites(item);
            var index           = 0;
            try
            {
                foreach (var extractedImage in extractedImages)
                {
                    imageRendererReferences[index].sprite = extractedImage;
                    index++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
        }
    }
}