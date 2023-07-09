using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler))]
    [DisallowMultipleComponent]
    public partial class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private WindowID[] initialWindows;


        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new();
        
        private List<WindowID> history = new();
        
        private Window focusedWindow;
        
        private List<WindowID> allKnowWindows;
        private List<GroupID> allKnowGroups;
        private List<LayerID> allKnowLayers;

        
        private Dictionary<WindowID, Window> instantiatedWindows = new();

        private bool initialized;

        protected virtual void Awake()
        {
            Initialize();
            LoadInitialWindows();
        }

        protected void Initialize()
        {
            if (initialized)
                return;

            allKnowWindows = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<WindowID>();
            allKnowGroups = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<GroupID>();
            allKnowLayers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<LayerID>();

            InitializeLayers();
            InitializeWindowIDs();
            InitializeHierarchyWindows(transform);
            InitializeAutoLoadedGroups();
            
            initialized = true;
        }

        private void InitializeAutoLoadedGroups()
        {
            for (int i = 0; i < allKnowGroups.Count; i++)
            {
                GroupID groupID = allKnowGroups[i];
                if (!groupID.AutoLoaded) 
                    continue;
                
                List<WindowID> autoLoadedWindows = GetAllWindowsFromGroups(groupID);
                for (int j = 0; j < autoLoadedWindows.Count; j++)
                {
                    WindowID autoLoadedWindow = autoLoadedWindows[j];
                    InstantiateWindow(autoLoadedWindow);
                }
            }
        }

        public void InitializeHierarchyWindows(Transform parent)
        {
            Window[] hierarchyWindows = parent.GetComponentsInChildren<Window>(true);
            List<Window> toBeDestroyedWindow = new List<Window>();
            for (int i = 0; i < hierarchyWindows.Length; i++)
            {
                Window hierarchyWindow = hierarchyWindows[i];
                if (hierarchyWindow.WindowID == null)
                {
                    Debug.LogError($"Window Instance {hierarchyWindow} doesn't have a WindowID assigned to it, will be destroyed");
                    toBeDestroyedWindow.Add(hierarchyWindow);
                    continue;
                }

                if (hierarchyWindow.WindowID.HasWindowInstance && hierarchyWindow.WindowID.WindowInstance != hierarchyWindow)
                {
                    UnloadWindow(hierarchyWindow.WindowID);
                    Debug.LogError($"Window Instance {hierarchyWindow} has a WindowID assigned to it, but it already has a Window Instance assigned to it, destroying the previous one ");
                }

                InitializeWindowInstance(hierarchyWindow);
            }

            for (int i = 0; i < toBeDestroyedWindow.Count; i++)
            {
                Destroy(toBeDestroyedWindow[i].gameObject);
            }
        }


        private void InitializeLayers()
        {
            for (int i = 0; i < allKnowLayers.Count; i++)
                CreateLayer(allKnowLayers[i]);

            foreach (var layerToRectTransform in layerToRectTransforms)
            {
                layerToRectTransform.Value.SetSiblingIndex(
                    layerToRectTransform.Key.Collection.IndexOf(layerToRectTransform.Key));
            }
        }

        private void InitializeWindowIDs()
        {
            for (int i = 0; i < allKnowWindows.Count; i++)
                allKnowWindows[i].Initialize(this);
        }

        protected void LoadInitialWindows()
        {
            for (int i = 0; i < initialWindows.Length; i++)
                Open(initialWindows[i]);
        }

        private void InstantiateWindow(WindowID windowID)
        {
            if (windowID.HasWindowInstance)
                return;
            
            Window windowInstance = Instantiate(windowID.GetWindowPrefab());
            InitializeWindowInstance(windowInstance);
        }

        private List<WindowID> GetAllWindowsFromGroups(params GroupID[] targetGroups)
        {
            List<WindowID> resultWindows = new List<WindowID>();
            for (int i = 0; i < allKnowWindows.Count; i++)
            {
                WindowID windowID = allKnowWindows[i];
                for (int j = 0; j < targetGroups.Length; j++)
                {
                    GroupID targetGroupID = targetGroups[j];
                    if (!windowID.Group.Contains(targetGroupID))
                        continue;

                    resultWindows.Add(windowID);
                }
            }

            return resultWindows;
        }

        public void Open(WindowID windowID)
        {
            StartCoroutine(OpenEnumerator(windowID));
        }

        public IEnumerator OpenEnumerator(WindowID targetWindowID)
        {
            Initialize();
            
            if (!targetWindowID.HasWindowInstance)
                CreateWindowInstanceForWindowID(targetWindowID);

            if (IsWindowOpen(targetWindowID))
                yield break;

            DispatchWindowEvent(WindowEvent.OnWillOpen, targetWindowID.WindowInstance);

            List<Window> previouslyOpenWindow = GetAllOpenWindows();
            LayerID windowLayer = targetWindowID.LayerID;
            if (windowLayer.Behaviour == LayerBehaviour.Exclusive)
            {
                if (TryGetOpenWindowsOfLayer(windowLayer, out List<Window> layerOpenWindows))
                {
                    for (int i = 0; i < layerOpenWindows.Count; i++)
                    {
                        Close(layerOpenWindows[i].WindowID);
                    }
                }
            }
            
            if (targetWindowID.LayerID.IncludedOnHistory)
                history.Add(targetWindowID);

            targetWindowID.WindowInstance.RectTransform.SetAsLastSibling();
            UpdateFocusedWindow();

            yield return targetWindowID.WindowInstance.OpenEnumerator();
            OnWindowInstanceOpened(targetWindowID.WindowInstance);
            DispatchTransition(previouslyOpenWindow, targetWindowID.WindowInstance);
        }
        
        
        public void Close(WindowID window)
        {
            StartCoroutine(CloseEnumerator(window));
        }

        public IEnumerator CloseEnumerator(WindowID targetWindowID)
        {
            Initialize();
            
            if (!IsWindowOpen(targetWindowID))
                yield break;
            
            DispatchWindowEvent(WindowEvent.OnWillClose, targetWindowID.WindowInstance);
            
            yield return targetWindowID.WindowInstance.CloseEnumerator();
            
            DispatchWindowEvent(WindowEvent.OnClosed, targetWindowID.WindowInstance);
            UpdateFocusedWindow();
        }

        private void OnWindowInstanceOpened(Window windowInstance)
        {
            DispatchWindowEvent(WindowEvent.OnOpened, windowInstance);
        }
        
        public bool TryGetWindowInstance<T>(WindowID targetWindowID, out T resultTypedWindow) where T : Window
        {
            if (instantiatedWindows.TryGetValue(targetWindowID, out Window resultWindow))
            {
                resultTypedWindow = resultWindow as T;
                return resultTypedWindow != null;
            }

            resultTypedWindow = null;
            return false;
        }

        private List<Window> GetAllOpenWindows()
        {
            List<Window> resultOpenWindows = new List<Window>();
            for (int i = 0; i < allKnowWindows.Count; i++)
            {
                WindowID windowID = allKnowWindows[i];
                
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
            Initialize();
            
            if (history.Count == 0)
                return;

            WindowID last = history.Last();
            history.RemoveAt(history.Count - 1);
            Close(last);
        }

        public void Back()
        {
            Initialize();
            
            if (history.Count <= 1)
                return;
            
            CloseLast();
            
            WindowID last = history.Last();
            if (IsWindowOpen(last))
                return;
            
            history.RemoveAt(history.Count - 1);
            Open(last);
        }
        
        private void UpdateFocusedWindow()
        {
            for (int i = 0; i < allKnowLayers.Count; i++)
            {
                LayerID layerID = allKnowLayers[i];
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
            if (targetWindow == focusedWindow)
                return;
            
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
            for (int i = 0; i < allKnowWindows.Count; i++)
            {
                WindowID windowID = allKnowWindows[i];

                if (!windowID.HasWindowInstance)
                    continue;
                
                if (windowID.LayerID != layerID)
                    continue;

                if (!windowID.IsOpen())
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

        private void CreateWindowInstanceForWindowID(WindowID targetWindowID)
        {
            Window windowPrefab = targetWindowID.GetWindowPrefab();
            if (windowPrefab == null)
            {
                Debug.LogError($"Missing Window Prefab for WindowID {targetWindowID}");
                return;
            }
            Window windowInstance = Instantiate(windowPrefab, GetParentForLayer(targetWindowID.LayerID), false);
            InitializeWindowInstance(windowInstance);
        }
        
        private void InitializeWindowInstance(Window windowInstance)
        {
            WindowID windowID = windowInstance.WindowID;
            LayerID layerID = windowID.LayerID;
            if (layerID == null)
            {
                layerID = allKnowLayers[0];
                Debug.LogWarning($"WindowID {windowID} doesn't have a LayerID assignedToID, using first one available {layerID}");
            }

            RectTransform parentLayer = GetParentForLayer(windowInstance.WindowID.LayerID);
            if (windowInstance.RectTransform.parent != parentLayer)
                windowInstance.RectTransform.SetParent(parentLayer, false);
            
            windowInstance.gameObject.SetActive(false);
            windowInstance.Initialize(this, windowID);
            windowID.SetWindowInstance(windowInstance);
            instantiatedWindows.Add(windowID, windowInstance);
            DispatchWindowEvent(WindowEvent.OnWindowInitialized, windowID.WindowInstance);
        }

        private RectTransform GetParentForLayer(LayerID layerID)
        {
            if (layerID == null)
                return layerToRectTransforms[allKnowLayers[0]];
            
            return layerToRectTransforms[layerID];
        }

        private void CreateLayer(LayerID targetLayer)
        {
            if (layerToRectTransforms.ContainsKey(targetLayer))
                return;

            Transform layerTransform = transform.Find(targetLayer.name);

            if (layerTransform == null)
            {
                GameObject layerGO = new(targetLayer.name, typeof(RectTransform));
                layerGO.transform.SetParent(transform, false);
                layerTransform = layerGO.transform;
            }
            
            RectTransform rectTransform = (RectTransform) layerTransform;
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.name = targetLayer.name;

            layerToRectTransforms.Add(targetLayer, rectTransform);
        }

        public void LoadGroup(GroupID targetGroupToLoad, Action onLoadedCallback = null  )
        {
            StartCoroutine(LoadGroupEnumerator(targetGroupToLoad, onLoadedCallback));
        }

        public IEnumerator LoadGroupEnumerator(GroupID targetGroupToLoad, Action onLoadedCallback = null)
        {
            Initialize();

            List<WindowID> allWindows = GetAllWindowsFromGroups(targetGroupToLoad);

            List<IAsyncPrefabLoader> prefabLoaders = new List<IAsyncPrefabLoader>(allWindows.Count);
            for (int i = 0; i < allWindows.Count; i++)
            {
                WindowID windowID = allWindows[i];
                if (windowID.HasWindowInstance)
                    continue;

                if (windowID is IAsyncPrefabLoader asyncPrefabLoader)
                {
                    if (asyncPrefabLoader.IsLoaded())
                        continue;

                    asyncPrefabLoader.LoadPrefab();
                    prefabLoaders.Add(asyncPrefabLoader);
                }
            }

            bool allPrefabsLoaded = false;
            while (!allPrefabsLoaded)
            {
                allPrefabsLoaded = true;
                for (int i = 0; i < prefabLoaders.Count; i++)
                {
                    IAsyncPrefabLoader prefabLoader = prefabLoaders[i];
                    if (!prefabLoader.IsLoaded())
                    {
                        allPrefabsLoaded = false;
                        break;
                    }
                }

                yield return null;
            }
            
            for (int i = 0; i < allWindows.Count; i++)
                CreateWindowInstanceForWindowID(allWindows[i]);
            
            onLoadedCallback?.Invoke();
        }

        public void UnloadGroup(params GroupID[] targetGroupToUnload)
        {
            Initialize();
            List<WindowID> allWindows = GetAllWindowsFromGroups(targetGroupToUnload);
            for (int i = 0; i < allWindows.Count; i++)
            {
                UnloadWindow(allWindows[i]);
            }
        }

        public void UnloadWindow(WindowID targetWindowID)
        {
            DestroyWindowInstance(targetWindowID);
                
            if (targetWindowID is IAsyncPrefabLoader asyncPrefabLoader)
                asyncPrefabLoader.UnloadPrefab();
        }

        private void DestroyWindowInstance(WindowID windowID)
        {
            if (!windowID.HasWindowInstance)
                return;

            Window targetInstance = windowID.WindowInstance;
            windowID.ClearWindowInstance();
            instantiatedWindows.Remove(windowID);
            Destroy(targetInstance.gameObject);
        }

        
    }
}
