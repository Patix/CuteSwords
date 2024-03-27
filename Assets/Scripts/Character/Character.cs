using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Patik.CodeArchitecture.Patterns;
using UnityEngine;

namespace InventoryAndEquipment
{
    public class Character
    {
        private static readonly string[] DefaultGear = { "Orange Hood", "Green Eyes Mask", "Cloth Orange", "Dagger One", "Torso Orange", "Set Neutral" };
        private Dictionary <EquipmentItem.GearslotType, EquipmentItem> equipedItems;

        public StateTypes State { get; set; }
        public Character()
        {
            equipedItems = new Dictionary <EquipmentItem.GearslotType, EquipmentItem>();
            Equip(DefaultGear);
            State = StateTypes.Idle;
        }


        public bool HasEquipped(EquipmentItem item)     => equipedItems.Values.Contains(item);
        public bool HasEquipped(string        itemName) => HasEquipped(EquipmentDatabase.GetItemByName(itemName));
        public void Equip(string        itemName) => Equip(EquipmentDatabase.GetItemByName(itemName));

        public void Equip(EquipmentItem item)
        {
            equipedItems[item.Gearslot] = item;
        }
        
        public void Equip(params string[] items)
        {
            foreach (var item in items) Equip(item);
        }
        
        
        public enum StateTypes
        {
            Idle,
            Moving,
            Attacking
        }
    }
}