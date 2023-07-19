using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler))]
    [DisallowMultipleComponent]
    public partial class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private UIWindow[] initialWindows;

        private Dictionary<LongGuid, RectTransform> layerToRectTransforms = new();
        
        private List<UIWindow> history = new();
        
        private WindowController focusedWindowController;
        
        private List<UIWindow> allKnowWindows;
        private List<UIGroup> allKnowGroups;
        private List<UILayer> allKnowLayers;

        
        private Dictionary<UIWindow, WindowController> instantiatedWindows = new();

        private bool initialized;
        private bool isBackEnabled;

        protected virtual void Awake()
        {
            Initialize();
            LoadInitialWindows();
        }

        protected void Initialize()
        {
            if (initialized)
                return;

            allKnowWindows = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UIWindow>();
            allKnowGroups = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UIGroup>();
            allKnowLayers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UILayer>();

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
                UIGroup uiGroup = allKnowGroups[i];
                if (!uiGroup.AutoLoaded) 
                    continue;
                
                List<UIWindow> autoLoadedWindows = GetAllWindowsFromGroups(uiGroup);
                for (int j = 0; j < autoLoadedWindows.Count; j++)
                {
                    UIWindow autoLoadedUIWindow = autoLoadedWindows[j];
                    InstantiateWindow(autoLoadedUIWindow);
                }
            }
        }

        public void InitializeHierarchyWindows(Transform parent)
        {
            WindowController[] hierarchyWindows = parent.GetComponentsInChildren<WindowController>(true);
            List<WindowController> toBeDestroyedWindow = new List<WindowController>();
            for (int i = 0; i < hierarchyWindows.Length; i++)
            {
                WindowController hierarchyWindowController = hierarchyWindows[i];
                if (hierarchyWindowController.UIWindow == null)
                {
                    Debug.LogError($"WindowController Instance {hierarchyWindowController} doesn't have a UIWindow assigned to it, will be destroyed");
                    toBeDestroyedWindow.Add(hierarchyWindowController);
                    continue;
                }

                if (hierarchyWindowController.UIWindow.HasWindowInstance && hierarchyWindowController.UIWindow.WindowInstance != hierarchyWindowController)
                {
                    UnloadWindow(hierarchyWindowController.UIWindow);
                    Debug.LogError($"WindowController Instance {hierarchyWindowController} has a UIWindow assigned to it, but it already has a WindowController Instance assigned to it, destroying the previous one ");
                }

                InitializeWindowInstance(hierarchyWindowController);
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

            for (int i = 0; i < allKnowLayers.Count; i++)
            {
                UILayer uiLayer = allKnowLayers[i];

                RectTransform parentRectTransform = GetParentForLayer(uiLayer);
                parentRectTransform.SetSiblingIndex(uiLayer.Collection.IndexOf(uiLayer));
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

        private void InstantiateWindow(UIWindow uiWindow)
        {
            if (uiWindow.HasWindowInstance)
                return;
            
            WindowController windowControllerInstance = Instantiate(uiWindow.GetWindowPrefab());
            InitializeWindowInstance(windowControllerInstance);
        }

        private List<UIWindow> GetAllWindowsFromGroups(params UIGroup[] targetGroups)
        {
            List<UIWindow> resultWindows = new List<UIWindow>();
            for (int i = 0; i < allKnowWindows.Count; i++)
            {
                UIWindow uiWindow = allKnowWindows[i];
                for (int j = 0; j < targetGroups.Length; j++)
                {
                    UIGroup targetUIGroup = targetGroups[j];
                    if (!uiWindow.Group.Contains(targetUIGroup))
                        continue;

                    resultWindows.Add(uiWindow);
                }
            }

            return resultWindows;
        }

        public void Open(UIWindow uiWindow)
        {
            StartCoroutine(OpenEnumerator(uiWindow));
        }

        public IEnumerator OpenEnumerator(UIWindow targetUIWindow)
        {
            Initialize();
            
            if (!targetUIWindow.HasWindowInstance)
                CreateWindowInstanceForWindowID(targetUIWindow);

            if (IsWindowOpen(targetUIWindow))
                yield break;

            DispatchWindowEvent(WindowEvent.OnWillOpen, targetUIWindow.WindowInstance);

            List<WindowController> previouslyOpenWindow = GetAllOpenWindows();
            UILayer windowUILayer = targetUIWindow.Layer;
            if (windowUILayer.Behaviour == UILayerBehaviour.Exclusive)
            {
                if (TryGetOpenWindowsOfLayer(windowUILayer, out List<WindowController> layerOpenWindows))
                {
                    for (int i = 0; i < layerOpenWindows.Count; i++)
                    {
                        Close(layerOpenWindows[i].UIWindow);
                    }
                }
            }
            
            if (targetUIWindow.Layer.IncludedOnHistory)
                history.Add(targetUIWindow);

            targetUIWindow.WindowInstance.RectTransform.SetAsLastSibling();
            UpdateFocusedWindow();

            yield return targetUIWindow.WindowInstance.OpenEnumerator();
            OnWindowInstanceOpened(targetUIWindow.WindowInstance);
            DispatchTransition(previouslyOpenWindow, targetUIWindow.WindowInstance);
        }
        
        
        public void Close(UIWindow uiWindow)
        {
            StartCoroutine(CloseEnumerator(uiWindow));
        }

        public IEnumerator CloseEnumerator(UIWindow targetUIWindow)
        {
            Initialize();
            
            if (!IsWindowOpen(targetUIWindow))
                yield break;
            
            DispatchWindowEvent(WindowEvent.OnWillClose, targetUIWindow.WindowInstance);
            
            yield return targetUIWindow.WindowInstance.CloseEnumerator();
            
            DispatchWindowEvent(WindowEvent.OnClosed, targetUIWindow.WindowInstance);
            UpdateFocusedWindow();
        }

        private void OnWindowInstanceOpened(WindowController windowControllerInstance)
        {
            DispatchWindowEvent(WindowEvent.OnOpened, windowControllerInstance);
        }
        
        public bool TryGetWindowInstance<T>(UIWindow targetUIWindow, out T resultTypedWindow) where T : WindowController
        {
            if (instantiatedWindows.TryGetValue(targetUIWindow, out WindowController resultWindow))
            {
                resultTypedWindow = resultWindow as T;
                return resultTypedWindow != null;
            }

            resultTypedWindow = null;
            return false;
        }

        private List<WindowController> GetAllOpenWindows()
        {
            List<WindowController> resultOpenWindows = new List<WindowController>();
            for (int i = 0; i < allKnowWindows.Count; i++)
            {
                UIWindow uiWindow = allKnowWindows[i];
                
                if (!uiWindow.HasWindowInstance)
                    continue;
                if (!uiWindow.WindowInstance.IsOpen)
                    continue;
                resultOpenWindows.Add(uiWindow.WindowInstance);
            }

            return resultOpenWindows;
        }
        
        public virtual void CloseLast()
        {
            Initialize();
            
            if (history.Count == 0)
                return;

            UIWindow last = history.Last();
            history.RemoveAt(history.Count - 1);
            Close(last);
        }

        public virtual void Back()
        {
            Initialize();

            if (!isBackEnabled)
                return;
            
            if (history.Count <= 1)
                return;
            
            CloseLast();
            
            UIWindow last = history.Last();
            if (IsWindowOpen(last))
                return;
            
            history.RemoveAt(history.Count - 1);
            Open(last);
        }

        public void SetBackEnabled(bool isEnabled)
        {
            isBackEnabled = isEnabled;
        }
        
        private void UpdateFocusedWindow()
        {
            for (int i = 0; i < allKnowLayers.Count; i++)
            {
                UILayer uiLayer = allKnowLayers[i];
                if (TryGetOpenWindowsOfLayer(uiLayer, out List<WindowController> openWindows))
                {
                    openWindows.Sort((windowA, windowB) => windowA.RectTransform.GetSiblingIndex()
                        .CompareTo(windowB.RectTransform.GetSiblingIndex()));

                    SetFocusedWindow(openWindows.Last());
                    return;
                }
            }
        }

        private void SetFocusedWindow(WindowController targetWindowController)
        {
            if (targetWindowController == focusedWindowController)
                return;
            
            if (focusedWindowController != null)
            {
                focusedWindowController.OnLostFocus();
                DispatchWindowEvent(WindowEvent.OnLostFocus, focusedWindowController);
            }

            focusedWindowController = targetWindowController;
            focusedWindowController.OnGainFocus();
            DispatchWindowEvent(WindowEvent.OnGainFocus, focusedWindowController);
        }

        private bool IsWindowOpen(UIWindow uiWindow)
        {
            if (!uiWindow.WindowInstance)
                return false;

            return uiWindow.WindowInstance.IsOpen;
        }

        private bool TryGetOpenWindowsOfLayer(UILayer uiLayer, out List<WindowController> resultWindows)
        {
            resultWindows = new List<WindowController>();
            for (int i = 0; i < allKnowWindows.Count; i++)
            {
                UIWindow uiWindow = allKnowWindows[i];

                if (!uiWindow.HasWindowInstance)
                    continue;
                
                if (uiWindow.Layer != uiLayer)
                    continue;

                if (!uiWindow.IsOpen())
                    continue;
                
                resultWindows.Add(uiWindow.WindowInstance);
            }

            return resultWindows.Count > 0;
        }

        private bool TryGetFirstOpenWindowOfLayer(UILayer uiLayer, out WindowController resultWindowController)
        {
            if (TryGetOpenWindowsOfLayer(uiLayer, out List<WindowController> windows))
            {
                resultWindowController = windows[0];
                return true;
            }

            resultWindowController = null;
            return false;
        }

        private void CreateWindowInstanceForWindowID(UIWindow targetUIWindow)
        {
            WindowController windowControllerPrefab = targetUIWindow.GetWindowPrefab();
            if (windowControllerPrefab == null)
            {
                Debug.LogError($"Missing WindowController Prefab for UIWindow {targetUIWindow}");
                return;
            }
            WindowController windowControllerInstance = Instantiate(windowControllerPrefab, GetParentForLayer(targetUIWindow.Layer), false);
            InitializeWindowInstance(windowControllerInstance);
        }
        
        private void InitializeWindowInstance(WindowController windowControllerInstance)
        {
            UIWindow uiWindow = windowControllerInstance.UIWindow;
            Assert.IsNotNull(uiWindow);
            
            UILayer uiLayer = uiWindow.Layer;
            if (uiLayer == null)
                uiLayer = allKnowLayers[0];

            RectTransform parentLayer = GetParentForLayer(uiLayer);
            if (windowControllerInstance.RectTransform.parent != parentLayer)
                windowControllerInstance.RectTransform.SetParent(parentLayer, false);
            
            windowControllerInstance.gameObject.SetActive(false);
            windowControllerInstance.Initialize(this, uiWindow);
            uiWindow.SetWindowInstance(windowControllerInstance);
            instantiatedWindows.Add(uiWindow, windowControllerInstance);
            DispatchWindowEvent(WindowEvent.OnWindowInitialized, uiWindow.WindowInstance);
        }

        private RectTransform GetParentForLayer(UILayer uiLayer)
        {
            LongGuid targetUID = uiLayer == null ? allKnowLayers[0].GUID : uiLayer.GUID;

            return layerToRectTransforms[targetUID];
        }

        private RectTransform CreateLayer(UILayer targetUILayer)
        {
            if (layerToRectTransforms.TryGetValue(targetUILayer.GUID, out RectTransform rectTransform))
                return rectTransform;

            Transform layerTransform = transform.Find(targetUILayer.name);

            if (layerTransform == null)
            {
                GameObject layerGO = new(targetUILayer.name, typeof(RectTransform));
                layerGO.transform.SetParent(transform, false);
                layerTransform = layerGO.transform;
            }
            
            rectTransform = (RectTransform) layerTransform;
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.name = targetUILayer.name;

            layerToRectTransforms.Add(targetUILayer.GUID, rectTransform);
            return rectTransform;
        }

        public void LoadGroup(UIGroup targetUIGroupToLoad, Action onLoadedCallback = null  )
        {
            StartCoroutine(LoadGroupEnumerator(targetUIGroupToLoad, onLoadedCallback));
        }

        public IEnumerator LoadGroupEnumerator(UIGroup targetUIGroupToLoad, Action onLoadedCallback = null)
        {
            Initialize();

            List<UIWindow> allWindows = GetAllWindowsFromGroups(targetUIGroupToLoad);

            List<IAsyncPrefabLoader> prefabLoaders = new List<IAsyncPrefabLoader>(allWindows.Count);
            for (int i = 0; i < allWindows.Count; i++)
            {
                UIWindow uiWindow = allWindows[i];
                if (uiWindow.HasWindowInstance)
                    continue;

                if (uiWindow is IAsyncPrefabLoader asyncPrefabLoader)
                {
                    if (asyncPrefabLoader.IsLoaded())
                        continue;

                    DispatchWindowEvent(WindowEvent.OnWillBeLoaded, uiWindow);
                    asyncPrefabLoader.LoadPrefab(() =>
                    {
                        DispatchWindowEvent(WindowEvent.OnLoaded, uiWindow);
                    });
                    
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

        public void UnloadGroup(params UIGroup[] targetGroupToUnload)
        {
            Initialize();
            List<UIWindow> allWindows = GetAllWindowsFromGroups(targetGroupToUnload);
            for (int i = 0; i < allWindows.Count; i++)
            {
                UnloadWindow(allWindows[i]);
            }
        }

        public void UnloadWindow(UIWindow targetUIWindow)
        {
            DestroyWindowInstance(targetUIWindow);

            if (targetUIWindow is IAsyncPrefabLoader asyncPrefabLoader)
                asyncPrefabLoader.UnloadPrefab();
        }

        private void DestroyWindowInstance(UIWindow uiWindow)
        {
            if (!uiWindow.HasWindowInstance)
                return;
            
            DispatchWindowEvent(WindowEvent.OnWillBeDestroyed, uiWindow);

            WindowController targetInstance = uiWindow.WindowInstance;
            uiWindow.ClearWindowInstance();
            instantiatedWindows.Remove(uiWindow);
            Destroy(targetInstance.gameObject);
            DispatchWindowEvent(WindowEvent.OnDestroyed, uiWindow);
        }
    }
}
