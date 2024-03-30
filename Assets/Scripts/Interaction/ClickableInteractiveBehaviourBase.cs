using Data;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Interaction
{
    public abstract class ClickableInteractiveBehaviourBase:MonoBehaviour
    {
        protected abstract InteractionType InteractionType { get; }
        protected abstract bool            CanInteract     { get;}
        protected abstract void            Interact();

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
            if(!CursorManager.CursorIsBehindUI) Interact();
        }
    }
}