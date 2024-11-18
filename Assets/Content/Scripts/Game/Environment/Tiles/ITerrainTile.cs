using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Environment.Tiles
{
    public interface ITerrainTile
    {
        TerrainTileData      TerrainTileData { get; }
    }
    
    public static class TerrainTileExtensions
    {
        public static void SetTileAndApplySettings(this Tilemap tilemap, TileBase tile,Vector3Int position)
        {
            tilemap.SetTile(position,tile);
            ApplyAdditionalTerrainTileSettings(tilemap,position);
        }
        public static void ApplyAdditionalTerrainTileSettings(this Tilemap tilemap, Vector3Int position)
        {
            var tileIsBillboardTerrain = tilemap.GetTile(position) is ITerrainTile { TerrainTileData: { m_IsBillboard: true } };
            
            if ( tileIsBillboardTerrain&& tilemap.IsSceneObject())
            {
                tilemap.RenderAsBillboard(position);
            }
        }
        
        public static void RenderAsBillboard(this Tilemap tilemap, Vector3Int position)
        {
            var tilemapTransform = tilemap.transform;
            
            if (tilemapTransform && tilemapTransform.eulerAngles.x == 90)
            {
                var tileMatrix = tilemap.GetTransformMatrix(position);
                var newMatrix  = Matrix4x4.TRS(tileMatrix.GetPosition(), Quaternion.Euler(270f, 0, 0), tileMatrix.lossyScale);
                tilemap.SetTransformMatrix(position, newMatrix);
            }
        }

        public static bool IsSceneObject(this Tilemap tilemap)
        {
            return tilemap.gameObject.scene.path != "";
        }
    }
}