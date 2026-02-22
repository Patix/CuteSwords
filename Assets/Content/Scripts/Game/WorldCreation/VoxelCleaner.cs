using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

public class VoxelCleaner
{
    struct FaceData
    {
        public Vector3 normal;
        public Color color;
    }

    struct VoxelData
    {
        public Dictionary<Vector3, Color> faceColors;
    }

    public static Mesh Clean(Mesh sourceMesh, float tolerance = 0.001f)
    {
        Vector3[] verts = sourceMesh.vertices;
        int[] tris = sourceMesh.triangles;
        Color[] colors = sourceMesh.colors.Length > 0 ? sourceMesh.colors : null;

        Dictionary<Vector3, VoxelData> voxels = new(new VecComparer(tolerance));

        // 1) Extract voxel centers + face colors
        for (int i = 0; i < tris.Length; i += 6)
        {
            int ia = tris[i];
            int ib = tris[i + 1];
            int ic = tris[i + 2];
            int id = tris[i + 5];

            Vector3 a = verts[ia];
            Vector3 b = verts[ib];
            Vector3 c = verts[ic];
            Vector3 d = verts[id];

            Vector3 faceCenter = (a + b + c + d) / 4f;
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            normal = SnapNormal(normal);

            float size = EstimateCubeSize(a, b, c);
            Vector3 voxelCenter = faceCenter - normal * size * 0.5f;
            voxelCenter = Snap(voxelCenter, tolerance);

            Color faceColor = Color.white;
            if (colors != null)
                faceColor = (colors[ia] + colors[ib] + colors[ic] + colors[id]) / 4f;

            if (!voxels.TryGetValue(voxelCenter, out var voxel))
                voxel = new VoxelData { faceColors = new Dictionary<Vector3, Color>() };

            voxel.faceColors[normal] = faceColor;
            voxels[voxelCenter] = voxel;
        }

        // 2) Rebuild mesh with colors
        return BuildMesh(voxels);
    }

    static Mesh BuildMesh(Dictionary<Vector3, VoxelData> voxels)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();
        List<Color> colors = new();

        Vector3[] dirs =
        {
            Vector3.forward, Vector3.back,
            Vector3.up, Vector3.down,
            Vector3.right, Vector3.left
        };

        foreach ((Vector3 position, VoxelData voxel) in voxels)
        {
            var offset = -Vector3.one * 0.5f;
            foreach (var dir in dirs)
            {
                Vector3 neighbor = position + dir;
                if (voxels.ContainsKey(neighbor))
                    continue; // hidden face

                int start = vertices.Count;
                AddFace(vertices, position+offset, dir);

                foreach (var triangle in stackalloc[] { 0, 1, 2, 0, 2, 3 })
                    triangles.Add(start + triangle);

                foreach (var uv in stackalloc[] { Vector2.zero, Vector2.right, Vector2.right + Vector2.up, Vector2.up }) 
                    uvs.Add(uv);
                
                Color c = voxel.faceColors.TryGetValue(dir, out var fc) ? fc : Color.white;
                for (var i = 0; i < 4; i++) 
                    colors.Add(c);
               
            }
        }

        Mesh mesh = new()
        { 
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32, 
            vertices = vertices.ToArray(), 
            triangles = triangles.ToArray(), 
            uv = uvs.ToArray(),
            colors                         = colors.ToArray()
        };
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static void AddFace(List<Vector3> v, Vector3 p, Vector3 dir)
    {
        Vector3[] quad = new Vector3[4];

        if (dir == Vector3.forward)
        {
            quad[0] = p + Vector3.forward;
            quad[1] = p + Vector3.right+Vector3.forward;
            quad[2] = p + Vector3.right+Vector3.forward+Vector3.up;
            quad[3] = p + Vector3.up+Vector3.forward;
        }
        else if (dir == Vector3.back)
        {
            quad[0] = p + new Vector3(1, 0, 0);
            quad[1] = p + new Vector3(0, 0, 0);
            quad[2] = p + new Vector3(0, 1, 0);
            quad[3] = p + new Vector3(1, 1, 0);
        }
        else if (dir == Vector3.up)
        {
            quad[0] = p + new Vector3(0, 1, 1);
            quad[1] = p + new Vector3(1, 1, 1);
            quad[2] = p + new Vector3(1, 1, 0);
            quad[3] = p + new Vector3(0, 1, 0);
        }
        else if (dir == Vector3.down)
        {
            quad[0] = p + new Vector3(0, 0, 0);
            quad[1] = p + new Vector3(1, 0, 0);
            quad[2] = p + new Vector3(1, 0, 1);
            quad[3] = p + Vector3.forward;
        }
        else if (dir == Vector3.right)
        {
            quad[0] = p + new Vector3(1, 0, 1);
            quad[1] = p + new Vector3(1, 0, 0);
            quad[2] = p + new Vector3(1, 1, 0);
            quad[3] = p + new Vector3(1, 1, 1);
        }
        else if (dir == Vector3.left)
        {
            quad[0] = p + new Vector3(0, 0, 0);
            quad[1] = p + new Vector3(0, 0, 1);
            quad[2] = p + new Vector3(0, 1, 1);
            quad[3] = p + new Vector3(0, 1, 0);
        }

        v.AddRange(quad);
    }

    static float EstimateCubeSize(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Min(Vector3.Distance(a, b), Vector3.Distance(a, c));
    }

    static Vector3 Snap(Vector3 v, float t)
    {
        return new Vector3(
            Mathf.Round(v.x / t) * t,
            Mathf.Round(v.y / t) * t,
            Mathf.Round(v.z / t) * t
        );
    }

    static Vector3 SnapNormal(Vector3 n)
    {
        n = n.normalized;
        if (Mathf.Abs(n.x) > 0.9f) return new Vector3(Mathf.Sign(n.x), 0, 0);
        if (Mathf.Abs(n.y) > 0.9f) return new Vector3(0, Mathf.Sign(n.y), 0);
        return new Vector3(0, 0, Mathf.Sign(n.z));
    }

    class VecComparer : IEqualityComparer<Vector3>
    {
        float t;
        public VecComparer(float tolerance) => t = tolerance;

        public bool Equals(Vector3 a, Vector3 b) =>
            Vector3.Distance(a, b) < t;

        public int GetHashCode(Vector3 v) =>
            (Mathf.RoundToInt(v.x / t) * 73856093) ^
            (Mathf.RoundToInt(v.y / t) * 19349663) ^
            (Mathf.RoundToInt(v.z / t) * 83492791);
    }
}

#if UNITY_EDITOR
public static class VoxelCleanerContextMenuSceneView
{
    // Validator: only enable menu if the component exists (optional)
    [MenuItem("CONTEXT/MeshRenderer/Combine Voxels", true)]
    private static bool ValidateCustomAction(MenuCommand command)
    {
        return command.context is MeshRenderer;
    }

    // Action: called when the menu item is clicked
    [MenuItem("CONTEXT/MeshRenderer/Combine Voxels")]
    private static void CustomAction(MenuCommand command)
    {
        MeshFilter mf = (command.context as MeshRenderer).GetComponent<MeshFilter>();
        if (mf != null)
        {
           CleanMesh(mf);
        }
    }
    
    public static void CleanMesh(MeshFilter meshFilter)
    {
        Mesh mesh = meshFilter.sharedMesh;
        mesh                                    = VoxelCleaner.Clean(mesh,1);
        meshFilter.sharedMesh= mesh;
        MeshCollider meshCollider = meshFilter.GetComponent<MeshCollider>();
        
        if (meshCollider != null) 
            meshCollider.sharedMesh = mesh;
       
    }
}
#endif