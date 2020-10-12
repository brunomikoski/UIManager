using System;
using System.Collections;
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
        private List<WindowID> nullGroupWindows = new List<WindowID>();
        
        private List<WindowID> history = new List<WindowID>();
        
        private Window focusedWindow;

        private void Awake()
        {
            Initialize();
            LoadInitialWindows();
        }

        private void Initialize()
        {
            CleanupHierarchy();

            InitializeLayers();
            InitializeGroups();
        }

        private void LoadInitialWindows()
        {
            List<WindowID> initialWindowIDs = new List<WindowID>();
            for (int i = 0; i < GroupIDs.Values.Count; i++)
            {
                GroupID groupID = GroupIDs.Values[i];
                if (!groupID.AutoLoaded)
                    continue;
                initialWindowIDs.AddRange(groupToWindows[groupID]);
            }

            initialWindowIDs.AddRange(nullGroupWindows);

            StartCoroutine(LoadWindowsEnumerator(OnInitialWindowsLoaded, initialWindowIDs.ToArray()));
        }

        private void OnInitialWindowsLoaded(WindowID[] loadedWindows)
        {
            if (initialWindowID != null)
                Open(initialWindowID);
        }


        private IEnumerator LoadWindowsEnumerator(Action<WindowID[]> onWindowsLoaded, params WindowID[] targetWindows)
        {
            List<WindowID> loadingWindows = new List<WindowID>();
            for (int i = 0; i < targetWindows.Length; i++)
            {
                WindowID windowID = targetWindows[i];
                StartCoroutine(InitializeWindowEnumerator(windowID));
                loadingWindows.Add(windowID);
            }
            bool allLoaded = false;
            while (!allLoaded)
            {
                allLoaded = true;
                for (int i = 0; i < loadingWindows.Count; i++)
                {
                    WindowID windowID = loadingWindows[i];
                    if (!windowID.HasWindowInstance)
                    {
                        allLoaded = false;
                        break;
                    }
                }

                if (!allLoaded)
                    yield return null;
            }

            onWindowsLoaded?.Invoke(targetWindows);
        }

        private void CleanupHierarchy()
        {
            Window[] windows = GetComponentsInChildren<Window>(true);
            for (int i = 0; i < windows.Length; i++)
            {
                Destroy(windows[i].gameObject);
            }
        }

        // public void LoadGroup(GroupID targetGroupID)
        // {
        //     if (!groupToWindows.TryGetValue(targetGroupID, out List<WindowID> windowIDsFromGroup)) 
        //         return;
        //     
        //     for (int i = 0; i < windowIDsFromGroup.Count; i++)
        //     {
        //         WindowID windowID = windowIDsFromGroup[i];
        //         if (windowID.HasWindowInstance)
        //             continue;
        //
        //         InitializeWindow(windowID);
        //     }
        // }
        //
        // public void UnloadGroup(GroupID targetGroupID)
        // {
        //     if (!groupToWindows.TryGetValue(targetGroupID, out List<WindowID> windowIDsFromGroup)) 
        //         return;
        //     
        //     for (int i = 0; i < windowIDsFromGroup.Count; i++)
        //     {
        //         WindowID windowID = windowIDsFromGroup[i];
        //         if(!windowID.HasWindowInstance)
        //             continue;
        //
        //         DestroyWindow(windowID.WindowInstance);
        //     }
        // }
        //
        // private void DestroyWindow(Window windowInstance)
        // {
        //     Destroy(windowInstance.gameObject);
        // }

        public void Open(WindowID windowID)
        {
            if (!windowID.HasWindowInstance)
                throw new Exception($"Instance of Window {windowID} is not loaded, did you forgot to load the group");

            if (IsWindowOpen(windowID))
                return;

            DispatchWindowEvent(WindowEvent.OnWillOpen, windowID.WindowInstance);

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
            
            windowID.WindowInstance.RectTransform.SetAsLastSibling();
            UpdateFocusedWindow();

            windowID.WindowInstance.Open(OnWindowInstanceOpened);
            
            history.Add(windowID);
            DispatchTransition(previouslyOpenWindow, windowID.WindowInstance);
        }

        private void OnWindowInstanceOpened(Window windowInstance)
        {
            DispatchWindowEvent(WindowEvent.OnOpened, windowInstance);
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
            DispatchWindowEvent(WindowEvent.OnWillClose, window);
            window.Close(OnWindowInstanceClosed);
            UpdateFocusedWindow();
        }

        private void OnWindowInstanceClosed(Window window)
        {
            DispatchWindowEvent(WindowEvent.OnClosed, window);
        }
        
        private void Close(WindowID windowID)
        {
            if (!IsWindowOpen(windowID))
                return;

            Close(windowID.WindowInstance);
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

                    SetFocusedWindow(openWindows.Last());
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
            if (!windowID.WindowInstance)
                return false;

            return windowID.WindowInstance.IsOpen;
        }

        private bool TryGetOpenWindowsOfLayer(LayerID layerID, out List<Window> resultWindows)
        {
            resultWindows = new List<Window>();
            if (layerToWindows.TryGetValue(layerID, out List<WindowID> windowIDs))
            {
                for (int i = 0; i < windowIDs.Count; i++)
                {
                    WindowID windowID = windowIDs[i];
                    if (!windowID.HasWindowInstance)
                        continue;
                    
 
                    if (windowID.WindowInstance.IsOpen)
                        resultWindows.Add(windowID.WindowInstance);
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
            for (int i = 0; i < GroupIDs.Values.Count; i++)
            {
                groupToWindows.Add(GroupIDs.Values[i], new List<WindowID>());
            }
            

            for (int i = 0; i < WindowIDs.Values.Count; i++)
            {
                WindowID windowID = WindowIDs.Values[i];

                if (windowID.GroupID == null)
                {
                    nullGroupWindows.Add(windowID);
                    continue;
                }
                
                groupToWindows[windowID.GroupID].Add(windowID);
            }
        }

        
        private IEnumerator InitializeWindowEnumerator(WindowID windowID)
        {
            if (windowID.HasWindowInstance)
                yield break;

            yield return windowID.InstantiateEnumerator(this);
            windowID.WindowInstance.transform.SetParent(GetParentForLayer(windowID.LayerID), false);
            windowID.WindowInstance.gameObject.SetActive(false);
            windowID.WindowInstance.Initialize(this, windowID);
            DispatchWindowEvent(WindowEvent.OnWindowInitialized, windowID.WindowInstance);
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
