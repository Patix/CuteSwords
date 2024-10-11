using System.Collections.Generic;
using Patik.CodeArchitecture.Patterns;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileInfoDatabase : SingletonSerializedScriptableObject <TileInfoDatabase>
{
    [SerializeField, Searchable] private Dictionary <Sprite, int>       m_Elevations     = new();
    [SerializeField, Searchable] private Dictionary <Sprite, Vector3>   m_Rotations      = new();
    [SerializeField, Searchable] private Dictionary <Sprite, AudioClip> m_FootstepSounds = new();

    public static AudioClip GetFootAudio(Sprite terrainSprite) { return Instance.m_FootstepSounds[terrainSprite]; }


    [Button, BoxGroup("Buttons")]
    void AssignAudio(Sprite[] sprites , AudioClip audioClip)
    {
        foreach (Sprite sprite in sprites)
        {
            if (!m_FootstepSounds.TryAdd(sprite, audioClip))
            {
                m_FootstepSounds[sprite] = audioClip;
            }
        }
    }
}
    
    
   