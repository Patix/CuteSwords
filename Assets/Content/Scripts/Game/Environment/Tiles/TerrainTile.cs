using System;
using Unity.Mathematics;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Game.Environment.Tiles
{
    
    public class TerrainTile : Tile , ITerrainTile
    {
        [field: SerializeField] public TerrainTileData TerrainTileData { get;       private set; }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);
            OnAfterTileRefreshed(position,tilemap);
        }

        public void OnAfterTileRefreshed(Vector3Int position, ITilemap tilemap)
        {
            var      tile     = tilemap.GetTile(position);
            if (tile is null || tile!=this) return;
            tilemap.GetComponent<Tilemap>().ApplyAdditionalTerrainTileSettings(position);
        }
    }
}

//[CustomEditor(typeof(TerrainRuleTile))] public class TerrainRuleEditor : OdinEditor { }