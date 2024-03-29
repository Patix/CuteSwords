using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using EventManagement;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace InventoryAndEquipment
{
    public static class Inventory
    {
        private const  int                         TotalSlots       = 6;
        private static int                         _Gold            = 1000;
        private static EquipmentItem []        _ItemsInBackPack = new EquipmentItem[TotalSlots];
        public static  IEnumerable <EquipmentItem> ItemsInBackPack => _ItemsInBackPack.Where(x=>x!=null);
        
        
        public static  int Gold => _Gold;

        public static void AddGold(int amount)
        {
            _Gold += amount;
            GameEvents.Inventory_Update_Gold.Invoke();
        }

        public static void SubtractGold(int amount)
        {
            _Gold = Mathf.Max(_Gold - amount, 0); // Clamp To Zero
            GameEvents.Inventory_Update_Gold.Invoke();
        } 

        public static int AvailableFreeSlots => TotalSlots - _ItemsInBackPack.Count(x=>x!=null);

        public static bool TryAddItem(EquipmentItem item)
        {
            if (AvailableFreeSlots > 0)
            {
                var indexOfFirstAvailableSlot = Array.IndexOf(_ItemsInBackPack,null);
                _ItemsInBackPack[indexOfFirstAvailableSlot] = item;
                
                GameEvents.Inventory_Update_Items.Invoke();
                return true;
            }

            return false;
        }

        public static EquipmentItem ItemAtIndex(int index)     => _ItemsInBackPack[index];

        public static void RemoveItem(int slotIndex)
        {
            _ItemsInBackPack[slotIndex] = null;
            GameEvents.Inventory_Update_Items.Invoke();
        }
        
        public static void RemoveItem(EquipmentItem item)      => RemoveItem(Array.IndexOf(_ItemsInBackPack, item));

        public static EquipmentItem SwapItemAtIndex(EquipmentItem itemToPutInInventory, int index)
        {
            var itemToReturnFromInventory = ItemAtIndex(index);
            _ItemsInBackPack[index] = itemToPutInInventory;
            GameEvents.Inventory_Update_Items.Invoke();
            return itemToReturnFromInventory;
        }

        public static bool Contains(EquipmentItem item)
        {
            
            return item !=null && _ItemsInBackPack.Contains(item);
        }
    }
}