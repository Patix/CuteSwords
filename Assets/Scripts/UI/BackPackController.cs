using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using EventManagement;
using InventoryAndEquipment;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    [Serializable]
    public class BackPackController : MonoBehaviour
    {
        private List <EquipmentItem> PreviusItems;
        
        public  List <EquipmentItemSlotUI> Slots;

        private List <Animator> animators;
        
        private void Awake()
        {
            animators = Slots.Select(x => x.GetComponent <Animator>()).ToList();
        }

        private void UpdateUI()
        {
            for (var i = 0; i < Slots.Count; i++)
            {
                var oldItem = Slots[i].DisplayedItem;
                var newItem     = Inventory.ItemAtIndex(i);
                AnimateItemUpdate(oldItem, newItem,i);
                
                Slots[i].Display(newItem, deleteOldIconOnlyWhenNewIconAppears:true);
            }
        }
     
        public void OnItemClicked(EquipmentItemSlotUI button)
        {
            if (button.DisplayedItem == null) return;

            if (ShopController.WindowIsActive)
            {
                ShopController.Sell(button.DisplayedItem);
            }

            else
            {
                Character.Instance.Equip(button.DisplayedItem);
            }
        }
        
        void OnEnable()
        {
            GameEvents.Inventory_Update_Items.Subscribe(UpdateUI);
            UpdateUI();
        }
        void OnDisable()
        {
            GameEvents.Inventory_Update_Items.Unsubscribe(UpdateUI);
        }

        private void AnimateItemUpdate(EquipmentItem oldItem, EquipmentItem newItem , int slotIndex)
        {
            if(oldItem == newItem) return;
            
            var animator = animators[slotIndex];

            if (oldItem      == null && newItem != null) animator.SetTrigger("Add");
            else if (oldItem != null && newItem == null) animator.SetTrigger("Remove");
            
            //Do Nothing on Swap
        }
    }
}