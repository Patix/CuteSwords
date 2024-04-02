using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveRenderTexture : MonoBehaviour
{
    [SerializeField]
    RenderTexture rt;

    [SerializeField]
    Camera cam;

    [SerializeField]
    string fileName;

    [ContextMenu("Save png")]
    public void SavePNG()
    {
       // RenderTexture mRt = new RenderTexture(rt.width, rt.height, rt.depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        //mRt.antiAliasing = rt.antiAliasing;
        var mRt = rt;
        var tex = new Texture2D(mRt.width, mRt.height, TextureFormat.ARGB32, false);
        cam.targetTexture = mRt;
        cam.Render();
        RenderTexture.active = mRt;

        tex.ReadPixels(new Rect(0, 0, mRt.width, mRt.height), 0, 0);
        tex.Apply();

        var path = "Assets" + fileName + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        return;
        Debug.Log("Saved file to: " + path);

        DestroyImmediate(tex);

        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        DestroyImmediate(mRt);
    }
}
