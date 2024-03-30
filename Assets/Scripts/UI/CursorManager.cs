using System;
using Data;
using Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace UI
{
    public class CursorManager : MonoBehaviour
    {
        private static EventSystem eventSystem;
        private static Texture2D currentCursor;
        private static Texture2D defaultCursor;
        public static  bool      CursorIsBehindUI => eventSystem && eventSystem.IsPointerOverGameObject();

        public static void TrySetCursorTexture(Texture2D texture2D)
        {
            if (!CursorIsBehindUI) SetCursorTexture(texture2D);
        }

        private static void SetCursorTexture(Texture2D texture2D)
        {
            currentCursor = texture2D;
            Cursor.SetCursor(texture2D, Vector2.zero, CursorMode.Auto);
        }

        private void OnEnable()
        {
            defaultCursor = GameConfiguration.Cursors[InteractionType.None].GetCursorTexture(true);
            SetCursorTexture(defaultCursor);
            
        }

        private void Update()
        {
            if (!eventSystem) eventSystem = EventSystem.current;
          
            if (CursorIsBehindUI && currentCursor != defaultCursor)
                SetCursorTexture(defaultCursor);
        }
    }
}