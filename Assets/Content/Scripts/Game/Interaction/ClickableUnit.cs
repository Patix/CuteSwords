using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using BezierUtility = UnityEditor.U2D.Path.BezierUtility;

namespace Interaction
{
    public class ClickableUnit : ClickableInteractiveBehaviourBase
    {
        [SerializeField] private InteractionType m_InteractionType;
        [SerializeField] private UnityEvent      OnInteracted;
        public override          InteractionType InteractionType => m_InteractionType;
     
        protected override void Interact()
        {
            OnInteracted.Invoke();
        }
        
    }
}