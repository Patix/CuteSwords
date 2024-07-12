using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.UI;
using UnityEngine;
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
            var cellPosition = tilemap.WorldToCell(playerPosition);
            var tile         = tilemap.GetTile(cellPosition);
            
            if (tile != default)
            {
                if (currentTile != tile)
                {
                    currentTile = tile;
                    TileData tileData = default;
                    
                    currentTile.GetTileData(cellPosition, tilemap, ref tileData);
                    NotifyCurrentTileUpdated(tileData);
                }
            }
        }
    }

    void NotifyCurrentTileUpdated(TileData data)
    {
        m_TileDataVisualizer.sprite = data.sprite;
    }
}
