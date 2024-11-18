using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Environment.Tiles.Editor
{
    [CustomGridBrush(true, false, false, "Terrain Brush")]
    public class TerrainBrush : GridBrush
    {
        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            base.Paint(gridLayout, brushTarget, position);
            var tilemap     = brushTarget.GetComponent <Tilemap>();
            tilemap.ApplyAdditionalTerrainTileSettings(position);
        }
    }

    [CustomEditor(typeof(TerrainBrush))]
    public class TerrainBrushEditor : GridBrushEditor
    {
        public override void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            base.PaintPreview(gridLayout, brushTarget, position);
            ApplyBillboardIfNeeded(brushTarget,position);
        }

        private void ApplyBillboardIfNeeded(GameObject brushTarget, Vector3Int position)
        {
            var min    = position - brush.pivot;
            var max    = min      + brush.size;
            var bounds = new BoundsInt(min, max - min);

            if (brushTarget != null)
            {
                var map = brushTarget.GetComponent<Tilemap>();
                if (map != null)
                {
                    foreach (var location in bounds.allPositionsWithin)
                    {
                        var brushPosition = location - min;
                        var cell          = brush.cells[brush.GetCellIndex(brushPosition)];
                        if (cell.tile is ITerrainTile { TerrainTileData: { m_IsBillboard: true } })
                        {
                            var previewMatrix = Matrix4x4.TRS(cell.matrix.GetPosition(), Quaternion.Euler(270f, 0f, 0f), cell.matrix.lossyScale);
                            map.SetEditorPreviewTransformMatrix(location, previewMatrix);
                        }
                    }
                }
            }
        }
    }
}