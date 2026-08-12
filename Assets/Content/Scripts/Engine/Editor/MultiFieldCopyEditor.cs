using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

/// <summary>
/// MultiFieldCopy
///
/// For regular component fields:
///   Right-click any field in Inspector → "Copy Multiple" / "Paste Multiple"
///
/// For GameObject Name:
///   Right-click in the Hierarchy → "Multi Copy/Copy Names"
///   Then select targets → Hierarchy right-click → "Multi Copy/Paste Names → N objects"
///   OR paste strategies available under "Multi Copy/Paste Names/"
/// </summary>

// ══════════════════════════════════════════════════════════════════════════════
//  SHARED CLIPBOARD
// ══════════════════════════════════════════════════════════════════════════════

namespace GameCoreEditor
{
    public static class MFC_Clipboard
    {
        public const string NAME_SENTINEL = "__go_name__";
        public static System.Type ComponentType;
        public static string FieldPath;
        public static string DisplayName;
        public static SerializedPropertyType PropertyType = SerializedPropertyType.Generic;
        public static List<string> SourceNames = new List<string>();
        public static List<object> Values = new List<object>();
        public static bool HasData => FieldPath != null && Values.Count > 0;
        public static bool IsNameField => FieldPath == NAME_SENTINEL;

        public static void Store(System.Type type, string path, string display,
                                 SerializedPropertyType propType,
                                 string[] srcNames, object[] values)
        {
            ComponentType = type;
            FieldPath = path;
            DisplayName = display;
            PropertyType = propType;
            SourceNames = srcNames.ToList();
            Values = values.ToList();
        }

        // Overload without propType for backwards-compat (Name field, etc.)
        public static void Store(System.Type type, string path, string display,
                                 string[] srcNames, object[] values)
        {
            Store(type, path, display, SerializedPropertyType.Generic, srcNames, values);
        }

