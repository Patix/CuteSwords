using UI;
using UnityEngine;

namespace Interaction.Concrete
{
    public class ShopUnit : ClickableInteractiveBehaviourBase
    {
        [SerializeField] private ShopController shopController;

        protected override InteractionType InteractionType => InteractionType.Open;
        protected override bool            CanInteract     => ChecklCanInteract();
        protected override void            Interact()      { shopController.Show(); }


        bool ChecklCanInteract()
        {
            return true;
            return Vector2.Distance(gameObject.transform.position, CharacterController.Instance.transform.position) < 10; //Or Something Else
        }
    }
}