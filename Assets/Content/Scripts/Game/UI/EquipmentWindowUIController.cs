using System;
using System.Linq;
using EventManagement;
using InventoryAndEquipment;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EquipmentWindowUIController : MonoBehaviour
    {
        [SerializeField] private EquipmentItemIcon[] equipmentItemIcons;

        private EventListeners eventListeners;


        void OnEnable()
        {
            eventListeners ??= new((GameEvents.Equipment_Updated, UpdateView));
            eventListeners.SubscribeAll();
            UpdateView();
        }

        void OnDisable()
        {
            eventListeners.UnsubscribeAll();
        }
        
        void UpdateView()
        {
            foreach (var equipmentItemIcon in equipmentItemIcons)
            {
                equipmentItemIcon.Display(Character.Instance.Equipment.First(x=> x.Gearslot==equipmentItemIcon.SlotType));
            }
        }
    }
}