        public static void Clear()
        {
            ComponentType = null; FieldPath = null; DisplayName = null;
            PropertyType = SerializedPropertyType.Generic;
            SourceNames.Clear(); Values.Clear();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    //  INSPECTOR HOOK — contextualPropertyMenu for all component fields
    // ══════════════════════════════════════════════════════════════════════════════
    [InitializeOnLoad]
    public static class MFC_InspectorHook
    {
        static MFC_InspectorHook()
        {
            EditorApplication.contextualPropertyMenu += OnFieldRightClick;
        }

        private static void OnFieldRightClick(GenericMenu menu, SerializedProperty prop)
        {
            string path = prop.propertyPath;
            string display = ObjectNames.NicifyVariableName(prop.name);
            System.Type compType = prop.serializedObject.targetObject.GetType();
            Object[] targets = prop.serializedObject.targetObjects;

            MFC_Logic.AddMenuItems(menu, path, display, compType, targets);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    //  ODIN INSPECTOR HOOK — IDefinesGenericMenuItems for all Odin-drawn fields
    // ══════════════════════════════════════════════════════════════════════════════
#if ODIN_INSPECTOR
    public class MFC_OdinDrawer : OdinDrawer, IDefinesGenericMenuItems
    {
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu menu)
        {
            // Only handle leaf value properties (not groups/categories)
            if (property.Info.PropertyType != PropertyType.Value) return;

            // Resolve Unity serialized path — may be null for Odin-only serialized fields
            string unityPath = property.UnityPropertyPath;
            if (string.IsNullOrEmpty(unityPath)) return;

            string display = property.NiceName;
            var compType = property.Info.TypeOfOwner;

            // Gather target Object[] from the property tree's weak targets (cast to Object)
            var weakTargets = property.Tree.WeakTargets;
            var targets = new Object[weakTargets.Count];
            for (int i = 0; i < weakTargets.Count; i++)
                targets[i] = weakTargets[i] as Object;

            // Filter out nulls (e.g. non-Unity-Object targets such as plain C# objects)
            targets = targets.Where(t => t != null).ToArray();
            if (targets.Length == 0) return;

            MFC_Logic.AddMenuItems(menu, unityPath, display, compType, targets);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            CallNextDrawer(label);
        }
    }
#endif

    // ══════════════════════════════════════════════════════════════════════════════
    //  HIERARCHY RIGHT-CLICK MENU — for GameObject Name field
    // ══════════════════════════════════════════════════════════════════════════════
    public static class MFC_HierarchyMenu
    {
        // ── Copy Names ────────────────────────────────────────────────────────────
        [MenuItem("GameObject/Multi Copy/Copy Names", false, 49)]
        private static void CopyNames()
        {
            GameObject[] selected = Selection.gameObjects
                .OrderBy(g => MFC_Logic.GetHierarchyPath(g.transform)).ToArray();
            if (selected.Length == 0) return;

            MFC_Clipboard.Store(
                typeof(GameObject),
                MFC_Clipboard.NAME_SENTINEL,
                "Name",
                selected.Select(g => g.name).ToArray(),
                selected.Select(g => (object)g.name).ToArray()
            );

            string preview = string.Join(", ", selected.Take(4).Select(g => g.name));
            if (selected.Length > 4) preview += "…";

            MFC_Logic.Notify($"Copied Name from {selected.Length} object(s): {preview}");
            Debug.Log($"[MultiFieldCopy] Copied Name from {selected.Length} object(s): [{preview}]");
        }

        [MenuItem("GameObject/Multi Copy/Copy Names", true)]
        private static bool CopyNamesValidate() => Selection.gameObjects.Length > 0;

        // ── Paste Names — Ordered ─────────────────────────────────────────────────
        [MenuItem("GameObject/Multi Copy/Paste Names/Ordered", false, 50)]
        private static void PasteNamesOrdered()
        {
            GameObject[] targets = Selection.gameObjects
                .OrderBy(g => MFC_Logic.GetHierarchyPath(g.transform)).ToArray();
            int count = Mathf.Min(targets.Length, MFC_Clipboard.Values.Count);
            for (int i = 0; i < count; i++)
                MFC_Logic.ApplyName(targets[i], (string)MFC_Clipboard.Values[i]);
            string msg = $"Pasted Name ordered → {count} object(s)";
            MFC_Logic.Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }
        [MenuItem("GameObject/Multi Copy/Paste Names/Ordered", true)]
        private static bool PasteNamesOrderedValidate() => MFC_Clipboard.HasData && MFC_Clipboard.IsNameField;

        // ── Paste Names — Ordered Reversed ───────────────────────────────────────
        [MenuItem("GameObject/Multi Copy/Paste Names/Ordered Reverse", false, 51)]
        private static void PasteNamesOrderedReversed()
        {
            GameObject[] targets = Selection.gameObjects
                .OrderBy(g => MFC_Logic.GetHierarchyPath(g.transform)).ToArray();
            var values = MFC_Clipboard.Values.AsEnumerable().Reverse().ToList();
            int count = Mathf.Min(targets.Length, values.Count);
            for (int i = 0; i < count; i++)
                MFC_Logic.ApplyName(targets[i], (string)values[i]);
            string msg = $"Pasted Name ordered reversed → {count} object(s)";
            MFC_Logic.Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }
        [MenuItem("GameObject/Multi Copy/Paste Names/Ordered Reverse", true)]
        private static bool PasteNamesOrderedReversedValidate() => MFC_Clipboard.HasData && MFC_Clipboard.IsNameField;

        // ── Paste Names — Cycled ──────────────────────────────────────────────────
        [MenuItem("GameObject/Multi Copy/Paste Names/Cycled", false, 52)]
        private static void PasteNamesCycle()
        {
            GameObject[] targets = Selection.gameObjects
                .OrderBy(g => MFC_Logic.GetHierarchyPath(g.transform)).ToArray();
            int clip = MFC_Clipboard.Values.Count;
            for (int i = 0; i < targets.Length; i++)
                MFC_Logic.ApplyName(targets[i], (string)MFC_Clipboard.Values[i % clip]);
            string msg = $"Pasted Name cycled → {targets.Length} object(s)";
            MFC_Logic.Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }
        [MenuItem("GameObject/Multi Copy/Paste Names/Cycled", true)]
        private static bool PasteNamesCycleValidate() => MFC_Clipboard.HasData && MFC_Clipboard.IsNameField;

        // ── Paste Names — Cycled Reversed ─────────────────────────────────────────
        [MenuItem("GameObject/Multi Copy/Paste Names/Cycled Reversed", false, 53)]
        private static void PasteNamesCycleReversed()
        {
            GameObject[] targets = Selection.gameObjects
                .OrderBy(g => MFC_Logic.GetHierarchyPath(g.transform)).ToArray();
            var values = MFC_Clipboard.Values.AsEnumerable().Reverse().ToList();
            int clip = values.Count;
            for (int i = 0; i < targets.Length; i++)
                MFC_Logic.ApplyName(targets[i], (string)values[i % clip]);
            string msg = $"Pasted Name cycled reversed → {targets.Length} object(s)";
            MFC_Logic.Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }
        [MenuItem("GameObject/Multi Copy/Paste Names/Cycled Reversed", true)]
        private static bool PasteNamesCycleReversedValidate() => MFC_Clipboard.HasData && MFC_Clipboard.IsNameField;

        // ── Clear ─────────────────────────────────────────────────────────────────
        [MenuItem("GameObject/Multi Copy/Clear Clipboard", false, 70)]
        private static void ClearClipboard()
        {
            MFC_Clipboard.Clear();
            MFC_Logic.Notify("Clipboard cleared");
        }

        [MenuItem("GameObject/Multi Copy/Clear Clipboard", true)]
        private static bool ClearClipboardValidate() => MFC_Clipboard.HasData;
    }

    // ══════════════════════════════════════════════════════════════════════════════
    //  SHARED LOGIC — menu building, copy, paste, apply
    // ══════════════════════════════════════════════════════════════════════════════
    public static class MFC_Logic
    {
        // ── Build and inject items into an existing GenericMenu ───────────────────
        public static void AddMenuItems(GenericMenu menu, string path, string display,
                                        System.Type compType, Object[] targets)
        {
            int selCount = targets.Length;

            // ── Copy Multiple ────────────────────────────────────────────────────
            menu.AddItem(
                new GUIContent($"Copy Multiple  ({selCount} selected)"),
                false,
                () => ExecuteCopy(path, display, compType, targets));

            menu.AddSeparator("");

            // ── Paste Multiple ───────────────────────────────────────────────────
            bool sameField = MFC_Clipboard.HasData
                          && MFC_Clipboard.FieldPath == path
                          && MFC_Clipboard.ComponentType == compType;

            // Cross-component / cross-field: clipboard value type is assignable to this field's type.
            // Field names do NOT need to match — only SerializedPropertyType must be compatible.
            bool crossPaste = !sameField
                           && MFC_Clipboard.HasData
                           && !MFC_Clipboard.IsNameField
                           && IsTypeCompatible(path, targets);

            if (sameField || crossPaste)
            {
                int clip = MFC_Clipboard.Values.Count;
                int tgt = targets.Length;
                string sourcePreview = string.Join(", ", MFC_Clipboard.SourceNames.Take(3))
                                     + (MFC_Clipboard.SourceNames.Count > 3 ? "…" : "");

                string pasteLabel = crossPaste
                    ? $"Paste Multiple  [{MFC_Clipboard.ComponentType?.Name}.{MFC_Clipboard.DisplayName} → {compType.Name}.{display}]"
                    : "Paste Multiple";

                menu.AddItem(
                    new GUIContent($"{pasteLabel}/Ordered  ({Mathf.Min(tgt, clip)} objects  |  from: {sourcePreview})"),
                    false,
                    () => PasteOrdered(path, display, targets));

                menu.AddItem(
                    new GUIContent($"{pasteLabel}/Cycled  ({clip} values over {tgt} objects)"),
                    false,
                    () => PasteCycle(path, display, targets));

                menu.AddItem(
                    new GUIContent($"{pasteLabel}/Ordered Reverse  ({Mathf.Min(tgt, clip)} objects  |  from: {sourcePreview})"),
                    false,
                    () => PasteOrderedReversed(path, display, targets));

                menu.AddItem(
                    new GUIContent($"{pasteLabel}/Cycled Reverse  ({clip} values over {tgt} objects)"),
                    false,
                    () => PasteCycleReversed(path, display, targets));

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Clear MultiFieldCopy Clipboard"), false, () =>
                {
                    MFC_Clipboard.Clear(); Notify("Clipboard cleared");
                });
            }
            else if (MFC_Clipboard.HasData)
            {
                menu.AddDisabledItem(new GUIContent(
                    $"Paste Multiple  (clipboard type \"{MFC_Clipboard.PropertyType}\" not compatible with \"{display}\" [{compType.Name}])"));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste Multiple  (nothing copied yet)"));
            }
        }

        // ── Type compatibility: does the clipboard's SerializedPropertyType match this field? ──
        // Checks the actual field on one of the target objects to get its real property type.
        private static bool IsTypeCompatible(string path, Object[] targets)
        {
            if (MFC_Clipboard.PropertyType == SerializedPropertyType.Generic) return false;
            foreach (var target in targets)
            {
                var so = new SerializedObject(target);
                so.Update();
                SerializedProperty sp = so.FindProperty(path);
                if (sp == null) continue;
                if (sp.propertyType == MFC_Clipboard.PropertyType) return true;
            }
            return false;
        }

        // ── Copy: read value from every selected object ───────────────────────────
        public static void ExecuteCopy(string path, string display,
                                       System.Type compType, Object[] sources)
        {
            sources = SortTargets(sources);
            var names = new string[sources.Length];
            var values = new object[sources.Length];
            var propType = SerializedPropertyType.Generic;

            for (int i = 0; i < sources.Length; i++)
            {
                var so = new SerializedObject(sources[i]);
                so.Update();
                SerializedProperty sp = so.FindProperty(path);
                values[i] = sp != null ? GetValue(sp) : null;
                names[i] = sources[i] is Component c ? c.gameObject.name : sources[i].name;
                if (i == 0 && sp != null) propType = sp.propertyType;
            }

            MFC_Clipboard.Store(compType, path, display, propType, names, values);

            string preview = string.Join(", ", values.Take(4).Select(v => v?.ToString() ?? "null"));
            if (values.Length > 4) preview += "…";
            Notify($"Copied \"{display}\" from {sources.Length} object(s)");
            Debug.Log($"[MultiFieldCopy] Copied [{compType.Name}].\"{display}\" values: [{preview}]");
        }

        // ── Sort helpers ──────────────────────────────────────────────────────────
        private static Object[] SortTargets(Object[] targets)
        {
            return targets.OrderBy(t =>
            {
                if (t is Component comp) return GetHierarchyPath(comp.transform);
                if (t is GameObject go) return GetHierarchyPath(go.transform);
                return t.name;
            }).ToArray();
        }

        public static string GetHierarchyPath(Transform tr)
        {
            var parts = new List<string>();
            while (tr != null)
            {
                parts.Insert(0, tr.GetSiblingIndex().ToString("D6"));
                tr = tr.parent;
            }
            return string.Join("/", parts);
        }

        // ── Paste strategies ──────────────────────────────────────────────────────
        private static void PasteOrdered(string path, string display, Object[] targets)
        {
            targets = SortTargets(targets);
            int count = Mathf.Min(targets.Length, MFC_Clipboard.Values.Count);
            for (int i = 0; i < count; i++)
                ApplyValue(targets[i], path, MFC_Clipboard.Values[i], display);
            string msg = $"Pasted \"{display}\" ordered → {count} object(s)";
            Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }

        private static void PasteOrderedReversed(string path, string display, Object[] targets)
        {
            targets = SortTargets(targets);
            var values = MFC_Clipboard.Values.AsEnumerable().Reverse().ToList();
            int count = Mathf.Min(targets.Length, values.Count);
            for (int i = 0; i < count; i++)
                ApplyValue(targets[i], path, values[i], display);
            string msg = $"Pasted \"{display}\" ordered reversed → {count} object(s)";
            Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }

        private static void PasteCycle(string path, string display, Object[] targets)
        {
            targets = SortTargets(targets);
            int clip = MFC_Clipboard.Values.Count;
            for (int i = 0; i < targets.Length; i++)
                ApplyValue(targets[i], path, MFC_Clipboard.Values[i % clip], display);
            string msg = $"Pasted \"{display}\" cycled → {targets.Length} object(s)";
            Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }

        private static void PasteCycleReversed(string path, string display, Object[] targets)
        {
            targets = SortTargets(targets);
            var values = MFC_Clipboard.Values.AsEnumerable().Reverse().ToList();
            int clip = values.Count;
            for (int i = 0; i < targets.Length; i++)
                ApplyValue(targets[i], path, values[i % clip], display);
            string msg = $"Pasted \"{display}\" cycled reversed → {targets.Length} object(s)";
            Notify(msg); Debug.Log($"[MultiFieldCopy] {msg}");
        }

        // ── Apply one value to one object ─────────────────────────────────────────
        private static void ApplyValue(Object target, string path, object value, string display)
        {
            Undo.RecordObject(target, $"Paste \"{display}\"");
            var so = new SerializedObject(target);
            so.Update();
            SerializedProperty sp = so.FindProperty(path);
            if (sp != null) { SetValue(sp, value); so.ApplyModifiedProperties(); }
            EditorUtility.SetDirty(target);
        }

        public static void ApplyName(GameObject go, string newName)
        {
            Undo.RecordObject(go, "Paste Name");
            go.name = newName;
            EditorUtility.SetDirty(go);
        }

        // ── Notification ──────────────────────────────────────────────────────────
        public static void Notify(string msg)
        {
            EditorWindow w = EditorWindow.focusedWindow;
            if (w != null) w.ShowNotification(new GUIContent(msg), 2.5);
        }

        // ── SerializedProperty get/set ────────────────────────────────────────────
        public static object GetValue(SerializedProperty sp)
        {
            switch (sp.propertyType)
            {
                case SerializedPropertyType.Integer: return sp.intValue;
                case SerializedPropertyType.Boolean: return sp.boolValue;
                case SerializedPropertyType.Float: return sp.floatValue;
                case SerializedPropertyType.String: return sp.stringValue;
                case SerializedPropertyType.Color: return sp.colorValue;
                case SerializedPropertyType.Vector2: return sp.vector2Value;
                case SerializedPropertyType.Vector3: return sp.vector3Value;
                case SerializedPropertyType.Vector4: return sp.vector4Value;
                case SerializedPropertyType.Rect: return sp.rectValue;
                case SerializedPropertyType.Bounds: return sp.boundsValue;
                case SerializedPropertyType.Quaternion: return sp.quaternionValue;
                case SerializedPropertyType.Vector2Int: return sp.vector2IntValue;
                case SerializedPropertyType.Vector3Int: return sp.vector3IntValue;
                case SerializedPropertyType.Enum: return sp.enumValueIndex;
                case SerializedPropertyType.ObjectReference: return sp.objectReferenceValue;
                case SerializedPropertyType.LayerMask: return sp.intValue;
                case SerializedPropertyType.AnimationCurve: return sp.animationCurveValue;
                default: return null;
            }
        }

        public static void SetValue(SerializedProperty sp, object value)
        {
            if (value == null) return;
            try
            {
                switch (sp.propertyType)
                {
                    case SerializedPropertyType.Integer: sp.intValue = (int)value; break;
                    case SerializedPropertyType.Boolean: sp.boolValue = (bool)value; break;
                    case SerializedPropertyType.Float: sp.floatValue = (float)value; break;
                    case SerializedPropertyType.String: sp.stringValue = (string)value; break;
                    case SerializedPropertyType.Color: sp.colorValue = (Color)value; break;
                    case SerializedPropertyType.Vector2: sp.vector2Value = (Vector2)value; break;
                    case SerializedPropertyType.Vector3: sp.vector3Value = (Vector3)value; break;
                    case SerializedPropertyType.Vector4: sp.vector4Value = (Vector4)value; break;
                    case SerializedPropertyType.Rect: sp.rectValue = (Rect)value; break;
                    case SerializedPropertyType.Bounds: sp.boundsValue = (Bounds)value; break;
                    case SerializedPropertyType.Quaternion: sp.quaternionValue = (Quaternion)value; break;
                    case SerializedPropertyType.Vector2Int: sp.vector2IntValue = (Vector2Int)value; break;
                    case SerializedPropertyType.Vector3Int: sp.vector3IntValue = (Vector3Int)value; break;
                    case SerializedPropertyType.Enum: sp.enumValueIndex = (int)value; break;
                    case SerializedPropertyType.ObjectReference: sp.objectReferenceValue = (Object)value; break;
                    case SerializedPropertyType.LayerMask: sp.intValue = (int)value; break;
                    case SerializedPropertyType.AnimationCurve: sp.animationCurveValue = (AnimationCurve)value; break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MultiFieldCopy] Could not set '{sp.name}': {ex.Message}");
            }
        }
    }
}