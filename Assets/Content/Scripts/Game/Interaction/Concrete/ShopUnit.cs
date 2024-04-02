using UI;
using UnityEngine;

namespace Interaction.Concrete
{
    public class ShopUnit : ClickableInteractiveBehaviourBase
    {
        [SerializeField] private ShopController shopController;

        public override InteractionType InteractionType => InteractionType.Open;
        
        protected override void Interact()  { shopController.Show(); }
     
    }
}