namespace InventoryAndEquipment
{
    public static class ShopExtensionsForItem
    {
        public static bool IsAvailableToBuy(this EquipmentItem item) => !IsOwned(item);

        public static bool IsAvailableToSell(this EquipmentItem item) => Inventory.Contains(item);
        public static bool IsOwned(this           EquipmentItem item) =>  Inventory.Contains(item) || Character.Instance.HasEquipped(item);
    }
}