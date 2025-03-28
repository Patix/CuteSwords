using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TerrainSampler : MonoBehaviour
{
    [SerializeField] private LayerMask m_MaskToSample;
    [SerializeField] private Tilemap[] m_TileRenderersToSample;
    [SerializeField] private Transform m_Character;
    [SerializeField] private Vector3   m_CharacterGroundOffset;
    
    [SerializeField] private Image     m_TileDataVisualizer;
    private                  Vector3   Position => m_Character.position+m_CharacterGroundOffset;
    
    private                  TileBase            currentTile;

    public event Action<Sprite> OnTileUpdated;
   
    // Update is called once per frame
    void Update()
    {
        Sample();
    }

    void Sample()
    {
        var playerPosition = Position;
        
        foreach (var tilemap in m_TileRenderersToSample.Where(gobject=>(gobject.gameObject.layer & m_MaskToSample.value)!=0))
        {
            playerPosition.y = tilemap.transform.position.y;
            
            var cellPosition = tilemap.WorldToCell(playerPosition);
            var tile         = tilemap.GetTile(cellPosition);
            
            if (tile != default)
            {
                if (currentTile != tile)
                {
                    currentTile = tile;
                    NotifyCurrentTileUpdated(tilemap.GetSprite(cellPosition));
                }
                break;
            }
        }
    }

    void NotifyCurrentTileUpdated(Sprite sprite)
    {
        m_TileDataVisualizer.sprite = sprite;
        OnTileUpdated?.Invoke(sprite);
    }
}
