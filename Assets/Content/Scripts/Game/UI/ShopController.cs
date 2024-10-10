using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using EventManagement;
using InventoryAndEquipment;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class ShopController : MonoBehaviour
    {
        public static bool WindowIsActive { get; private set; }
      
        [SerializeField] private Vector3    m_gearTypeSwitcherButtonClickedSize;
        [SerializeField] private ShopItem   m_ShopItemPrefab;
        [SerializeField] private Transform  m_ShopItemGrid;
        [SerializeField] private TMP_Text   m_PlayerGoldText;
        [SerializeField] private Animator   m_GoldAnimator;
        [SerializeField] private Button[]   m_GearslotTypeSwitchButtons;
   
        private InteractionMode            intentionTypeChosen;
        private EquipmentItem.GearslotType buyPanelChosenType;
        private List <ShopItem>            instantiatedShopItems;

        private GameEventListeners eventListeners;
    
        private void Awake()
        {
            instantiatedShopItems = new List <ShopItem>();
            ItemBuyTypeSwitchClicked("Hood");
        }

        private void OnEnable()
        {
            eventListeners ??= new GameEventListeners
            (
                (GameEvents.Inventory_Update_Gold, RefreshGoldText),
                (GameEvents.Inventory_Update_Items, RefreshItems)
            );
            eventListeners.SubscribeAll();

            RefreshGoldText();
            RefreshItems();
            WindowIsActive = true;
        }

        private void OnDisable()
        {
            eventListeners.UnsubscribeAll();
            WindowIsActive = false;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }
        

        private void RefreshItems()
        {
            foreach (var item in instantiatedShopItems)
            {
                item.Hide();
            
                if(item.DisplayedItem ==null) continue; 
            
                if (intentionTypeChosen == InteractionMode.Buy)
                {
                    if (item.DisplayedItem.IsAvailableToBuy() && item.DisplayedItem.Gearslot == buyPanelChosenType)
                        item.Show();
                    else item.Hide();
                }

                else // IntentionType.Sell
                {
                    if(item.DisplayedItem.IsAvailableToSell())
                        item.Show();
                    else item.Hide();
                }
            }
        }
    
        private void RefreshGoldText()
        {
            m_PlayerGoldText.text = Inventory.Gold.ToString();
        }

        private void ActivateBuyType(EquipmentItem.GearslotType gearslotType)
        {
            buyPanelChosenType = gearslotType;
            LoadPanelAndPopulateItems(InteractionMode.Buy, ItemsAvailableToPurchase(gearslotType));
        }

        private void ActivateSellPanel()
        {
            LoadPanelAndPopulateItems(InteractionMode.Sell, Inventory.ItemsInBackPack);
        }

        private void LoadPanelAndPopulateItems(InteractionMode intention, IEnumerable <EquipmentItem> items)
        {
            intentionTypeChosen = intention;
            PopulateShopItems(items);
            RefreshItems();
        }
    
        private void PopulateShopItems(IEnumerable <EquipmentItem> items)
        {
            var itemCount = items.Count();
        
            while (instantiatedShopItems.Count < itemCount) CreateShopItem();
        
            var index = 0;
        
            foreach (var shopItem in instantiatedShopItems)
            {
                shopItem.Clear();
                shopItem.Hide();
            }
            foreach (var equipmentItem in items)
            {
                instantiatedShopItems[index++].Display(equipmentItem);
            }
        
            RefreshItems();
        }

        private void CreateShopItem()
        {
            var newShopItem = Instantiate(m_ShopItemPrefab, m_ShopItemGrid).GetComponent <ShopItem>();
            instantiatedShopItems.Add(newShopItem);
            newShopItem.OnClicked += OnShopItemClicked;
        }

 
        public void OnShopItemClicked(EquipmentItem item)
        {
            if (intentionTypeChosen == InteractionMode.Buy)
            {
                if (Inventory.Gold >= item.Price)
                {
                    if (Inventory.TryAddItem(item))
                    {
                        GameEvents.Shop_Item_Purchased.Invoke();
                        Inventory.SubtractGold(item.Price);
                    }
                
                    else
                    {
                        GameEvents.Shop_FailedToBuy_NotEnoughSlotsInInventory.Invoke();
                    }
                }

                else
                {
                    GameEvents.Shop_FailedToBuy_NotEnoughGold.Invoke();
                }
           
            }

            else //sell
            {
                Sell(item);
            }
        }

        public static void Sell(EquipmentItem item)
        {
            Inventory.RemoveItem(item);
            Inventory.AddGold(item.Price);
            GameEvents.Shop_Item_Sold.Invoke();
        }

        public void ItemBuyTypeSwitchClicked(string name)
        {
            //Scale Clicked Button
            foreach (var switchButton in m_GearslotTypeSwitchButtons)
            {
                if (switchButton.name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                    switchButton.transform.localScale = m_gearTypeSwitcherButtonClickedSize;
                else
                {
                    switchButton.transform.localScale = Vector3.one;
                }
            }

            //Activate Type
            var gearslotType = name switch { 
                "Face"      => EquipmentItem.GearslotType.Face,
                "Hood"      => EquipmentItem.GearslotType.Hood,
                "Pelvis"    => EquipmentItem.GearslotType.Pelvis, 
                "Torso"     => EquipmentItem.GearslotType.Torso, 
                "Shoulders" => EquipmentItem.GearslotType.ShouldersAndBase, 
                "Weapon"    => EquipmentItem.GearslotType.Weapon, 
            };

            ActivateBuyType(gearslotType);
        }

        private void UpdateGearTypePanelVisibility()
        {
            m_GearslotTypeSwitchButtons.First().transform.parent.gameObject.SetActive(intentionTypeChosen == InteractionMode.Buy);
        }
    
        public void ActivateBuyPanelClicked()
        {
            ActivateBuyType(buyPanelChosenType);
            UpdateGearTypePanelVisibility();
        }

        public void ActivateSellPanelClicked()
        {
            ActivateSellPanel();
            UpdateGearTypePanelVisibility();
        }

        private IEnumerable <EquipmentItem> ItemsAvailableToPurchase(EquipmentItem.GearslotType gearSlotType) { return EquipmentDatabase.AllItemsOfType(gearSlotType).Where(x => x.IsAvailableToBuy()); }
    
        enum InteractionMode
        {
            Buy,
            Sell
        }
    }
}