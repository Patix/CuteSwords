using System;
using System.Collections.Generic;
using System.Linq;
using Patik.CodeArchitecture.Patterns;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileInfoDatabase : SingletonSerializedScriptableObject <TileInfoDatabase>
{
    [SerializeField, Searchable] private Dictionary <Sprite, int>        m_Elevations     = new();
    [SerializeField, Searchable] private Dictionary <Sprite, Vector3>    m_Rotations      = new();
    [SerializeField, Searchable] private Dictionary <Sprite, AudioClip>  m_FootstepSounds = new();
    [SerializeField, Searchable] private Dictionary <Sprite, GameObject> m_SurfacePrefabs = new();

    public static AudioClip GetFootAudio(Sprite    terrainSprite)                     { return Instance.m_FootstepSounds[terrainSprite]; }
    public static bool      TryGetRotation(Sprite  terrainSprite, out Vector3 result) { return Instance.m_Rotations.TryGetValue(terrainSprite, out result);}
    public static bool      TryGetElevation(Sprite terrainSprite, out int elevation) { return Instance.m_Elevations.TryGetValue(terrainSprite, out elevation);}

    public static GameObject GetSurfacePrefab(Sprite sprite)
    {
        return Instance.m_SurfacePrefabs[sprite];
    }
    
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

    [Button]
    void AddAllSprites(Sprite[] sprites , GameObject []prefabs)
    {
        foreach (Sprite sprite in sprites)
        {
            var correctPrefab = prefabs.FirstOrDefault(x => sprite.name.Contains(x.name.Split("_").Last(), StringComparison.InvariantCultureIgnoreCase));

            m_SurfacePrefabs.TryAdd(sprite, correctPrefab);
        }
     
    }
}
    
    
   