using System.Collections.Generic;
using InventoryAndEquipment;
using Sirenix.OdinInspector;
using UnityEngine;

public class EquipmentDatabase : ScriptableObject
{
    [SerializeField, TableList] public List <EquipmentItem> EquipableItems;
}

