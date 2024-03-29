using InventoryAndEquipment;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class EquipmentItemSlotUI : MonoBehaviour
    {
        [SerializeField] private Transform         m_IconSocket;
        private                  EquipmentItemIcon instantiatedIcon;
        public                   EquipmentItem     DisplayedItem { get; private set; }

        public void Display(EquipmentItem item, bool deleteOldIconOnlyWhenNewIconAppears = false)
        {
            if(item == DisplayedItem) return;
           
            if(item!=null || !deleteOldIconOnlyWhenNewIconAppears) Clear();
            
            DisplayedItem    = item;
            
            if (item != null)
            {
                instantiatedIcon = LoadIcon(item);
                instantiatedIcon.Display(item);
            }
        }

        public void Clear()
        {
            DisplayedItem = null;
            
            if (instantiatedIcon)
            {
                Destroy(instantiatedIcon.gameObject);
            }
        }

        private EquipmentItemIcon LoadIcon(EquipmentItem item)
        {
            var templatePrefab = EquipmentDatabase.GetIconTemplatePrefab(item.Gearslot);
            return Instantiate(templatePrefab.gameObject, m_IconSocket).GetComponent <EquipmentItemIcon>();
        }
    }
}