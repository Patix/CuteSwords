using System.Collections;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class CameraCustomSortAxis : MonoBehaviour
{
    private          Camera    camera;
    private readonly Hashtable defaultSettings = new();
    private void OnEnable()
    {
        if (!camera) camera = GetComponent <Camera>();
        SaveDefaults();
        camera.transparencySortMode = TransparencySortMode.CustomAxis;
        camera.transparencySortAxis = transform.forward;
    }

    private void OnDisable() { RestoreDefaults(); }

    private void SaveDefaults()
    {
        defaultSettings["SortDirection"]        = camera.transparencySortAxis;
        defaultSettings["TransparencySortMode"] = camera.transparencySortMode;
    }

    private void RestoreDefaults()
    {
        if (defaultSettings.Count == 0 || !camera) return;

        camera.transparencySortAxis = (Vector3)defaultSettings["SortDirection"];
        camera.transparencySortMode = (TransparencySortMode)defaultSettings["TransparencySortMode"];
    }
}