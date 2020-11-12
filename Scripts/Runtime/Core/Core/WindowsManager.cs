using System;
using System.Collections;
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

        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new Dictionary<LayerID, RectTransform>();
        private Dictionary<LayerID, List<WindowID>> layerToWindows = new Dictionary<LayerID, List<WindowID>>();
        private Dictionary<GroupID, List<WindowID>> groupToWindows = new Dictionary<GroupID, List<WindowID>>();
        private List<WindowID> nullGroupWindows = new List<WindowID>();
        
        private List<WindowID> history = new List<WindowID>();
        
        private Window focusedWindow;

        private List<WindowID> availableWindowIDs = new List<WindowID>();
        private List<GroupID> availableGroupIDs = new List<GroupID>();
        private List<LayerID> availableLayerIDs = new List<LayerID>();

        private void Awake()
        {
            Initialize();
            LoadInitialWindows();
        }

        private void Initialize()
        {
            CleanupHierarchy();

            InitializeCollections();
            InitializeLayers();
            InitializeGroups();
            InitializeWindows();
        }

        private void InitializeCollections()
        {
            if (CollectionsRegistry.Instance.TryGetCollectionsOfType(out List<WindowIDs> windowsCollections))
            {
                for (int i = 0; i < windowsCollections.Count; i++)
                    availableWindowIDs.AddRange(windowsCollections[i]);
            }
            
            if (CollectionsRegistry.Instance.TryGetCollectionsOfType(out List<GroupIDs> groupsCollections))
            {
                for (int i = 0; i < groupsCollections.Count; i++)
                    availableGroupIDs.AddRange(groupsCollections[i]);
            }
            
            if (CollectionsRegistry.Instance.TryGetCollectionsOfType(out List<LayerIDs> layersCollections))
            {
                for (int i = 0; i < layersCollections.Count; i++)
                    availableLayerIDs.AddRange(layersCollections[i]);
            }
        }

        private void InitializeWindows()
        {
            for (int i = 0; i < availableWindowIDs.Count; i++)
            {
                availableWindowIDs[i].SetWindowsManager(this);
            }
        }

        private void LoadInitialWindows()
        {
            List<WindowID> initialWindowIDs = new List<WindowID>();
            for (int i = 0; i < availableGroupIDs.Count; i++)
            {
                GroupID groupID = availableGroupIDs[i];
                if (!groupID.AutoLoaded)
                    continue;
                initialWindowIDs.AddRange(groupToWindows[groupID]);
            }

            initialWindowIDs.AddRange(nullGroupWindows);

            LoadWindows(OnInitialWindowsLoaded, initialWindowIDs.ToArray());
        }

        private void OnInitialWindowsLoaded(WindowID[] loadedWindows)
        {
            if (initialWindowID != null)
                Open(initialWindowID);
        }

        private void LoadWindows(Action<WindowID[]> onWindowsLoaded, params WindowID[] targetWindows)
        {
            StartCoroutine(LoadWindowsEnumerator(onWindowsLoaded, targetWindows));
        }

        private void UnloadWindows(Action<WindowID[]> onWindowsLoaded, params WindowID[] targetWindows)
        {
            StartCoroutine(UnloadWindowsEnumerator(onWindowsLoaded, targetWindows));
        }

        private IEnumerator UnloadWindowsEnumerator(Action<WindowID[]> onWindowsLoaded, params WindowID[] targetWindows)
        {
            List<WindowID> unLoadingWindows = new List<WindowID>();
            for (int i = 0; i < targetWindows.Length; i++)
            {
                WindowID windowID = targetWindows[i];
                if (!windowID.HasWindowInstance)
                    continue;

                StartCoroutine(DestroyWindowEnumerator(windowID));
                unLoadingWindows.Add(windowID);
            }
            bool allUnloaded = false;
            while (!allUnloaded)
            {
                allUnloaded = true;
                for (int i = 0; i < unLoadingWindows.Count; i++)
                {
                    WindowID windowID = unLoadingWindows[i];
                    if (windowID.HasWindowInstance)
                    {
                        allUnloaded = false;
                        break;
                    }
                }

                if (!allUnloaded)
                    yield return null;
            }

            onWindowsLoaded?.Invoke(targetWindows);
        }

        private IEnumerator LoadWindowsEnumerator(Action<WindowID[]> onWindowsLoaded, params WindowID[] targetWindows)
        {
            List<WindowID> loadingWindows = new List<WindowID>();
            for (int i = 0; i < targetWindows.Length; i++)
            {
                WindowID windowID = targetWindows[i];
                if (windowID.HasWindowInstance)
                    continue;
                
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
        
        public void LoadGroup(params GroupID[] targetGroupIDs)
        {
            LoadGroup(null, targetGroupIDs);
        }
        
        public void LoadGroup(Action<GroupID[]> onGroupLoaded = null, params GroupID[] targetGroupIDs)
        {
            List<WindowID> windows = new List<WindowID>();
            for (int i = 0; i < targetGroupIDs.Length; i++)
            {
                windows.AddRange(groupToWindows[targetGroupIDs[i]]);
            }

            LoadWindows(ids =>
            {
                onGroupLoaded?.Invoke(targetGroupIDs);
            }, windows.ToArray());
        }
        
        public void UnloadGroup(params GroupID[] targetGroupIDs)
        {
            UnloadGroup(null, targetGroupIDs);
        }
        
        public void UnloadGroup(Action<GroupID[]> onGroupsUnloaded = null, params GroupID[] targetGroupIDs)
        {
            List<WindowID> windows = new List<WindowID>();
            for (int i = 0; i < targetGroupIDs.Length; i++)
            {
                windows.AddRange(groupToWindows[targetGroupIDs[i]]);
            }

            UnloadWindows(ids =>
            {
                onGroupsUnloaded?.Invoke(targetGroupIDs);
            }, windows.ToArray());
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
            {
                if (windowID.GroupID == null)
                {
                    LoadWindows(ids =>
                    {
                        Open(windowID);
                    }, windowID);
                    return;
                }

                List<WindowID> groupWindows = groupToWindows[windowID.GroupID];
                LoadWindows(ids =>
                {
                    Open(windowID);
                }, groupWindows.ToArray());
                return;
            }

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
            for (int i = availableLayerIDs.Count - 1; i >= 0; i--)
            {
                if (!TryGetOpenWindowsOfLayer(availableLayerIDs[i], out List<Window> openWindows)) 
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
        
        public void Close(WindowID windowID)
        {
            if (!IsWindowOpen(windowID))
                return;

            Close(windowID.WindowInstance);
        }

        private void UpdateFocusedWindow()
        {
            for (int i = availableLayerIDs.Count - 1; i >= 0; i--)
            {
                LayerID layerID = availableLayerIDs[i];
                if (TryGetOpenWindowsOfLayer(layerID, out List<Window> openWindows))
                {
                    openWindows.Sort((windowA, windowB) => windowA.RectTransform.GetSiblingIndex()
                        .CompareTo(windowB.RectTransform.GetSiblingIndex()));

                    SetFocusedWindow(openWindows.Last());
                    return;
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
            for (int i = 0; i < availableLayerIDs.Count; i++)
            {
                CreateLayer(availableLayerIDs[i]);
                layerToWindows.Add(availableLayerIDs[i], new List<WindowID>());
            }

            for (int i = 0; i < availableWindowIDs.Count; i++)
            {
                layerToWindows[availableWindowIDs[i].LayerID].Add(availableWindowIDs[i]);
            }
            
        }
        
        private void InitializeGroups()
        {
            for (int i = 0; i < availableGroupIDs.Count; i++)
            {
                groupToWindows.Add(availableGroupIDs[i], new List<WindowID>());
            }
            

            for (int i = 0; i < availableWindowIDs.Count; i++)
            {
                WindowID windowID = availableWindowIDs[i];

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

        private IEnumerator DestroyWindowEnumerator(WindowID windowID)
        {
            if (!windowID.HasWindowInstance)
                yield break;

            DispatchWindowEvent(WindowEvent.OnWindowWillBeDestroyed, windowID.WindowInstance);
            yield return windowID.DestroyEnumerator();
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
