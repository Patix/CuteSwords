using System;
using System.Collections.Generic;
using System.Reflection;
using InventoryAndEquipment;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace Io.Tests
{
    public static class StaticScriptExecuter
    {
        #if UNITY_EDITOR
        [Button]
        public static void Execute(Transform[] transforms)
        {
            foreach (var transform in transforms)
            {
                transform.name = transform.name.Remove(0,1).ToTitleCase();
            }
        }
        #endif
        
    }
}