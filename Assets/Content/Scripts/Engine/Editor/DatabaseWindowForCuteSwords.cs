using System;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.GettingStarted;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DatabaseWindowForCuteSwords : OdinMenuEditorWindow
{
    private   static     DatabaseWindowForCuteSwords instance;

    [MenuItem("Database/Open")]
    public static void OpenWindow()
    {
        instance=GetWindow<DatabaseWindowForCuteSwords>();
        instance.Show();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        instance = null;
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree          = new();
        // Main
        AddtoHierarchy<TileInfoDatabase>(tree, "Main");
        
        return tree;
    }

    private void ShowButton(Rect rect)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(MenuWidth);
        if(SirenixEditorGUI.Button("Select this asset", ButtonSizes.Large))
        {
            Selection.activeObject = (UnityEngine.Object)MenuTree.Selection.SelectedValue;
        }
        
        if(SirenixEditorGUI.Button("Open in floating window", ButtonSizes.Large))
        {
           EditorUtility.OpenPropertyEditor((UnityEngine.Object)MenuTree.Selection.SelectedValue);
        }
        
        if(SirenixEditorGUI.Button("Force Save Asset", ButtonSizes.Large))
        {
            UnityEditor.AssetDatabase.SaveAssets();
        }
        
   
        GUILayout.EndHorizontal();;
    }

    private static void AddtoHierarchy<T>(OdinMenuTree tree, string rootName, Func<T, string> subName=null , Func<T,Sprite> image=null) where T:UnityEngine.Object
    {

        var allAssetsOfTypeT = LoadAndReturnAll<T>();
        var niceTypeName     = SplitNameAndRemoveSOPrefix(typeof(T).GetNiceName());
        
        foreach (var item in allAssetsOfTypeT )
        {
            var path = $"{rootName}/{niceTypeName}";
            
            if (subName != null)
            {
                path += $"/{subName(item)}";
            }

            if (image == null)
            {
                tree.Add(path, item);
            }

            else
            {
                tree.Add(path, item, image(item));
            }
        }
    }

    private static string SplitNameAndRemoveSOPrefix(string ScriptableObjectSoName)
    {
      
        var splitByUpperCase = Regex.Matches(ScriptableObjectSoName, "([A-Z][^A-Z]+)");
        if (splitByUpperCase.Count==0) return ScriptableObjectSoName;
        
        
        var stringBuilder = new StringBuilder();
        foreach (var word in splitByUpperCase)
        {
            stringBuilder.Append(word);
            stringBuilder.Append(" ");
        }

        return stringBuilder.ToString();
    }
    
    
    private static T[] LoadAndReturnAll<T>() where T: UnityEngine.Object
    {
        var foundAssetPaths=AssetDatabase.FindAssets($"t:{typeof(T)}");
        if (foundAssetPaths.Length <= 0) return null;
        
        var resultArray = new T[foundAssetPaths.Length];
        for (var i = 0; i < foundAssetPaths.Length; i++)
        {
            resultArray[i] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(foundAssetPaths[i]));
        }

        return resultArray;

    }
}
