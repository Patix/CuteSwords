using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Patik.EditorTools
{
    /// <summary>
    /// Creates Scriptable Objects in "Assets/ScriptableObjects" without need of  adding CreateAssetMenuAttribute on ScriptableObject Derived Classes
    /// </summary>
    public class CreateScriptableObject_EditorWindow : EditorWindow
    {
       /// <summary>
       /// Will EXCLUDE classes with namespace STARTED with following strings.
       /// </summary>
       private static string[] ExcludedNamespaces = { "UnityEngine.", "UnityEditor.", "Unity." };
   
       /// <summary>
       /// Filtered out Types (Stripped UnityEngine , UnityEditor Objects)
       /// </summary>
       Dictionary<string, Type> SearchableTypes = new Dictionary<string, Type>();
       
       /// <summary>
       /// Text which is typed in search bar is stored here 
       /// </summary>
       private string searchString = string.Empty;
       /// <summary>
       /// fills dropdown with string values
       /// </summary>
       private string[] searchableTypesCachedAsStringsForDropDown;
       private int selectionIndexForDropDown;
   
   
       [MenuItem("Tools/Patik/Scriptable Object/Create")]
       static void ShowWindow()
       {
   
           CreateScriptableObject_EditorWindow window = (CreateScriptableObject_EditorWindow)GetWindow(typeof(CreateScriptableObject_EditorWindow));
           window.maxSize= new Vector2(350,100);
           window.titleContent = new GUIContent(" Create Scriptable Object");
           window.LoadTypes();
           window.Show();
       }
   
       void OnGUI()
       {
           GUILayout.Label("Engine and abstract classes are hidden from search");
   
           //save string from old frame
           var oldString = searchString;
           //Read String from Field
           searchString = EditorGUILayout.TextField(searchString);
   
           //If Search String is not Changed , don't re-filter each frame
           if (oldString != searchString)
           {
               searchableTypesCachedAsStringsForDropDown = SearchableTypes.Keys.ToArray();
               if (!string.IsNullOrEmpty(searchString))
               {
                   FilterSearch();
               }
           }
           //If List is not empty fill dropdown and add Create button
           if (searchableTypesCachedAsStringsForDropDown != null && searchableTypesCachedAsStringsForDropDown.Length>0)
           {
               selectionIndexForDropDown = EditorGUILayout.Popup(selectionIndexForDropDown, searchableTypesCachedAsStringsForDropDown);
   
               //If Pressed on Button
               if (GUILayout.Button("Create"))
               {                  
                   //Return string by index from filtered array then type from string from dictionary
                   Type type = SearchableTypes[searchableTypesCachedAsStringsForDropDown[selectionIndexForDropDown]];

                   //Get Active Folder in Editor
                   var activeFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,null).ToString();
                  
                   //If Invalid Folder 
                   if (!AssetDatabase.IsValidFolder(activeFolderPath))
                   {
                      Debug.LogError($"Can not create Scriptable Asset in selected folder with path :{activeFolderPath} , folder is outside unity assets directory");
                      return;
                   }
   
                   //If such asset was already created , don't overwrite , create new with higher number
                   var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{activeFolderPath}/{type.Name}.asset");
                   UnityEditor.AssetDatabase.CreateAsset(CreateInstance(type), assetPath);
   
                   //highlight newly created object in Editor
                   EditorGUIUtility.PingObject(UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, type));   
               }
           }
       }
   
       private void FilterSearch()
       {
           searchableTypesCachedAsStringsForDropDown =
               (from t in SearchableTypes
                where t.Key.Contains(searchString, StringComparison.InvariantCultureIgnoreCase)
                select t.Key)
               .ToArray();
       }
   
       private void LoadTypes()
       {
           //============== Filling & Filtering Known Non Engine Types
   
           var allTypes =
               (from assembly in AppDomain.CurrentDomain.GetAssemblies()
               where ExcludedNamespaces.All(excludedNameSpace=>!assembly.FullName.StartsWith(excludedNameSpace))
               select assembly.GetTypes()).
               SelectMany(x => x);
   
           var filteredTypes = from t in allTypes
               where typeof(ScriptableObject).IsAssignableFrom(t) && !typeof(Editor).IsAssignableFrom(t) && !typeof(EditorWindow).IsAssignableFrom(t) && !t.IsAbstract && 
                     (t.Namespace==null ||ExcludedNamespaces.All(excludedNameSpace=>!t.Namespace.StartsWith(excludedNameSpace)))
               select t;
   
   
           foreach (var type in filteredTypes)
           {
               if (!SearchableTypes.ContainsKey(type.FullName))
               {
                   SearchableTypes.Add(type.FullName, type);
               }
           }
       }
   
   } 
}

