using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public partial class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private WindowID initialWindowID;

        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new Dictionary<LayerID, RectTransform>();
        private Dictionary<LayerID, List<WindowID>> layerToWindows = new Dictionary<LayerID, List<WindowID>>();
        private Dictionary<GroupID, List<WindowID>> groupToWindows = new Dictionary<GroupID, List<WindowID>>();
        private Dictionary<WindowID, Window> windowIDToWindowInstance = new Dictionary<WindowID, Window>();

        
        private List<WindowID> history = new List<WindowID>();
        
        private Window focusedWindow;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            CleanupHierarchy();

            InitializeLayers();
            InitializeGroups();

            InitializeWindows();

            if (initialWindowID != null)
                Open(initialWindowID);
        }

        private void CleanupHierarchy()
        {
            Window[] windows = GetComponentsInChildren<Window>(true);
            for (int i = 0; i < windows.Length; i++)
            {
                Destroy(windows[i].gameObject);
            }
        }

        public bool IsWindowInstanceCreated(WindowID targetWindowID)
        {
            return TryGetWindowInstance(targetWindowID, out _);
        }
        
        public bool TryGetWindowInstance(WindowID windowID, out Window window)
        {
            return windowIDToWindowInstance.TryGetValue(windowID, out window);
        }

        public void InstantiateGroup(GroupID targetGroupID)
        {
            if (!groupToWindows.TryGetValue(targetGroupID, out List<WindowID> windowIDsFromGroup)) 
                return;
            
            for (int i = 0; i < windowIDsFromGroup.Count; i++)
            {
                WindowID windowID = windowIDsFromGroup[i];
                if (IsWindowInstanceCreated(windowID))
                    continue;

                InitializeWindow(windowID);
            }
        }

        public void DestroyGroup(GroupID targetGroupID)
        {
            if (!groupToWindows.TryGetValue(targetGroupID, out List<WindowID> windowIDsFromGroup)) 
                return;
            
            for (int i = 0; i < windowIDsFromGroup.Count; i++)
            {
                WindowID windowID = windowIDsFromGroup[i];
                if (!TryGetWindowInstance(windowID, out Window windowInstance))
                    continue;

                DestroyWindow(windowInstance);
            }
        }

        private void DestroyWindow(Window windowInstance)
        {
            windowIDToWindowInstance.Remove(windowInstance.WindowID);
            Destroy(windowInstance.gameObject);
        }

        public void Open(WindowID windowID)
        {
            if (IsWindowOpen(windowID))
                return;
            
            if(TryGetWindowInstance(windowID, out Window windowInstance))
            {
                windowInstance = InitializeWindow(windowID);
            }

            List<Window> previouslyOpenWindow = GetAllOpenWindows();
            LayerID windowLayer = windowID.LayerID;
            if (windowLayer.Behaviour == LayerBehaviour.Exclusive)
            {
                if (TryGetOpenWindowsOfLayer(windowLayer, out List<Window> layerOpenWindows))
                {
                    for (int i = 0; i < layerOpenWindows.Count; i++)
                    {
                        Close(layerOpenWindows[i]);
                    }
                }
            }
            
            windowInstance.RectTransform.SetAsLastSibling();
            windowInstance.Open();
            history.Add(windowID);
            DispatchWindowEvent(WindowEvent.OnOpen, windowInstance);
            DispatchTransition(previouslyOpenWindow, windowInstance);
            UpdateFocusedWindow();
        }

        private List<Window> GetAllOpenWindows()
        {
            List<Window> resultOpenWindows = new List<Window>();
            for (int i = LayerIDs.Values.Count - 1; i >= 0; i--)
            {
                if (!TryGetOpenWindowsOfLayer(LayerIDs.Values[i], out List<Window> openWindows)) 
                    continue;
                    
                resultOpenWindows.AddRange(openWindows);
            }

            return resultOpenWindows;
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
            if (history.Count <= 1)
                return;
            
            CloseLast();
            
            WindowID last = history.Last();
            if (IsWindowOpen(last))
                return;
            
            history.RemoveAt(history.Count - 1);
            Open(last);
        }
        
        private void Close(Window window)
        {
            window.Close();
            DispatchWindowEvent(WindowEvent.OnClose, window);
            UpdateFocusedWindow();
        }
        
        private void Close(WindowID windowID)
        {
            if (!IsWindowOpen(windowID))
                return;

            if (!windowIDToWindowInstance.TryGetValue(windowID, out Window window)) 
                return;
            
            Close(window);
        }

        private void UpdateFocusedWindow()
        {
            for (int i = LayerIDs.Values.Count - 1; i >= 0; i--)
            {
                LayerID layerID = LayerIDs.Values[i];
                if (TryGetOpenWindowsOfLayer(layerID, out List<Window> openWindows))
                {
                    openWindows.Sort((windowA, windowB) => windowA.RectTransform.GetSiblingIndex()
                        .CompareTo(windowB.RectTransform.GetSiblingIndex()));

                    for (int j = openWindows.Count - 1; j >= 0; j--)
                    {
                        SetFocusedWindow(openWindows[j]);
                        return;
                    }
                }
            }
        }

        private void SetFocusedWindow(Window targetWindow)
        {
            if (focusedWindow != null)
            {
                focusedWindow.OnLostFocus();
                DispatchWindowEvent(WindowEvent.OnLostFocus, focusedWindow);
            }

            focusedWindow = targetWindow;
            focusedWindow.OnGainFocus();
            DispatchWindowEvent(WindowEvent.OnGainFocus, focusedWindow);
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

        public void InitializeLayers()
        {
            for (int i = 0; i < LayerIDs.Values.Count; i++)
            {
                CreateLayer(LayerIDs.Values[i]);
                layerToWindows.Add(LayerIDs.Values[i], new List<WindowID>());
            }

            for (int i = 0; i < WindowIDs.Values.Count; i++)
            {
                layerToWindows[WindowIDs.Values[i].LayerID].Add(WindowIDs.Values[i]);
            }
            
        }
        
        private void InitializeGroups()
        {
            for (int i = 0; i < WindowIDs.Values.Count; i++)
            {
                WindowID windowID = WindowIDs.Values[i];

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
            for (int i = 0; i < WindowIDs.Values.Count; i++)
            {
                InitializeWindow(WindowIDs.Values[i]);
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
                return windowIDToWindowInstance[windowID];
            
            RectTransform targetLayer = GetParentForLayer(windowID.LayerID);

            Window windowInstance = Instantiate(windowID.WindowPrefab, targetLayer, false);

            windowInstance.Initialize(this, windowID);
            windowIDToWindowInstance.Add(windowID, windowInstance);

            windowInstance.gameObject.SetActive(false);

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
