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
        [SerializeField] private float       m_characterMovementSpeed =10;
        [SerializeField]                                                     private CursorIcons m_Cursors;
        
        public static float       CharacterMovementSpeed => Instance.m_characterMovementSpeed;
        public static CursorIcons Cursors                => Instance.m_Cursors;
        
        
        
        [Serializable] public class CursorIcons : SerializedDictionary <InteractionType, InteractionCursorSettings >{}
    }
}