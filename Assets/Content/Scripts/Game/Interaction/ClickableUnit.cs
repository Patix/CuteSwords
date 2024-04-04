using UnityEngine;
using UnityEngine.Events;

namespace Interaction
{
    public class ClickableUnit : ClickableInteractiveBehaviourBase
    {
        [SerializeField] private bool            m_IsAllowedToInteract=true;
        [SerializeField] private float           m_InteractionRadius =2;
        [SerializeField] private InteractionType m_InteractionType;
        [SerializeField] private UnityEvent      OnInteracted;

      

        public override bool CanInteract
        {
            get
            {
                 return m_IsAllowedToInteract &&
                 Vector2.Distance(CharacterController.Instance.transform.position, transform.position) <= m_InteractionRadius;

            }
            set => m_IsAllowedToInteract = value;
        }

        public override InteractionType InteractionType => m_InteractionType;
     
        protected override void Interact()
        {
            OnInteracted.Invoke();
        }
        
    }
}