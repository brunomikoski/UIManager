using System.Collections.Generic;
using System.Linq;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public partial class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private WindowID initialWindowID;

        [SerializeField]
        private HierarchyWindowBehaviour hierarchyWindowBehaviour = HierarchyWindowBehaviour.CleanUp;


        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new Dictionary<LayerID, RectTransform>();
        private Dictionary<LayerID, List<WindowID>> layerToWindows = new Dictionary<LayerID, List<WindowID>>();
        private Dictionary<GroupID, List<WindowID>> groupToWindows = new Dictionary<GroupID, List<WindowID>>();
        private Dictionary<WindowID, Window> windowIDToWindowInstance = new Dictionary<WindowID, Window>();

        
        private List<WindowID> history = new List<WindowID>();

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            InitializeLayers();
            InitializeGroups();
            InitializeHierarchy();

            InitializeWindows();

            if (initialWindowID != null)
                Open(initialWindowID);
        }

        private void InitializeHierarchy()
        {
            Window[] windows = GetComponentsInChildren<Window>();
            for (int i = 0; i < windows.Length; i++)
            {
                if (hierarchyWindowBehaviour == HierarchyWindowBehaviour.CleanUp)
                {
                    Destroy(windows[i].gameObject);
                }
            }
        }

        public void Open(WindowID windowID)
        {
            if (IsWindowOpen(windowID))
                return;
            
            if (!windowIDToWindowInstance.TryGetValue(windowID, out Window windowInstance))
            {
                windowInstance = InitializeWindow(windowID);
            }

            LayerID windowLayer = windowID.LayerID;
            if (TryGetOpenWindowsOfLayer(windowLayer, out List<Window> openWindows))
            {
                for (int i = 0; i < openWindows.Count; i++)
                {
                    if (windowLayer.Behaviour == LayerBehaviour.Exclusive)
                    {
                        Close(openWindows[i]);
                    }
                    else if(windowLayer.Behaviour == LayerBehaviour.Additive)
                    {
                        SendToBackground(openWindows[i].WindowID);
                    }
                }
            }
            
            windowInstance.RectTransform.SetAsLastSibling();
            windowInstance.Open();
            history.Add(windowID);
            DispatchWindowEvent(WindowEvent.OnOpen, windowID);
            SetFocusedWindow(windowInstance);
        }
        
        private void SendToBackground(WindowID windowID)
        {
            if (!IsWindowOpen(windowID))
                return;

            if (!windowIDToWindowInstance.TryGetValue(windowID, out Window window)) 
                return;
            
            SendToBackground(window);
        }
        private void SendToBackground(Window window)
        {
            window.OnSentToBackground();
            DispatchWindowEvent(WindowEvent.OnEnterBackground, window.WindowID);
       }

        public void CloseLast()
        {
            if (history.Count == 0)
                return;

            WindowID last = history.Last();
            history.RemoveAt(history.Count - 1);
            Close(last);
        }

        public void Back()
        {
            CloseLast();
            if (history.Count == 0)
                return;
            
            WindowID last = history.Last();
            history.RemoveAt(history.Count - 1);
            Open(last);
        }
        
        private void Close(Window window)
        {
            window.Close();
            DispatchWindowEvent(WindowEvent.OnClose, window.WindowID);
        }
        
        private void Close(WindowID windowID)
        {
            if (!IsWindowOpen(windowID))
                return;

            if (!windowIDToWindowInstance.TryGetValue(windowID, out Window window)) 
                return;
            
            Close(window);

            if (windowID.LayerID.Behaviour == LayerBehaviour.Additive)
                UpdateFocusedWindow(windowID.LayerID);
        }

        private void UpdateFocusedWindow(LayerID windowIDLayerID)
        {
            if (!TryGetOpenWindowsOfLayer(windowIDLayerID, out List<Window> openWindows)) 
                return;
            
            Window focusedWindowInstance = openWindows.OrderBy(window => window.RectTransform.GetSiblingIndex()).Last();
            SetFocusedWindow(focusedWindowInstance);
        }

        private void SetFocusedWindow(Window targetWindow)
        {
            targetWindow.OnWindowFocused();
            DispatchWindowEvent(WindowEvent.OnBecomeFocused, targetWindow.WindowID);
        }

        private bool IsWindowOpen(WindowID windowID)
        {
            if (windowIDToWindowInstance.TryGetValue(windowID, out Window window))
                return window.IsOpen;
            return false;
        }

        private bool TryGetOpenWindowsOfLayer(LayerID layerID, out List<Window> resultWindows)
        {
            resultWindows = new List<Window>();
            if (layerToWindows.TryGetValue(layerID, out List<WindowID> windowIDs))
            {
                for (int i = 0; i < windowIDs.Count; i++)
                {
                    WindowID windowID = windowIDs[i];
                    if (windowIDToWindowInstance.TryGetValue(windowID, out Window window))
                    {
                        if (window.IsOpen)
                        {
                            resultWindows.Add(window);
                        } 
                    }
                }
            }

            return resultWindows.Count > 0;
        }

        private bool TryGetFirstOpenWindowOfLayer(LayerID layerID, out Window resultWindow)
        {
            if (TryGetOpenWindowsOfLayer(layerID, out List<Window> windows))
            {
                resultWindow = windows[0];
                return true;
            }

            resultWindow = null;
            return false;

        }

        private void InitializeLayers()
        {
            if (!CollectionsRegistry.Instance.TryGetCollection(out LayerIDs layers)) 
                return;
            
            for (int i = 0; i < layers.Count; i++)
            {
                CreateLayer(layers[i]);
            }
        }
        
        private void InitializeGroups()
        {
            if (!CollectionsRegistry.Instance.TryGetCollection(out WindowIDs windows))
                return;

            for (int i = 0; i < windows.Count; i++)
            {
                WindowID windowID = windows[i];

                if (windowID.GroupID == null) 
                    continue;
                
                if (!groupToWindows.ContainsKey(windowID.GroupID))
                {
                    groupToWindows.Add(windowID.GroupID, new List<WindowID>() {windowID});
                }
                else
                {
                    groupToWindows[windowID.GroupID].Add(windowID);
                }
            }
        }

        
        private void InitializeWindows()
        {
            if (!CollectionsRegistry.Instance.TryGetCollection(out WindowIDs windows))
                return;

            for (int i = 0; i < windows.Count; i++)
            {
                InitializeWindow(windows[i]);
            }
        }

        private Window InitializeWindow(WindowID windowID)
        {
            if (windowID is PrefabBasedWindowID prefabBasedWindowID)
                return InitializeWindow(prefabBasedWindowID);
            return null;
        }

        private Window InitializeWindow(PrefabBasedWindowID windowID)
        {
            if (windowIDToWindowInstance.ContainsKey(windowID))
            {
                return windowIDToWindowInstance[windowID];
            }
            
            RectTransform targetLayer = GetParentForLayer(windowID.LayerID);

            Window windowInstance = Instantiate(windowID.WindowPrefab, targetLayer, false);
            windowInstance.Initialize(this, windowID);
            windowIDToWindowInstance.Add(windowID, windowInstance);

            windowInstance.gameObject.SetActive(false);


            if (!layerToWindows.ContainsKey(windowID.LayerID))
                layerToWindows.Add(windowID.LayerID, new List<WindowID>());

            layerToWindows[windowID.LayerID].Add(windowID);
            
            
            windowID.SetWindowsManager(this);
            return windowInstance;
        }

        private RectTransform GetParentForLayer(LayerID layerID)
        {
            return layerToRectTransforms[layerID];
        }

        private void CreateLayer(LayerID targetLayer)
        {
            if (layerToRectTransforms.ContainsKey(targetLayer))
                return;
            
            GameObject newLayer = new GameObject(targetLayer.name, typeof(RectTransform));
            newLayer.transform.SetParent(transform, false);
            RectTransform rectTransform = (RectTransform) newLayer.transform;
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            layerToRectTransforms.Add(targetLayer, rectTransform);
        }
    }
}
