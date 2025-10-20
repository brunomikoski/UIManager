using System.Collections.Generic;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.UIManager.CustomEditors
{
    [CustomEditor(typeof(WindowsManager), true)]
    public class WindowsManagerCustomEditor : Editor
    {
        [SerializeField]
        private UIWindow _selectedUIWindow;
        private SerializedObject _editorSO;

        private void OnEnable()
        {
            _editorSO = new SerializedObject(this);
        }

        private void OnDisable()
        {
            _editorSO = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
            {
                using (new EditorGUILayout.VerticalScope("Box"))
                {
                    EditorGUILayout.LabelField("Windows Utilities", EditorStyles.boldLabel);

                    if (GUILayout.Button("Organize Windows Hierarchy"))
                    {
                        OrganizeHierarchy();
                    }

                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Quick Instantiate", EditorStyles.miniBoldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (_editorSO != null)
                        {
                            _editorSO.Update();
                            EditorGUILayout.PropertyField(_editorSO.FindProperty("_selectedUIWindow"), GUIContent.none);
                            _editorSO.ApplyModifiedProperties();
                        }
                        using (new EditorGUI.DisabledGroupScope(_selectedUIWindow == null))
                        {
                            if (GUILayout.Button("Instantiate", EditorStyles.miniButton, GUILayout.Width(100)))
                            {
                                InstantiateSelectedWindow();
                            }
                        }
                    }
                }
            }
        }

        private void OrganizeHierarchy()
        {
            WindowsManager windowsManager = (WindowsManager)target;
            Transform root = windowsManager.transform;

            List<UILayer> layers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UILayer>();
            if (layers == null || layers.Count == 0)
            {
                Debug.LogWarning("No UILayers found in CollectionsRegistry. Cannot organize hierarchy.");
                return;
            }

            // Ensure layer parents exist and have correct setup
            Dictionary<UILayer, RectTransform> layerParents = new Dictionary<UILayer, RectTransform>(layers.Count);
            for (int i = 0; i < layers.Count; i++)
            {
                UILayer uiLayer = layers[i];
                RectTransform parentForLayer = EnsureLayerParent(root, uiLayer);
                layerParents[uiLayer] = parentForLayer;
                // Order layer parents by collection index
                int siblingIndex = uiLayer.Collection.IndexOf(uiLayer);
                parentForLayer.SetSiblingIndex(Mathf.Max(0, siblingIndex));
            }

            // Reparent existing windows to their respective layer parents
            WindowController[] windowControllers = root.GetComponentsInChildren<WindowController>(true);
            for (int i = 0; i < windowControllers.Length; i++)
            {
                WindowController wc = windowControllers[i];
                UIWindow uiWindow = wc.UIWindow;
                UILayer targetLayer = uiWindow != null && uiWindow.Layer != null ? uiWindow.Layer : layers[0];
                RectTransform targetParent = layerParents[targetLayer];
                if (wc.transform.parent != targetParent)
                {
                    Undo.SetTransformParent(wc.transform, targetParent, "Organize Windows Hierarchy");
                    wc.RectTransform.SetAsLastSibling();
                    EditorUtility.SetDirty(wc);
                }
            }

            EditorUtility.SetDirty(root);
        }

        private RectTransform EnsureLayerParent(Transform root, UILayer uiLayer)
        {
            Transform found = root.Find(uiLayer.name);
            RectTransform rectTransform;
            if (found == null)
            {
                GameObject layerGO = new GameObject(uiLayer.name, typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(layerGO, "Create UI Layer Parent");
                rectTransform = layerGO.transform as RectTransform;
                Undo.SetTransformParent(rectTransform, root, "Parent UI Layer");
                SetupLayerRect(rectTransform, uiLayer.name);
            }
            else
            {
                rectTransform = (RectTransform)found;
                SetupLayerRect(rectTransform, uiLayer.name);
            }

            return rectTransform;
        }

        private static void SetupLayerRect(RectTransform rectTransform, string name)
        {
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.name = name;
        }

        private void InstantiateSelectedWindow()
        {
            if (_selectedUIWindow == null)
                return;

            WindowsManager windowsManager = (WindowsManager)target;
            Transform root = windowsManager.transform;

            List<UILayer> layers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UILayer>();
            UILayer targetLayer = _selectedUIWindow.Layer != null ? _selectedUIWindow.Layer : (layers.Count > 0 ? layers[0] : null);

            RectTransform parent = targetLayer != null ? EnsureLayerParent(root, targetLayer) : (RectTransform)root;

            // Try to find an existing instance of this UIWindow in the hierarchy first
            WindowController existing = FindExistingWindow(root, _selectedUIWindow);
            if (existing != null)
            {
                // Ensure it lives under the correct layer parent
                if (existing.transform.parent != parent)
                {
                    Undo.SetTransformParent(existing.transform, parent, "Reparent UIWindow to Layer");
                }

                // Enable and bring to front
                Undo.RecordObject(existing.gameObject, "Enable UIWindow");
                existing.gameObject.SetActive(true);
                existing.RectTransform.SetAsLastSibling();

                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            // Otherwise instantiate a new prefab instance under the proper parent
            WindowController prefab = _selectedUIWindow.GetWindowPrefab();
            if (prefab == null)
            {
                Debug.LogError($"Missing WindowController Prefab for {_selectedUIWindow.name}");
                return;
            }

            Object instance = PrefabUtility.InstantiatePrefab(prefab, parent);
            GameObject go = instance as GameObject ?? (instance as Component)?.gameObject;
            if (go != null)
            {
                Undo.RegisterCreatedObjectUndo(go, "Instantiate UIWindow");
                var rect = go.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchorMax = Vector2.one;
                    rect.anchorMin = Vector2.zero;
                    rect.sizeDelta = Vector2.zero;
                    rect.anchoredPosition = Vector2.zero;
                }
                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
            }

            // Keep selection after domain reload
            // Do not clear _selectedUIWindow; dev may want to instantiate multiple
        }

        private static WindowController FindExistingWindow(Transform root, UIWindow target)
        {
            WindowController[] windowControllers = root.GetComponentsInChildren<WindowController>(true);
            for (int i = 0; i < windowControllers.Length; i++)
            {
                WindowController wc = windowControllers[i];
                if (wc != null && wc.UIWindow == target)
                    return wc;
            }
            return null;
        }
    }
}
