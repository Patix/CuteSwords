using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Create3DCollidersFromTilemap : SerializedMonoBehaviour
{
    [SerializeField]private Dictionary <Sprite, int> Elevations;
    [SerializeField]private Dictionary <Sprite, Vector3> Rotations;
    
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Vector3           BaseOffset;


    public  bool Merge;
    private bool continueButton;

    private Coroutine creationCoroutine;
    [Button]
    void Create()
    {
        if(creationCoroutine!=null) StopCoroutine(creationCoroutine);
        creationCoroutine=StartCoroutine(CreateCoroutine());
    }

    [Button,ShowIf("@creationCoroutine!=null")]
    void Continue()
    {
        continueButton = true;
    }

    IEnumerator CreateCoroutine()
    {
        var currentRow       = -1;
        var currentColumn    = -1;
        var currentElevation = BaseOffset.y;
        
        ClearChildren();

        var colliders = new Dictionary <BoxCollider,Sprite>();
        
        yield return SampleAll((tileData, coordinates) =>
        {   
            if(!Rotations.TryGetValue(tileData.sprite,out var rotation))
            {
                rotation = Vector3.right * 90f;
            }
            var realPosition = tilemap.CellToWorld(coordinates)+BaseOffset;
            var newCollider  = CreateCollider($"{coordinates} {tileData.sprite.name}", realPosition, rotation);
            colliders.Add(newCollider,tileData.sprite);
        });
     
        
        foreach (var (collider,sprite) in colliders)
        {
            if (Elevations.TryGetValue(sprite, out var elevationHeight))
            {
                foreach (var (otherCollider,_) in colliders )
                {
                    if (Mathf.Approximately(otherCollider.transform.position.x , collider.transform.position.x) && otherCollider.transform.position.z > collider.transform.position.z)
                    {
                        //Elevate Consequtive Tiles
                        var newPosition = otherCollider.transform.position;
                        newPosition.y++;
                        newPosition.z--;
                        otherCollider.transform.position = newPosition;
                    }
                }
            }
        }

        foreach (var (collider,sprite) in colliders)
        {
            if (Rotations.TryGetValue(sprite, out var rotation))
            {
                collider.transform.eulerAngles = rotation; 
                
                //if is ladder
                if (rotation.x == 45)
                {
                    var position = collider.transform.position;
                    position.z--;
                    collider.transform.position = position;
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
    BoxCollider CreateCollider(string name, Vector3 position, Vector3 eulerAngles)
    {

        var child       = new GameObject(name).transform;
        var boxCollider = child.gameObject.AddComponent <BoxCollider>();

        boxCollider.size   = new Vector3(1,1,0.01f);
        
        if (Mathf.Approximately(eulerAngles.x, 45))
        {
            Debug.Log("CU");
            boxCollider.size = new Vector3(1, 1.41f, 1f);
        }
        
        boxCollider.center = boxCollider.size / 2;

       
        child.position = position;
        child.rotation = Quaternion.Euler(eulerAngles);
        child.parent   = transform;
        return boxCollider;
    }
   
    bool CanBeMerged(BoxCollider col1, BoxCollider col2, SnapAxis mergeAxis)
    {
        if (col1 == col2) return false;
        
        bool      result = Mathf.Approximately(col1.transform.position.y, col2.transform.position.y); // Same Height (MUST);
        
        switch (mergeAxis)
        {
            case SnapAxis.X: result &= Mathf.Approximately(col1.transform.position.x, col2.transform.position.x); // Merge on Same X
                break;
            case SnapAxis.Z: result &= Mathf.Approximately(col1.transform.position.z, col2.transform.position.z); // Or Same Z
                break;
        }
        
        var bounds = col1.bounds;
        bounds.size *= 1.01f;

        result &= bounds.Intersects(col2.bounds) && col1.transform.eulerAngles == col2.transform.eulerAngles;
        
        return result;
    }
    
    [Button]
    void MergeColliders()
    {
         foreach (var axis in new[] {SnapAxis.X, SnapAxis.Z})
         {
             var boxes    = transform.GetComponentsInChildren <BoxCollider>().ToList();
             for (var i = 0; i < boxes.Count; i++)
             {
                 var currentBox       = boxes[i];
                 if(!currentBox || !currentBox.gameObject) continue;

                 var overlappingBoxes = boxes.Where(x => x && CanBeMerged(x, currentBox,axis));

                 var newBounds = currentBox.bounds;
          
                 while (overlappingBoxes.Any())
                 {
                     foreach (var overlappingBox in overlappingBoxes)
                     {
                         if(!overlappingBox || !overlappingBox.gameObject) continue;
                         newBounds.Encapsulate(overlappingBox.bounds);
                         DestroyImmediate(overlappingBox.gameObject);
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
   
    IEnumerator SampleAll(Action<TileData, Vector3Int> onTileDataFound)
    {
        for (var x = tilemap.cellBounds.min.x; x <= tilemap.cellBounds.max.x; x++)
        {
            for (var y = tilemap.cellBounds.min.y; y <= tilemap.cellBounds.max.y; y++)
            {
                var coordinates = new Vector3Int(x, y);
                var tileData    = Sample(coordinates);

                if (tileData.sprite)
                {
                    onTileDataFound?.Invoke(tileData,coordinates);
                    continueButton = false;
                    
                    while (false)
                    {
                        yield return null;
                    }
                }
            }
        }
    }
    
    TileData Sample(Vector3Int position)
    {
        TileData tileData = default;
        var      tile     = tilemap.GetTile(position);
        if (tile != default){
            tile.GetTileData(position, tilemap, ref tileData);
        }
        return tileData;
    }
}
