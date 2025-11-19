using System.Collections.Generic;
using System.Linq;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEditor;
using UnityEngine;

namespace BrunoMikoski.UIManager.CustomEditors
{
    [CustomEditor(typeof(WindowsManager), true)]
    public class WindowsManagerCustomEditor : Editor
    {
        [SerializeField]
        private UIWindow selectedUIWindow;
        private SerializedObject editorSO;
        private WindowsManager windowsManager;

        private void OnEnable()
        {
            editorSO = new SerializedObject(this);
            windowsManager = (WindowsManager)target;
        }

        private void OnDisable()
        {
            editorSO = null;
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
                        if (editorSO != null)
                        {
                            editorSO.Update();
                            EditorGUILayout.PropertyField(editorSO.FindProperty("selectedUIWindow"), GUIContent.none);
                            editorSO.ApplyModifiedProperties();
                        }
                        using (new EditorGUI.DisabledGroupScope(selectedUIWindow == null))
                        {
                            if (GUILayout.Button("Instantiate", EditorStyles.miniButton, GUILayout.Width(100)))
                            {
                                InstantiateSelectedWindow();
                            }
                        }
                    }
                }
            }
            
            DrawHistoryDebug();
            DrawFocusDebug();
            Repaint();
        }

        private void DrawFocusDebug()
        {
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                EditorGUILayout.LabelField("Focus Debug", EditorStyles.boldLabel);

                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Enter Play Mode to view current focused window and manual focus overrides.", MessageType.Info);
                }
                else
                {
                    IReadOnlyCollection<object> manual = windowsManager.ManuallyFocusedObjects;
                    int manualCount = manual.Count;
                    if (manualCount > 0)
                    {
                        EditorGUILayout.HelpBox("Focus is currently overridden by manually added object(s).", MessageType.Warning);
                        EditorGUILayout.LabelField($"Manual Focused Objects ({manualCount})", EditorStyles.miniBoldLabel);

                        int index = 0;
                        foreach (object obj in manual)
                        {
                            if (obj is Object unityObj)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.ObjectField($"[{index}]", unityObj, typeof(Object), true);
                                }
                            }
                            else
                            {
                                string label = obj != null ? $"{obj.GetType().Name}: {obj}" : "null";
                                EditorGUILayout.LabelField($"[{index}]", label);
                            }
                            index++;
                        }
                    }
                    else
                    {
                        WindowController focused = windowsManager.FocusedWindowController;
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            Object focusedObj = focused != null ? (Object)focused : null;
                            EditorGUILayout.ObjectField("Focused Window", focusedObj, typeof(WindowController), true);
                        }
                    }
                }
            }
        }


        private void DrawHistoryDebug()
        {
            using (new EditorGUILayout.VerticalScope("Box"))
            {
                EditorGUILayout.LabelField("History Debug", EditorStyles.boldLabel);

                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Enter Play Mode to view history.", MessageType.Info);
                    return;
                }

                bool canGoBack = windowsManager.CanGoBack();
                EditorGUILayout.LabelField("Can Go Back", canGoBack ? "Yes" : "No");

                IReadOnlyList<UIWindow> history = windowsManager.History;
                int count = history?.Count ?? 0;
                EditorGUILayout.LabelField("Count", count.ToString());

                UIWindow lastOpened = windowsManager.LastOpenedWindow;
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField("Last Opened", lastOpened, typeof(UIWindow), false);
                }

                EditorGUILayout.Space(2);

                if (count == 0)
                {
                    EditorGUILayout.LabelField("History", "<empty>");
                    return;
                }

                for (int i = 0; i < count; i++)
                {
                    int index = i;
                    UIWindow uiw = history[index];
                    bool isTop = index == count - 1;
                    bool isOpen = uiw != null && uiw.IsOpen();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        string label = isTop ? $"[{index}] TOP" : $"[{index}]";
                        GUILayout.Label(label, GUILayout.Width(70));

                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.ObjectField(uiw, typeof(UIWindow), false);
                        }

                        GUILayout.Label(isOpen ? "Open" : "Closed", isOpen ? EditorStyles.miniBoldLabel : EditorStyles.miniLabel, GUILayout.Width(55));
                        if (uiw != null)
                        {
                            GUILayout.Label(uiw.Layer != null ? uiw.Layer.name : "<NoLayer>", EditorStyles.miniLabel, GUILayout.Width(100));
                            GUILayout.Label(uiw.Layer != null && uiw.Layer.IncludedOnHistory ? "History:on" : "History:off", EditorStyles.miniLabel, GUILayout.Width(85));
                        }

                        using (new EditorGUI.DisabledGroupScope(uiw == null))
                        {
                            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(55)) && uiw != null)
                            {
                                Selection.activeObject = uiw;
                                EditorGUIUtility.PingObject(uiw);
                            }

                            if (GUILayout.Button("Focus", EditorStyles.miniButton, GUILayout.Width(50)) && uiw != null)
                            {
                                if (uiw.HasWindowInstance && uiw.WindowInstance)
                                {
                                    Selection.activeObject = uiw.WindowInstance.gameObject;
                                    EditorGUIUtility.PingObject(uiw.WindowInstance.gameObject);
                                }
                                else
                                {
                                    Selection.activeObject = uiw;
                                    EditorGUIUtility.PingObject(uiw);
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.Space(4);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(!canGoBack))
                    {
                        if (GUILayout.Button("Back", EditorStyles.miniButton, GUILayout.Width(60)))
                        {
                            windowsManager.Back();
                        }
                    }

                    if (GUILayout.Button("Log Stack", EditorStyles.miniButton, GUILayout.Width(80)))
                    {
                        IEnumerable<string> names = history.Select(h => h != null ? h.name : "<null>");
                        Debug.Log($"[WindowsManager] History: [" + string.Join(", ", names) + "]\nTop: " + (history[^1] != null ? history[^1].name : "<null>"));
                    }
                }
            }
        }

        private void OrganizeHierarchy()
        {
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
            if (selectedUIWindow == null)
                return;

            WindowsManager windowsManager = (WindowsManager)target;
            Transform root = windowsManager.transform;

            List<UILayer> layers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UILayer>();
            UILayer targetLayer = selectedUIWindow.Layer != null ? selectedUIWindow.Layer : (layers.Count > 0 ? layers[0] : null);

            RectTransform parent = targetLayer != null ? EnsureLayerParent(root, targetLayer) : (RectTransform)root;

            WindowController existing = FindExistingWindow(root, selectedUIWindow);
            if (existing != null)
            {
                if (existing.transform.parent != parent)
                {
                    Undo.SetTransformParent(existing.transform, parent, "Reparent UIWindow to Layer");
                }

                Undo.RecordObject(existing.gameObject, "Enable UIWindow");
                existing.gameObject.SetActive(true);
                existing.RectTransform.SetAsLastSibling();

                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            WindowController prefab = selectedUIWindow.GetWindowPrefab();
            if (prefab == null)
            {
                Debug.LogError($"Missing WindowController Prefab for {selectedUIWindow.name}");
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
