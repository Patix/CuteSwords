using System;
using Imported;
using Interaction;
using Patik.CodeArchitecture.Patterns;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    public class GameConfiguration : SingletonScriptableObject <GameConfiguration>
    {
       
        [SerializeField] private CursorIcons m_Cursors;
        
        public static            CursorIcons Cursors                                  => Instance.m_Cursors;
       
        [Serializable]
        public class CursorIcons : SerializedDictionary <InteractionType, InteractionCursorSettings>
        {
        }
    }
}