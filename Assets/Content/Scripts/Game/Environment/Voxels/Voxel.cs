using System;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Voxel : MonoBehaviour
{
    [Flags]
    public enum OptionalSides
    {
        None = 0,
        Back = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3
    }

    [SerializeField] public Sprite TopSprite;
    [SerializeField] public Sprite FrontSprite;
 
    [SerializeField] OptionalSides optionalSides = OptionalSides.None;

    Mesh mesh;

    private void Awake()
    {
        if(!mesh) BuildVoxel();
    }

    public void SetSides()
    {
        foreach (var side in Enum.GetNames(typeof(OptionalSides)))
        {
            if (TopSprite.name.Contains(side, StringComparison.InvariantCultureIgnoreCase))
            {
                optionalSides|= (OptionalSides)Enum.Parse(typeof(OptionalSides), side);
            }
        }
    }

    public void SetSides(OptionalSides sides)
    {
        optionalSides |= sides;
    }
    
    [Button]
    public void BuildVoxel()
    {
        SetSides();
        mesh = new Mesh();
        mesh.name = "VoxelCube";

        Vector3[] vertices = new Vector3[24]; // max possible, can shrink if needed
        int[] triangles = new int[36];
        Vector2[] uvs = new Vector2[24];

        Vector3 p0 = new Vector3(0, 0, 0);
        Vector3 p1 = new Vector3(1, 0, 0);
        Vector3 p2 = new Vector3(1, 1, 0);
        Vector3 p3 = new Vector3(0, 1, 0);
        Vector3 p4 = new Vector3(0, 0, 1);
        Vector3 p5 = new Vector3(1, 0, 1);
        Vector3 p6 = new Vector3(1, 1, 1);
        Vector3 p7 = new Vector3(0, 1, 1);

        int faceIndex = 0;

        // FRONT (always)
        AddFace(vertices, triangles, faceIndex++, p0, p1, p2, p3);

        // BACK
        if ((optionalSides & OptionalSides.Back) != 0)
            AddFace(vertices, triangles, faceIndex++, p5, p4, p7, p6);

        // LEFT
        if ((optionalSides & OptionalSides.Left) != 0)
            AddFace(vertices, triangles, faceIndex++, p4, p0, p3, p7);

        // RIGHT
        if ((optionalSides & OptionalSides.Right) != 0)
            AddFace(vertices, triangles, faceIndex++, p1, p5, p6, p2);

        // TOP (always)
        AddFace(vertices, triangles, faceIndex++, p3, p2, p6, p7);

        // BOTTOM
        if ((optionalSides & OptionalSides.Bottom) != 0)
            AddFace(vertices, triangles, faceIndex++, p4, p5, p1, p0);

        ApplySpriteUVs(uvs, faceIndex);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh     = mesh;
        GetComponent <MeshCollider>().sharedMesh = mesh;
    }

    void AddFace(Vector3[] v, int[] t, int face, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var offset = new Vector3(0.5f, 0.5f, 0.5f); // center pivot
        int vi = face * 4;
        int ti = face * 6;

        v[vi + 0] = a - offset;
        v[vi + 1] = b - offset;
        v[vi + 2] = c - offset;
        v[vi + 3] = d - offset;

        t[ti + 0] = vi + 0;
        t[ti + 1] = vi + 2;
        t[ti + 2] = vi + 1;
        t[ti + 3] = vi + 0;
        t[ti + 4] = vi + 3;
        t[ti + 5] = vi + 2;
    }

    void ApplySpriteUVs(Vector2[] uvs, int faceCount)
    {
        if (FrontSprite == null || TopSprite == null)
        {
            string log                  = gameObject.name;
            if(FrontSprite == null) log = " Front Missing";
            if(TopSprite == null)   log = " ,Top Missing";
            Debug.Log(log,gameObject);
        }
           
        int faceIndex = 0;

        // Front (always)
        SetFaceUVFromSprite(uvs, faceIndex++, FrontSprite);

        // Optional faces
        if ((optionalSides & OptionalSides.Back) != 0)
            SetFaceUVFromSprite(uvs, faceIndex++, FrontSprite);
        if ((optionalSides & OptionalSides.Left) != 0)
            SetFaceUVFromSprite(uvs, faceIndex++, FrontSprite);
        if ((optionalSides & OptionalSides.Right) != 0)
            SetFaceUVFromSprite(uvs, faceIndex++, FrontSprite);

        // Top (always)
        SetFaceUVFromSprite(uvs, faceIndex++, TopSprite);

        // Bottom (optional)
        if ((optionalSides & OptionalSides.Bottom) != 0)
            SetFaceUVFromSprite(uvs, faceIndex++, TopSprite);
    }

    static void SetFaceUVFromSprite(Vector2[] uvs, int faceIndex, Sprite sprite)
    {
        if (!sprite)
        {
           // Debug.LogError("Voxel: Sprites not assigned!");
            return;
        }
        
        Texture2D tex = sprite.texture;
        Rect r = sprite.rect;

        int start = faceIndex * 4;

        float xMin = r.xMin / tex.width;
        float yMin = r.yMin / tex.height;
        float xMax = r.xMax / tex.width;
        float yMax = r.yMax / tex.height;

        uvs[start + 0] = new Vector2(xMin, yMin);
        uvs[start + 1] = new Vector2(xMax, yMin);
        uvs[start + 2] = new Vector2(xMax, yMax);
        uvs[start + 3] = new Vector2(xMin, yMax);
    }
}
