﻿using System;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InventoryAndEquipment
{
    [Serializable]
    public partial class EquipmentItemData : ScriptableObject
    {
        [SerializeField] private BodyPartType bodyPart;
        [SerializeField] private Sprite       sprite;
        [SerializeField] private Sprite       spriteRightPair;
        
        public BodyPartType BodyPart => bodyPart;

        public void Equip(Transform root)
        {
            var spriteRenderers = root.GetComponentsInChildren <SpriteRenderer>();
            var targetSlotName  = GetTargetSlotName();

            if (bodyPart == BodyPartType.WEAPON)
            {
                var bothWeaponRenderers = spriteRenderers.Where(spriteRender => spriteRender.name == targetSlotName);

                foreach (var weaponRenderer in bothWeaponRenderers)
                {
                    weaponRenderer.sprite = sprite;
                }
            }

            else
            {
                spriteRenderers.First(x => x.name == targetSlotName).sprite = sprite;

                if (HasPair())
                {
                    var rightPairTargetSlotName = GetRightPairSlotName(targetSlotName);
                    spriteRenderers.First(x => x.name == rightPairTargetSlotName).sprite = spriteRightPair;
                }
            }
        }

        string GetTargetSlotName()
        {
            var bodyPartToLowerString = bodyPart.ToString().ToLower();

            if (HasPair())
                return $"Rogue_{bodyPartToLowerString}_l_01";
            return $"Rogue_{bodyPartToLowerString}_01";
        }

        private bool HasPair()
        {
            switch (bodyPart)
            {
                case BodyPartType.LEG:
                case BodyPartType.ELBOW:
                case BodyPartType.BOOT:
                case BodyPartType.SHOULDER:
                case BodyPartType.WRIST: return true;
                default: return false;
            }
        }

        public string GetRightPairSlotName(string slotName) { return slotName.Replace("_l_", "_r_"); }
    }
}