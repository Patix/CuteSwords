using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Voxel : MonoBehaviour
{
    [Flags]
    public enum OptionalSides
    {
        None   = 0,
        Back   = 1 << 0,
        Left   = 1 << 1,
        Right  = 1 << 2,
        Bottom = 1 << 3
    }

    [PreviewField]
    public Sprite TopSprite,
                  FrontSprite;

    [SerializeField] OptionalSides optionalSides = OptionalSides.None;

    Mesh mesh;

    // ============================ BUILD ============================
    [ShowInInspector] private Vector3 _offset = Vector3.one*0.5f;
    
    [Button]
    public void BuildVoxel(bool forceCreateNewMesh = false, bool saveMesh = false , bool overwriteExisting = false)
    {
        #if UNITY_EDITOR
        string meshPath = AssetDatabase.GetAssetPath(GetComponent<MeshFilter>()?.sharedMesh);
        #endif


        if (overwriteExisting)
        {
            mesh = GetComponent<MeshFilter>()?.sharedMesh;
        }
        
        if ((!overwriteExisting) &&forceCreateNewMesh || mesh == null)
            mesh = new Mesh { name = gameObject.name };

        int faceCount = 2;
        if (Has(OptionalSides.Back))   faceCount++;
        if (Has(OptionalSides.Left))   faceCount++;
        if (Has(OptionalSides.Right))  faceCount++;
        if (Has(OptionalSides.Bottom)) faceCount++;

        Vector3[] vertices  = new Vector3[faceCount * 4];
        Vector2[] uvs       = new Vector2[faceCount * 4];
        int[] triangles     = new int[faceCount * 6];
        Color[] colors      = new Color[faceCount * 4]; // tile index stored here

        Vector3 s = _offset;

        Vector3 p0 = new(-s.x, -s.y, -s.z);
        Vector3 p1 = new( s.x, -s.y, -s.z);
        Vector3 p2 = new( s.x,  s.y, -s.z);
        Vector3 p3 = new(-s.x,  s.y, -s.z);
        Vector3 p4 = new(-s.x, -s.y,  s.z);
        Vector3 p5 = new( s.x, -s.y,  s.z);
        Vector3 p6 = new( s.x,  s.y,  s.z);
        Vector3 p7 = new(-s.x,  s.y,  s.z);

        int face = 0;

        int topTile  = GetTileIndexFromSprite(TopSprite);

        if (FrontSprite)
        {
            int sideTile = GetTileIndexFromSprite(FrontSprite);
           
            // FRONT
            AddFace(vertices, triangles, uvs, colors, face++, p0, p1, p2, p3, sideTile);

            // BACK
            if (Has(OptionalSides.Back))
                AddFace(vertices, triangles, uvs, colors, face++, p5, p4, p7, p6, sideTile);

            // LEFT
            if (Has(OptionalSides.Left))
                AddFace(vertices, triangles, uvs, colors, face++, p4, p0, p3, p7, sideTile);

            // RIGHT
            if (Has(OptionalSides.Right))
                AddFace(vertices, triangles, uvs, colors, face++, p1, p5, p6, p2, sideTile);
        }

        if (TopSprite)
        {
            // TOP
            AddFace(vertices, triangles, uvs, colors, face++, p3, p2, p6, p7, topTile);

            // BOTTOM
            if (Has(OptionalSides.Bottom))
                AddFace(vertices, triangles, uvs, colors, face++, p4, p5, p1, p0, topTile);
        }
       

        mesh.Clear();
        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.uv        = uvs;
        mesh.colors    = colors;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        #if UNITY_EDITOR
        
        if (saveMesh)
        {
            if(!overwriteExisting) SaveMesh(mesh, gameObject.name);
            else SaveMesh(mesh,gameObject.name, meshPath);

        }
        #endif
     
           
    }
    public void AddSides(OptionalSides side)
    {
        optionalSides |= side;
    }

    
    [Button]
    private void SetColor(int index)
    {
        if (!mesh) mesh = GetComponent <MeshFilter>().sharedMesh;
        var colors      = mesh.colors32;
        for (var i = 0; i < colors.Length; i++)
        {
            colors[i].r= (byte)index;
        }
        
        mesh.colors32 = colors;
    }
   
    // ============================ HELPERS ============================

    bool Has(OptionalSides s) => (optionalSides & s) != 0;

    static void AddFace(
        Vector3[] v,
        int[] t,
        Vector2[] uv,
        Color[] c,
        int face,
        Vector3 a,
        Vector3 b,
        Vector3 c1,
        Vector3 d,
        int tileIndex)
    {
        int vi = face * 4;
        int ti = face * 6;

        v[vi + 0] = a;
        v[vi + 1] = b;
        v[vi + 2] = c1;
        v[vi + 3] = d;

        t[ti + 0] = vi + 0;
        t[ti + 1] = vi + 2;
        t[ti + 2] = vi + 1;
        t[ti + 3] = vi + 0;
        t[ti + 4] = vi + 3;
        t[ti + 5] = vi + 2;

        uv[vi + 0] = new(0, 0);
        uv[vi + 1] = new(1, 0);
        uv[vi + 2] = new(1, 1);
        uv[vi + 3] = new(0, 1);

        float encoded = Mathf.Clamp(tileIndex, 0, 255) / 255f;
        Color col = new(encoded, 0, 0, 1);

        c[vi + 0] = col;
        c[vi + 1] = col;
        c[vi + 2] = col;
        c[vi + 3] = col;
    }

    int GetTileIndexFromSprite(Sprite sprite)
    {
        if (!sprite) return 0;
        
        float row     = (sprite.texture.height-sprite.rect.yMax) / sprite.pixelsPerUnit;
        float col     = sprite.rect.min.x                        / sprite.pixelsPerUnit;
        float totalColumns = (sprite.texture.width / sprite.rect.width);
        return (int) (row * totalColumns + col);
    }

    [ContextMenu("Name/From Top Sprite")]
    private void NameFromTopSprite()
    {
        Rename(TopSprite.name);
    }
    
    [ContextMenu("Name/From Front Sprite")]
    private void NameFromFrontSprite()
    { 
        Rename(FrontSprite.name);
    }

    private void Rename(string newName)
    {
        var oldName = gameObject.name;
        gameObject.name = TopSprite.name;
        
        string path = AssetDatabase.GetAssetPath(gameObject);
        if(string.IsNullOrEmpty(path)) return;
        
        AssetDatabase.RenameAsset(path, path.Replace(oldName, gameObject.name));
        PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
      
    }

    public static void SaveMesh(Mesh mesh, string name, string path = null)
    {
        #if UNITY_EDITOR

        if (string.IsNullOrEmpty(path)) path = $"Assets/Content/Meshes/{name}.mesh";

        // If an asset exists at the path, update it in-place. Otherwise, create a new asset.
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            // Copy all serialized data from the provided mesh into the existing asset.
            EditorUtility.CopySerialized(mesh, existing);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
        }
        else
        {
            // Create a new asset when none exists at the given path.
            UnityEditor.AssetDatabase.CreateAsset(mesh, path);
            UnityEditor.AssetDatabase.SaveAssets();
        }
        #endif
    }


}
