using System;
using Data;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Interaction
{
    public abstract class ClickableInteractiveBehaviourBase:MonoBehaviour
    {
        public virtual     InteractionType InteractionType { get; }
        public virtual     bool            CanInteract     { get; set; }
        protected abstract void            Interact();

        private void OnDisable()
        {
            OnMouseExit();
        }

        private void OnMouseOver()
        {
            CursorManager.TrySetCursorTexture(GameConfiguration.Cursors[InteractionType].GetCursorTexture(CanInteract));
        }

        private void OnMouseExit()
        {
            CursorManager.TrySetCursorTexture(GameConfiguration.Cursors[InteractionType.None].GetCursorTexture(true));
        }

        private void OnMouseDown()
        {
            if(!CursorManager.CursorIsBehindUI && CanInteract) Interact();
        }
    }
}