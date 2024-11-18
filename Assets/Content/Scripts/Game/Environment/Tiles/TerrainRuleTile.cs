using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Environment.Tiles
{
    
    public class TerrainRuleTile : RuleTile, ITerrainTile
    {
        [SerializeField, AssetSelector] private List <TileBase> m_FriendTiles;
      
        [field:SerializeField] public TerrainTileData TerrainTileData { get; private set; }
        
        /// <summary>
        /// Retrieves any tile rendering data from the scripted tile.
        /// </summary>
        /// <param name="position">Position of the Tile on the Tilemap.</param>
        /// <param name="tilemap">The Tilemap the tile is present on.</param>
        /// <param name="tileData">Data to render the tile.</param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
           base.GetTileData(position,tilemap,ref tileData);
           tileData.flags ^= TileFlags.LockTransform;
        }

        public override bool RuleMatch(int neighborRule, TileBase other)
        {
            return neighborRule switch
            {
                TilingRuleOutput.Neighbor.This    => base.RuleMatch(neighborRule, other) || m_FriendTiles.Contains(other),  //Is Same or Friendly Tile
                TilingRuleOutput.Neighbor.NotThis => base.RuleMatch(neighborRule, other) && !m_FriendTiles.Contains(other), //Is Different ,  non-friendly or empty tile
            };
        }
    }
}

//[CustomEditor(typeof(TerrainRuleTile))] public class TerrainRuleEditor : OdinEditor { }