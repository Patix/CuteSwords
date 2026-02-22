#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editors
{
    internal static class VoxelPaintUtil
    {
        internal const string DefaultRoot = "Terrain";
        internal const float TopDot = 0.95f; // ~18° from up
        internal const float SideDot = 0.7f;  // ~45° from axis

        internal static void TrySelectAtMouse(Vector2 guiPos, string rootName, ref int lastOwnerIndex)
        {
            var ray = HandleUtility.GUIPointToWorldRay(guiPos);
            // 3D
            if (Physics.Raycast(ray, out var h3d, Mathf.Infinity))
            {
                var n = h3d.normal;
                if (IsTop(n) || IsSide(n)) { HandleHit(h3d.collider ? h3d.collider.transform : null, n, rootName, ref lastOwnerIndex); return; }
                lastOwnerIndex = -1; return;
            }
            // 2D
            var h2d = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
            if (h2d.collider)
            {
                var n = new Vector3(h2d.normal.x, h2d.normal.y, 0f).normalized;
                if (IsTop(n) || IsSide(n)) { HandleHit(h2d.collider.transform, n, rootName, ref lastOwnerIndex); return; }
                lastOwnerIndex = -1; return;
            }
            lastOwnerIndex = -1;
        }

        private static bool IsTop(Vector3 normal)
        {
            if (normal.sqrMagnitude < 1e-6f) return false;
            return Vector3.Dot(normal.normalized, Vector3.up) >= TopDot;
        }
        private static bool IsSide(Vector3 normal)
        {
            if (normal.sqrMagnitude < 1e-6f) return false;
            var n = normal.normalized;
            if (Vector3.Dot(n, Vector3.up) >= TopDot) return false;
            return Vector3.Dot(n, Vector3.right) >= SideDot || Vector3.Dot(n, Vector3.forward) >= SideDot || Vector3.Dot(n, Vector3.back) >= SideDot;
        }

        private static void HandleHit(Transform hitTr, Vector3 hitNormal, string rootName, ref int lastOwnerIndex)
        {
            if (!hitTr) { lastOwnerIndex = -1; return; }
            var root = FindRoot(rootName);
            if (!root) { lastOwnerIndex = -1; return; }
            if (!IsUnderRoot(hitTr, root)) { lastOwnerIndex = -1; return; }
            var layers = GetGroundLayers(root);
            if (layers.Count == 0) { lastOwnerIndex = -1; return; }

            int cur = GetOwningLayerIndex(hitTr, root, layers);
            if (cur < 0) { lastOwnerIndex = -1; return; }
            if (cur == lastOwnerIndex) return; // prevent cycling when hovering same child
            lastOwnerIndex = cur;

            int target = -1;
            if (IsTop(hitNormal)) target = Next(cur, layers.Count);
            else if (IsSide(hitNormal)) target = cur; // select current owning layer on side hit
            if (target < 0 || target >= layers.Count) return;

            var nextGO = layers[target] ? layers[target].gameObject : null;
            if (!nextGO) return;
            Selection.activeGameObject = nextGO;
            EditorGUIUtility.PingObject(nextGO);
        }

        private static Transform FindRoot(string rootName)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return null;
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++) { var go = roots[i]; if (go && go.name == rootName) return go.transform; }
            return null;
        }
        private static bool IsUnderRoot(Transform t, Transform root)
        {
            var cur = t; while (cur) { if (cur == root) return true; cur = cur.parent; } return false;
        }
        private static List<Transform> GetGroundLayers(Transform root)
        {
            var list = new List<Transform>(); if (!root) return list;
            for (int i = 0, c = root.childCount; i < c; i++) { var ch = root.GetChild(i); if (ch && ch.name.StartsWith("Ground Layer")) list.Add(ch); }
            return list;
        }
        private static int GetOwningLayerIndex(Transform t, Transform root, List<Transform> layers)
        {
            Transform cur = t, direct = null; while (cur && cur != root) { direct = cur; cur = cur.parent; }
            if (cur != root || !direct) return -1;
            for (int i = 0; i < layers.Count; i++) if (layers[i] == direct) return i; return -1;
        }
        private static int Next(int cur, int count) { if (count <= 0) return -1; if (cur < 0 || cur >= count) return 0; return (cur + 1) % count; }
    }

    [EditorTool("Voxel Prefab Paint")]
    public class VoxelPrefabPaintTool : EditorTool
    {
        [SerializeField] private string _groundLayerRootName = VoxelPaintUtil.DefaultRoot;
        private int _lastOwnerIndex = -1;
        private string RootName => string.IsNullOrEmpty(_groundLayerRootName) ? VoxelPaintUtil.DefaultRoot : _groundLayerRootName;
        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView)) return; var e = Event.current; if (e == null) return;
            VoxelPaintUtil.TrySelectAtMouse(e.mousePosition, RootName, ref _lastOwnerIndex);
        }
    }

    [InitializeOnLoad]
    public static class VoxelPrefabPaintAutoSelector
    {
        private const string PrefsKey = "VoxelPrefabPaint_AutoEnabled";
        private static bool _enabled;
        private static int _lastOwnerIndex;
        private static Vector2 _lastMousePos; private static bool _hasMousePos;
        private static readonly string _rootName = VoxelPaintUtil.DefaultRoot;

        static VoxelPrefabPaintAutoSelector() { _enabled = EditorPrefs.GetBool(PrefsKey, false); UpdateSubs(); }
        [MenuItem("Tools/Voxel Prefab Paint/Auto Select Toggle")] private static void Toggle() { _enabled = !_enabled; EditorPrefs.SetBool(PrefsKey, _enabled); UpdateSubs(); }
        [MenuItem("Tools/Voxel Prefab Paint/Auto Select Toggle", true)] private static bool ValidateToggle() { Menu.SetChecked("Tools/Voxel Prefab Paint/Auto Select Toggle", _enabled); return true; }
        private static void UpdateSubs() { SceneView.duringSceneGui -= OnSceneGUI; EditorApplication.update -= OnUpdate; if (_enabled) { SceneView.duringSceneGui += OnSceneGUI; EditorApplication.update += OnUpdate; } _lastOwnerIndex = -1; _hasMousePos = false; }
        private static void OnSceneGUI(SceneView sv) { var e = Event.current; if (e == null) return; _lastMousePos = e.mousePosition; _hasMousePos = true; VoxelPaintUtil.TrySelectAtMouse(_lastMousePos, _rootName, ref _lastOwnerIndex); }
        private static void OnUpdate() { if (!_enabled) return; var sv = SceneView.lastActiveSceneView; if (!sv) return; if (!_hasMousePos) return; VoxelPaintUtil.TrySelectAtMouse(_lastMousePos, _rootName, ref _lastOwnerIndex); }
    }

    [InitializeOnLoad]
    public static class VoxelPrefabPaintShortcut
    {
        private static bool _zHeld; 
        private static readonly System.Type ToolType = typeof(VoxelPrefabPaintTool);
        static VoxelPrefabPaintShortcut() { SceneView.duringSceneGui -= OnSceneGUI; SceneView.duringSceneGui += OnSceneGUI; }
        private static void OnSceneGUI(SceneView sv)
        {
            var e = Event.current; if (e == null) return;
            if (e.type      == EventType.KeyDown && e.keyCode == KeyCode.Alpha1) { if (!_zHeld) { _zHeld = true; ToolManager.SetActiveTool(ToolType); e.Use(); } }
            else if (e.type == EventType.KeyUp   && e.keyCode == KeyCode.Alpha1) { if (_zHeld) { _zHeld  = false; ToolManager.RestorePreviousTool(); e.Use(); } }
        }
    }
}
#endif

