using System.Collections.Generic;
using System.Linq;
using InventoryAndEquipment;
using Patik.CodeArchitecture.Patterns;
using Sirenix.OdinInspector;
using UnityEngine;

public class EquipmentDatabase : SingletonScriptableObject <EquipmentDatabase>
{
    [SerializeField, TableList] public List <EquipmentItem> EquipableItems;
    [SerializeField]            public List <GameObject>    EquipmentItemUIIconRendererPrefabs;

    public static EquipmentItem               GetItemByName(string name) => Instance.EquipableItems.First(x => x.Name == name);
    public static IEnumerable <EquipmentItem> EquipmentItems             => Instance.EquipableItems;
}

