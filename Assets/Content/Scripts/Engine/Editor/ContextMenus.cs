using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ContextMenus
{

    #region Transform

    [MenuItem("CONTEXT/"+nameof(Transform)+"/"+"Advanced/Reset Transform Without Affecting Children")]
    static void ResetTransformWithoutAffectingChildren(MenuCommand command)
    {
        var transform = (Transform)command.context;
        var children  = Enumerable.Range(0, transform.childCount).Select(i => transform.GetChild(i)).ToList();
        
        foreach (Transform child in children)
        {
            child.parent = null;
        }
        
        transform.position   = transform.localEulerAngles =Vector3.zero;
        transform.localScale = Vector3.one;
        Debug.Log(transform.eulerAngles);
        foreach (Transform child in children)
        {
            child.parent = transform;
        }
    }
    

    #endregion
}
