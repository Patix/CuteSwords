using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DefaultNamespace;
using EventManagement;
using Interaction;
using Patik.CodeArchitecture.Patterns;
using UnityEngine;
using UnityEngine.Events;

namespace InventoryAndEquipment
{
    public class Character : Singleton <Character>
    {
        private static readonly string[]                                               DefaultGear = { "Orange Hood", "Green Eyes Mask", "Cloth Orange", "Dagger One", "Torso Orange", "Set Neutral" };
        private                 Dictionary <EquipmentItem.GearslotType, EquipmentItem> equipedItems;
        public                  IEnumerable <EquipmentItem>                            Equipment => equipedItems.Values;
        public                  StateTypes                                             State     { get; set; }

        public Character()
        {
            equipedItems = new Dictionary <EquipmentItem.GearslotType, EquipmentItem>();
            EquipStartingGearWithoutNotifyingObservers();
            State = StateTypes.Idle;
        }
    
        public bool HasEquipped(EquipmentItem item) => equipedItems.Values.Contains(item);

        public void Equip(EquipmentItem newItem, bool notifyObservers = true)
        {
            if (equipedItems.TryGetValue(newItem.Gearslot, out var equipmentToUneEquip)) //if something is already eqquiped
            {
                var currentlyEquippedItem = equipedItems[newItem.Gearslot];
                Inventory.SwapItems(currentlyEquippedItem, newItem);
            }

            equipedItems[newItem.Gearslot] = newItem;
            if (notifyObservers)
                GameEvents.Equipment_Updated.Invoke();
        }

        private void EquipStartingGearWithoutNotifyingObservers()
        {
            foreach (var itemName in DefaultGear) Equip(EquipmentDatabase.GetItemByName(itemName), false);
        }

        public enum StateTypes
        {
            Idle,
            Moving,
            Interacting
        }
    }
}