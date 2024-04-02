using System;
using InventoryAndEquipment;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ShopItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text            m_DescriptionText;
        [SerializeField] private TMP_Text            m_PriceText;
        [SerializeField] private EquipmentItemSlotUI m_EquipmentSlot;
  
    
        public EquipmentItem DisplayedItem => m_EquipmentSlot.DisplayedItem;

        public event Action <EquipmentItem> OnClicked;
    
        public void Display(EquipmentItem item)
        {
            m_EquipmentSlot.Display(item);

            m_PriceText.text       = item.Price.ToString();
            m_DescriptionText.text = item.Name;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Clear()
        {
            m_EquipmentSlot.Clear(); // Clear to avoid duplication of items
        }
    
        public void OnClick() { OnClicked?.Invoke(DisplayedItem); }
    }
}