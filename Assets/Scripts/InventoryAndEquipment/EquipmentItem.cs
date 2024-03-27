using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace InventoryAndEquipment
{
    [Serializable]
    public partial class EquipmentItem
    {
        [SerializeField] private string                   name;
        [SerializeField] private List <EquipmentItemData> items;
        [SerializeField] private int                      price;
       
        public                   GearslotType             Gearslot => GetGearSlot();
        public                   string                   Name     => name;
        public                   int                      Price    => price;

        public IEnumerable <Sprite> UIIcons =>items.SelectMany(x => x.GetUIIcons());
        
        
        public void Equip(Transform rootTransform)
        {
            foreach (var equipmentItemData in items)
            {
                equipmentItemData.Attach(rootTransform);
            }
        }

        private GearslotType GetGearSlot()
        {
            if (items == null || items.Count == 0) return GearslotType.Undefined;
            if (items.Count == 1)
            {
                var equipmentPart = items[0];

                if (equipmentPart == null) return GearslotType.Undefined;

                switch (equipmentPart.BodyPart)
                {
                    case EquipmentItemData.BodyPartType.HOOD:   return GearslotType.HOOD;
                    case EquipmentItemData.BodyPartType.FACE:   return GearslotType.FACE;
                    case EquipmentItemData.BodyPartType.TORSO:  return GearslotType.TORSO;
                    case EquipmentItemData.BodyPartType.PELVIS: return GearslotType.Lower;
                    case EquipmentItemData.BodyPartType.WEAPON: return GearslotType.Weapon;
                }
            }

            if (items.Count > 0) return GearslotType.Base;

            return GearslotType.Undefined;
        }
    }
}