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
        public static void ApplyAdditionalTerrainTileSettings<T>(this T tile, bool isBillboard, Vector3Int position, ITilemap tilemap,ref TileData tileData) where T: TileBase
        {
            if (isBillboard && tilemap.IsSceneObject())
            {
                tile.RenderAsBillboard(position,tilemap,ref tileData);
            }
        }

        public static void RenderAsBillboard<T>(this T tile, Vector3Int position, ITilemap tilemap, ref TileData tileData) where T:TileBase
        {
            var transform = tilemap.GetComponent <Transform>();
            if (transform && transform.eulerAngles.x == 90)
            {
                tileData.transform *= Matrix4x4.Rotate(Quaternion.Euler(270f, 0, 0));
                tilemap.GetComponent<Tilemap>().SetTransformMatrix(position, tileData.transform);
            }
        }

        public static bool IsSceneObject(this ITilemap tilemap)
        {
            return tilemap.GetComponent <Transform>().gameObject.scene.path != "";
        }
    }
}