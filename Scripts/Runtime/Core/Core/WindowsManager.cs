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
    [DisallowMultipleComponent]
    public partial class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private WindowID initialWindowID;

        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new Dictionary<LayerID, RectTransform>();
        
        private List<WindowID> history = new List<WindowID>();
        
        private Window focusedWindow;
        
        private List<WindowID> availableWindows;
        private List<GroupID> availableGroups;
        private List<LayerID> availableLayers;

        private void Awake()
        {
            Initialize();
            LoadInitialWindows();
        }

        private void Initialize()
        {
            CleanupHierarchy();

            availableWindows = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<WindowID>();
            availableGroups = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<GroupID>();
            availableLayers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<LayerID>();

            
            InitializeLayers();            
            InitializeWindows();
        }

        private void InitializeLayers()
        {
            for (int i = 0; i < availableLayers.Count; i++)
                CreateLayer(availableLayers[i]);
        }

        private void InitializeWindows()
        {
            for (int i = 0; i < availableWindows.Count; i++)
                availableWindows[i].Initialize(this);
        }

        private void LoadInitialWindows()
        {
            List<WindowID> initialWindowIDs = new List<WindowID>();
            for (int i = 0; i < availableWindows.Count; i++)
            {
                WindowID windowID = availableWindows[i];
                if (windowID.GroupID != null && !windowID.GroupID.AutoLoaded)
                    continue;
                
                initialWindowIDs.Add(windowID);
            }

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
            OnBeforeStartLoadingWindows(targetWindows);
            List<WindowID> loadingWindows = new List<WindowID>();
            for (int i = 0; i < targetWindows.Length; i++)
            {
                WindowID windowID = targetWindows[i];
                if (windowID.HasWindowInstance)
                    continue;
                
                StartCoroutine(InstantiateWindowEnumerator(windowID));
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

            OnFinishLoadingWindows();
            onWindowsLoaded?.Invoke(targetWindows);
        }

        protected virtual void OnBeforeStartLoadingWindows(WindowID[] targetWindows)
        {
            
        }
        
        protected virtual void OnFinishLoadingWindows()
        {
            
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
            LoadWindows(ids =>
            {
                onGroupLoaded?.Invoke(targetGroupIDs);
            }, GetAllWindowsForGroups(targetGroupIDs).ToArray());
        }
        
        public void UnloadGroup(params GroupID[] targetGroupIDs)
        {
            UnloadGroup(null, targetGroupIDs);
        }

        public List<WindowID> GetAllWindowsForGroups(params GroupID[] targetGroups)
        {
            List<WindowID> resultWindows = new List<WindowID>();
            for (int i = 0; i < availableWindows.Count; i++)
            {
                WindowID windowID = availableWindows[i];
                for (int j = 0; j < targetGroups.Length; j++)
                {
                    GroupID targetGroupID = targetGroups[j];
                    if (windowID.GroupID != targetGroupID)
                        continue;

                    resultWindows.Add(windowID);
                }
            }

            return resultWindows;
        }
        
        public void UnloadGroup(Action<GroupID[]> onGroupsUnloaded = null, params GroupID[] targetGroupIDs)
        {
            List<WindowID> windows = GetAllWindowsForGroups(targetGroupIDs);

            UnloadWindows(ids =>
            {
                onGroupsUnloaded?.Invoke(targetGroupIDs);
            }, windows.ToArray());
        }

        public void Open(WindowID windowID)
        {
            if (!windowID.HasWindowInstance)
            {
                LoadWindows(ids => Open(windowID), windowID);
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

            if (windowID.LayerID.IncludedOnHistory)
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
            for (int i = 0; i < availableWindows.Count; i++)
            {
                WindowID windowID = availableWindows[i];
                
                if (!windowID.HasWindowInstance)
                    continue;
                if (!windowID.WindowInstance.IsOpen)
                    continue;
                resultOpenWindows.Add(windowID.WindowInstance);
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
            for (int i = 0; i < availableLayers.Count; i++)
            {
                LayerID layerID = availableLayers[i];
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
            for (int i = 0; i < availableWindows.Count; i++)
            {
                WindowID windowID = availableWindows[i];

                if (!windowID.HasWindowInstance)
                    continue;
                
                if (windowID.LayerID != layerID)
                    continue;
                
                resultWindows.Add(windowID.WindowInstance);
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
        
        private IEnumerator InstantiateWindowEnumerator(WindowID windowID)
        {
            if (windowID.HasWindowInstance)
                yield break;

            if (windowID.LayerID == null)
            {
                Debug.LogError($"{windowID} has no layer assigned, Window will not be initialized", windowID);
                yield break;
            }

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
