using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Game.Environment.Tiles
{
    
    public class TerrainTile : Tile , ITerrainTile
    {
        [field: SerializeField] public TerrainTileData TerrainTileData  { get; private set; }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            this.ApplyAdditionalTerrainTileSettings(TerrainTileData.m_IsBillboard, position,tilemap, ref tileData);
        }
        
    }
}

//[CustomEditor(typeof(TerrainRuleTile))] public class TerrainRuleEditor : OdinEditor { }