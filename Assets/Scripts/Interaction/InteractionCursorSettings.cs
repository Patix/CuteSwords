using System;
using UnityEngine;

namespace Interaction
{
    [Serializable]
    public class InteractionCursorSettings
    {
        public                   InteractionType InteractionType    => interactionType;
      
        [SerializeField] private InteractionType interactionType;
        [SerializeField] private Texture2D       m_Allowed_Texture;
        [SerializeField] private Texture2D       m_NotAllowed_Texture;

        public Texture2D GetCursorTexture(bool interactionAllowed)
        {
            if (interactionAllowed) return m_Allowed_Texture;
            return m_NotAllowed_Texture;
        }
    }
}