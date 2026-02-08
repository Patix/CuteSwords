using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class VoxelPrefabsGenerator : MonoBehaviour
{
    [SerializeField] private GameObject voxelPrefab;
    [SerializeField] List<Sprite> sprites;

    [SerializeField] List <int> _left,center, right, solowithRoot;
    [SerializeField] List <int> _hasRoot;
    [SerializeField] List <int> roots;
    [SerializeField] List <int> solo;
    
    [Button]
    void GenerateVoxels()
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            Sprite     spr      = sprites[i];
         
            var row=GetRow(spr);
            var col=GetColumn(spr);
          
            if(roots.Contains(row)) continue;
            
            GameObject newVoxel = Instantiate(voxelPrefab, transform);
            newVoxel.transform.localPosition=new Vector3(col, 0, -row);
            
            newVoxel.name=$"{row}_{col}_{spr.name}";
            Voxel voxelComponent = newVoxel.GetComponent<Voxel>();

            if (_left.Contains(col))
            {
                voxelComponent.AddSides(Voxel.OptionalSides.Left);
            }

            if (right.Contains(col))
            {
                voxelComponent.AddSides(Voxel.OptionalSides.Right);
            }
            
            if(solowithRoot.Contains(row))
                voxelComponent.AddSides(Voxel.OptionalSides.Left|Voxel.OptionalSides.Right);
            
            voxelComponent.TopSprite   = spr;
            voxelComponent.FrontSprite = GetRoot(spr);
            voxelComponent.BuildVoxel();
        }
    }

    
    private Sprite GetRoot(Sprite sprite)
    { 
        var nearestRootRow =  roots.Find(x=>x>GetRow(sprite));
        return sprites.Find(x=>GetRow(x)==nearestRootRow && GetColumn(x)==GetColumn(sprite));
    }
    
    private int GetColumn(Sprite sprite)
    {
        return (int)(sprite.rect.min.x / sprite.pixelsPerUnit);
    }
    
    private int GetRow(Sprite sprite)
    {
        return (int)((sprite.texture.height-sprite.rect.yMax) / sprite.pixelsPerUnit);
    }
}
