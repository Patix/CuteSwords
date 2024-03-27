//Patik

using System;
using System.IO;
using System.Threading.Tasks;
using Patik.Utilities.File;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Patik.CodeArchitecture.Patterns
{
    /// <summary>
    /// Extending Child Classes Automatically Acquire Singleton Functionality (Automatic Instance Management)
    ///<para>by default "<see cref="DontDestroyFlag"/>" returns False, You can override (True) - to persist behaviour between scenes</para>
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        /// <summary>
        /// [True] Singleton Persists Amongst Scenes and Must Be Root Object (OnCreationProcess Called Once)
        /// <para>[False] Singleton Gets Destroyed and Recreated During Scene Loadings (OnCreationProcess Multiple Times)</para>
        /// </summary>
        protected virtual bool DontDestroyFlag=>false;
        
        private static T    instance;
        
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                
                //In Case It's attached to any GameObject as Pre- created MonoBehaviour 
                var sceneRootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                for (var i = 0; i < sceneRootObjects.Length; i++) {

                    var found = sceneRootObjects[i].GetComponentInChildren<T>(true);
                    if (found != default(T))
                    {
                        instance = found;
                        break;
                    }
                }

                //In Case it's not attached anywhere and we need to Create GameObject Automatically
                if (instance == null)
                {
                    var createdGameObject = new GameObject();
                    instance               = createdGameObject.AddComponent<T>();
                    createdGameObject.name = $"Singleton<{instance.GetType().Name}>";
                }

                if(instance.DontDestroyFlag) DontDestroyOnLoad(instance.gameObject);
                instance?.OnCreationProcess();
                return instance;
            }

            set => instance = value;
        }
        
        /// <summary>
        /// Constructor Like Method Called on "instance" field during creation. Called before 'instance' is assigned to Instance property , so use "this." instead of directly accessing "Instance" property
        /// </summary>
        protected virtual void OnCreationProcess() { }

        //if instance is Enabled before it's Accessed , Look Up in scene Objects
        protected virtual void Awake()
        {
            if (instance == this) return;
            
            //If 2 Instances Exist (Primarily During Scene Changes , or by mistake)
            if (instance != null)
            {
                //IF Instances Have DontDestroyOnLoad => Disallow Second Instance to Exist 
                if (DontDestroyFlag)
                {
                    //Instance already exists but this is not instance (instance!=this && instance!=null) , so delete this
                    Destroy(this);
                }
                
                //IF Instances Don't Have DontDestroyOnLoad -> second instance probably got created when we switched to new scene (means old get deleted) 
                //so we assign new instance from new scene
                else
                {
                    instance = this as T;
                    instance?.OnCreationProcess();
                }
            }
            
            //If Instance Doesn't Exist at all , just assign it 
            else
            {
                instance = this as T;
                if(instance.DontDestroyFlag) DontDestroyOnLoad(instance);
                instance?.OnCreationProcess();
                
                if(DontDestroyFlag)
                    DontDestroyOnLoad(instance);
            }

        }
    }


    /// <summary>
    /// This type of Singleton is Loaded from Scriptable Object . If ScriptableObject does not exist  , it will be created automatically in Resources Folder [When playing In Editor] .
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static string assetAddress => "SingletonScriptableObjects/" + typeof(T).Name;

        private static T instance;
        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    var originalAsset = Resources.Load<T>(assetAddress);
#if UNITY_EDITOR
                    if (originalAsset == null)
                    {
                        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources" + assetAddress))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                        }

                        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources/SingletonScriptableObjects"))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "SingletonScriptableObjects");
                        }

                        UnityEditor.AssetDatabase.CreateAsset(CreateInstance<T>(), "Assets/Resources/" + assetAddress + ".asset");
                        originalAsset = Resources.Load<T>(assetAddress);
                    }
