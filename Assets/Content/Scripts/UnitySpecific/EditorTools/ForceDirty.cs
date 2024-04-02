using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public static class ForceDirty
    {
        #if UNITY_EDITOR
        [HideInInspector,MenuItem("Set Dirty/Set Dirty and Force Save", priority = 0)]
        static void SetDirtyAndForceSave()
        {
            if (!Selection.activeObject || !EditorUtility.IsPersistent(Selection.activeObject)) return;
            EditorUtility.SetDirty(Selection.activeObject);
            AssetDatabase.SaveAssets();
            Debug.Log(Selection.activeObject +" Saved");
        }
        #endif
    }
}