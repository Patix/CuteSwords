using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class Create3DCollidersFromTilemap : SerializedMonoBehaviour
{
    private const float MinimumDeltaToMerge=0.001f;
   
    [SerializeField] private Tilemap[] tilemaps;
    [SerializeField] private Vector3   BaseOffset;
    [SerializeField] private SnapAxis  MergeAxis = SnapAxis.X | SnapAxis.Z;

    public  bool Merge;

    private Coroutine creationCoroutine;
    
    [Button]
    void Create()
    {
        if(creationCoroutine!=null) StopCoroutine(creationCoroutine);
        creationCoroutine=StartCoroutine(CreateCoroutine());
    }
    
    IEnumerator CreateCoroutine()
    {
        ClearChildren();
        
        List <(Sprite sprite, Vector3 position, int elevation)> ExtractedTiles = new List <(Sprite sprite, Vector3 position, int elevation)>();
        
        //Extract Tiles
        foreach (var tilemap in tilemaps)
        {
            yield return SampleAll(tilemap,(tileData, coordinates) =>
            {
                var realPosition   = GetSpriteVisualCenter(tilemap, tileData, coordinates);
                realPosition   =  Snapping.Snap(realPosition, Vector3.one * 0.1f);
                ExtractedTiles.Add((tileData.sprite,realPosition,-1));
            });
        }
        
        // Calculate Elevations without changing positions
        for (var i = 0; i < ExtractedTiles.Count; i++)
        {
            if(TileInfoDatabase.TryGetElevation(ExtractedTiles[i].sprite, out var elevationHeight))
            {
                for (var j = 0; j < ExtractedTiles.Count; j++)
                {
                    if(i == j) continue;
                
                    if (Mathf.Approximately(ExtractedTiles[j].position.x , ExtractedTiles[i].position.x) && ExtractedTiles[j].position.z > ExtractedTiles[i].position.z)
                    {
                        var tileInfo                           = ExtractedTiles[j];
                        tileInfo.elevation += elevationHeight;
                        ExtractedTiles[j] = tileInfo;
                    }
                }
            }
        }
        
        //Assign Elevations and change positions
        for (var i = 0; i < ExtractedTiles.Count; i++)
        {
            var tile = ExtractedTiles[i];
            
            if (tile.elevation > 0)
            {
                tile.position.y += tile.elevation;
                tile.position.z -= tile.elevation;
            }

            ExtractedTiles[i] = tile;
        }
        
        //Create Colliders
        for (var i = 0; i < ExtractedTiles.Count; i++)
        {
            var tile         = ExtractedTiles[i];
            var newColliders = CreateCollidersPrefab($"{tile.sprite.name}", tile.position, tile.sprite);
            
            if (newColliders is { Length: > 0 })
            {
                var mainCollider      = newColliders[0];
                var additiveColliders = newColliders.Skip(1).ToList();
                foreach (BoxCollider additiveCollider in additiveColliders)
                {
                 //   DestroyImmediate(additiveCollider);
                }
            }
        }
        
        if(Merge)
            MergeColliders();
        
        
        
    }
    
    
    [Button]
    void ClearChildren()
    {
        if (creationCoroutine != null) StopCoroutine(creationCoroutine);
        
        while (transform.childCount>0)
            for (int i = 0; i < transform.childCount; i++)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
    }

    Vector3 GetSpriteVisualCenter(Tilemap tilemap,TileData tileData, Vector3Int coordinateInt)
    {
        Vector3 coordinate  = tilemap.GetCellCenterWorld(coordinateInt);
        var     pivotOffset = 0.5f - tileData.sprite.pivot.y;
        coordinate.z+= pivotOffset; // Compensate Sprite Pivot
        return coordinate;
    }
  
    BoxCollider[] CreateCollidersPrefab(string name, Vector3 position, Sprite sprite)
    {

        var prefab      = TileInfoDatabase.GetSurfacePrefab(sprite);
        if (prefab == null) return default;

        var newPosition = position + prefab.transform.position;
        var child       = Instantiate(prefab, newPosition,prefab.transform.rotation);
        
        child.transform.parent   = transform;
        return child.GetComponents<BoxCollider>();
    }
   
    bool CanBeMerged(BoxCollider col1, BoxCollider col2, SnapAxis mergeAxis)
    {
        if (col1                   == col2) return false;
        if(col1.transform.rotation !=col2.transform.rotation) return false;
        if (mergeAxis              == SnapAxis.None) return false;
       
        bool      result = Mathf.Approximately(col1.bounds.center.y, col2.bounds.center.y); // Same Height (MUST);
        
        if(mergeAxis.HasFlag(SnapAxis.X))
        {
            result &= Mathf.Approximately(col1.bounds.center.x, col2.bounds.center.x); // Merge on Same X
            result &= Mathf.Approximately(col1.size.x,            col2.size.x); //With Same Wide
        }
        if(mergeAxis.HasFlag(SnapAxis.Z))
        {
            result &= Mathf.Approximately(col1.bounds.center.z, col2.bounds.center.z); // Merge on Same y
            result &= Mathf.Approximately(col1.bounds.size.z, col2.bounds.size.z); 
        }
       
        var bounds = col1.bounds;
        bounds.size *= 1.01f;

        result &= bounds.Intersects(col2.bounds);
        
        return result;
    }
    
    [Button]
    void MergeColliders()
    {
        if(MergeAxis.HasFlag(SnapAxis.X)) MergeOnAxis(SnapAxis.X);
        if(MergeAxis.HasFlag(SnapAxis.Z)) MergeOnAxis(SnapAxis.Z);
        
        void MergeOnAxis(SnapAxis axis)
        {
            var boxes = transform.GetComponentsInChildren <BoxCollider>().ToList();
            for (var i = 0; i < boxes.Count; i++)
            {
                var currentBox = boxes[i];
                if (!currentBox || !currentBox.gameObject) continue;

                var overlappingBoxes = boxes.Where(x => x && CanBeMerged(x, currentBox, axis));

                var newBounds = currentBox.bounds;

                while (overlappingBoxes.Any())
                {
                    foreach (var overlappingBox in overlappingBoxes)
                    {
                        if (!overlappingBox || !overlappingBox.gameObject) continue;
                        newBounds.Encapsulate(overlappingBox.bounds);

                        newBounds.center   = Round(newBounds.center, 4);
                        newBounds.size     = Round(newBounds.size,   4);
                       
                        if (overlappingBox.GetComponentsInChildren <BoxCollider>()?.Length > 1)
                        {
                            DestroyImmediate(overlappingBox);
                        }

                        else
                        {
                            DestroyImmediate(overlappingBox.gameObject);
                        }
                    }
                }


                if (currentBox.bounds.size != newBounds.size)
                {
                    currentBox.size   = newBounds.size;
                    currentBox.center = newBounds.center - currentBox.transform.position;
                }
            }
        }
    }
   
    IEnumerator SampleAll(Tilemap tilemap,Action<TileData, Vector3Int> onTileDataFound)
    {
        for (var x = tilemap.cellBounds.min.x; x <= tilemap.cellBounds.max.x; x++)
        {
            for (var y = tilemap.cellBounds.min.y; y <= tilemap.cellBounds.max.y; y++)
            {
                var coordinates = new Vector3Int(x, y);
                var tileData    = Sample(tilemap,coordinates);

                if (tileData.sprite)
                {
                    onTileDataFound?.Invoke(tileData,coordinates);
                }

                if(false) yield return null;
            }
        }
    }
    
    TileData Sample(Tilemap tilemap,Vector3Int position)
    {
        TileData tileData = default;
        var      tile     = tilemap.GetTile(position);
        if (tile != default){
            tile.GetTileData(position, tilemap, ref tileData);
        }
        return tileData;
    }

    private Vector3 Round(Vector3 vector3, int numberOfDigits = 4)
    {
        return new Vector3((float)Math.Round(vector3.x, numberOfDigits),
                           (float)Math.Round(vector3.y, numberOfDigits),
                           (float)Math.Round(vector3.z, numberOfDigits));
    }
  
    #if UNITY_EDITOR
   
    [BoxGroup("Debug")] [ShowInInspector, LabelText("Merging Colliders")] private bool e_DebugMerge = true;
    
    private List <(Vector3 start, Vector3 end)> e_DebugMergeLines      = new List <(Vector3, Vector3)>();
    private bool                                e_LinesNeedToBeUpdated = true;
    
    private void OnValidate()
    {
        e_LinesNeedToBeUpdated = true;;
    }

    private void OnTransformChildrenChanged()
    {
        e_LinesNeedToBeUpdated = true;;
    }
    

    private void OnDrawGizmosSelected()
    {
        if(!e_DebugMerge) return;

        if (e_LinesNeedToBeUpdated)
        {
            e_LinesNeedToBeUpdated = false;
            e_DebugMergeLines.Clear();
            var colliders = transform.GetComponentsInChildren<BoxCollider>();
            
            List <SnapAxis> mergeAxises   = new List<SnapAxis>();
            if(MergeAxis.HasFlag(SnapAxis.X)) mergeAxises.Add(SnapAxis.X);
            if(MergeAxis.HasFlag(SnapAxis.Z)) mergeAxises.Add(SnapAxis.Z);
        
            foreach (BoxCollider col in colliders)
            {
                var mergables = colliders.Where(col2=>mergeAxises.Any(axis=> CanBeMerged(col,col2,axis)));
                e_DebugMergeLines.AddRange(mergables.Select(x=> (x.bounds.center, col.bounds.center)));
            }
        }
        
        foreach (var line in e_DebugMergeLines)
        {
            Debug.DrawLine(line.start,line.end,Color.red);
        }
      
    }
    
    #endif
  
}