#endif
                    //Instantiate Clone while playing in editor to avoid Accidental Change of Data
                    if (Application.isPlaying)
                    {
                        instance = Instantiate(originalAsset);
                    }
                    else
                    {
                        instance = originalAsset;
                    }
                    instance.OnCreationProcess();
                }


                return instance;
            }

            set => instance = value;
        }
        /// <summary>
        /// Constructor Like Method Called on  "instance" field during creation. Called before 'instance' is assigned to Instance property , so use "this." instead of directly accessing "Instance" property
        /// </summary>/
        protected virtual void OnCreationProcess() { }

    }

    /// <summary>
    /// This type of Singleton is Loaded from File which is automatically created if missing .
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class SingletonFromFile<T> where T : SingletonFromFile<T>, new()
    {
        /// <summary>
        /// Thread Safe lock
        /// </summary>
        [NonSerialized]
        private object SyncRoot = new object();

        /// <summary>
        /// Dirty Flag , True Means file Needs to be Re saved (Set Manually)
        /// </summary>
        [NonSerialized]
        protected bool isDirty;

        /// <summary>
        /// <para>Format which is used by class for Serialization. default is <see cref="FileReaderWriter.FileFormat.Json"/>.</para>
        /// <para>Use <see cref="SerializationInstructionsAttribute"/> on class To Change Format and Encryption</para>
        /// </summary>
        protected static FileReaderWriter.FileFormat Format
        {
            get
            {
                var foundSerializationInstructionAttributes = typeof(T).GetCustomAttributes(typeof(SerializationInstructionsAttribute), true);
                if (foundSerializationInstructionAttributes.Length > 0)
                {
                    return ((SerializationInstructionsAttribute)foundSerializationInstructionAttributes[0]).Format;
                }
                return default;
            }
        }
        /// <summary>
        /// <para> Encrypter for Class file . Default = Unencrypted </para>
        /// <para>Use <see cref="SerializationInstructionsAttribute"/> on class To Change Format and Encryption</para>
        /// </summary>
        /// <returns></returns>
        protected static FileReaderWriter.EncryptionInfo Encryption
        {
            get
            {
                var foundSerializationInstructionAttributes = typeof(T).GetCustomAttributes(typeof(SerializationInstructionsAttribute), true);
                if (foundSerializationInstructionAttributes.Length > 0)
                {
                    return ((SerializationInstructionsAttribute)foundSerializationInstructionAttributes[0]).EncryptionInfo;
                }
                return default;
            }
        }

        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!File.Exists(FileReaderWriter.PathFor.ClassSingletonFile<T>(Format)))
                    {
                        instance = new T();
                        instance.OnBeforeSavingToFile(false,false);
                        WriteTo.GenericFile.AsClassSingleton(instance, Format, Encryption);
                    }
                    
                    instance = ReadFrom.GenericFile.ClassSingleton<T>(Format, Encryption);
                    instance.SyncRoot=new object();
                    instance.OnAfterReadFromFile(instance);
                }

                return instance;
            }

            set => instance = value;
        }

        /// <summary>
        /// Flags Object Dirty , Means it needs to be re-saved
        /// </summary>
        public static void SetDirty()
        {
            Instance.isDirty = true;
        }

        /// <summary>
        /// Called after file is read from file .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inst"></param>
        protected virtual void OnAfterReadFromFile(T inst)
        {

        }

        /// <summary>
        /// Called before saving on disk
        /// </summary>
        /// <param name="classFileAlreadyExists">indicates if class file already exists on disk</param>
        /// <param name="isSavedUsingExternalThread">indicates whether file is getting saved using external thread</param>
        protected virtual void OnBeforeSavingToFile(bool classFileAlreadyExists, bool isSavedUsingExternalThread)
        {

        }

        /// <summary>
        /// Saves Changes To Disk and Clears Dirty Flag
        /// </summary>
        public static void Save()
        {  
            Instance.OnBeforeSavingToFile(true,false);
            WriteTo.GenericFile.AsClassSingleton(Instance, Format, Encryption);
            Instance.isDirty = false;
        }
        /// <summary>
        /// Saves Changes to Disk and Clears Dirty Flag (Using External Thread)
        /// </summary>
        public static async Task SaveAsync()
        {
            var threadSafePath = FileReaderWriter.PathFor.ClassSingletonFile<T>(Format);
            await Task.Run(() =>
            {
                lock (Instance.SyncRoot)
                {
                    Instance.OnBeforeSavingToFile(true, true);
                    WriteTo.GenericFile.AtAdress(threadSafePath,Instance, Format, Encryption);
                    Instance.isDirty = false;
                }
            });
        }


        /// <summary>
        /// Resets File . Writing Default values for class
        /// </summary>
        public static void Reset()
        {
            instance = new T();
            instance.OnBeforeSavingToFile(false,false);
            WriteTo.GenericFile.AsClassSingleton(instance, Format, Encryption);
        }
    }


    /// <summary>
    /// Extending Child Classes Automatically Acquire Singleton Functionality ( "Automatic Instance Management" <see cref="Singleton{T}.Instance"/> )
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    [Serializable]
    public abstract class Singleton<T> where T :Singleton<T> ,new()
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }

            set => instance = value;
        }
    }
}
