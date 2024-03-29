using System.Collections.Generic;
using System.Linq;
using InventoryAndEquipment;
using Patik.CodeArchitecture.Patterns;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;

public class EquipmentDatabase : SingletonScriptableObject <EquipmentDatabase>
{
    [SerializeField, TableList] public List <EquipmentItem> EquipableItems;
    [SerializeField, AssetSelector]            public List <EquipmentItemIcon>    EquipmentItemUIIconRendererPrefabs;

    public static EquipmentItem     GetItemByName(string                             name)         => Instance.EquipableItems.First(x => x.Name                         == name);
    public static EquipmentItemIcon GetIconTemplatePrefab(EquipmentItem.GearslotType gearslotType) => Instance.EquipmentItemUIIconRendererPrefabs.First(x => x.SlotType == gearslotType);

    public static IEnumerable <EquipmentItem> AllItemsOfType(EquipmentItem.GearslotType slotType) => Instance.EquipableItems.Where(x => x.Gearslot == slotType);
}

