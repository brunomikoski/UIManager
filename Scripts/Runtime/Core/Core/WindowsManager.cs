using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BrunoMikoski.ScriptableObjectCollections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler))]
    [DisallowMultipleComponent]
    public partial class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private UIWindow[] initialWindows;

        private Dictionary<UILayer, RectTransform> layerToRectTransforms = new();
        
        private List<UIWindow> history = new();
        public IReadOnlyList<UIWindow> History => history;
        private WindowController focusedWindowController;
        private HashSet<object> manuallyFocusedObjects = new();
        
        private List<UIWindow> allKnownWindows;
        private List<UIGroup> allKnownGroups;
        private List<UILayer> allKnownLayers;

        
        private Dictionary<UIWindow, WindowController> instantiatedWindows = new();
        private HashSet<UIGroup> loadedGroups = new();

        private bool initialized;
        public bool IsBackEnabled { get; private set; }= true;

        public UIWindow LastOpenedWindow { get; private set; }

        private static bool IsQuitting;

        public WindowController FocusedWindowController => focusedWindowController;
        public IReadOnlyCollection<object> ManuallyFocusedObjects => manuallyFocusedObjects;

        protected virtual void Awake()
        {
            IsQuitting = false;
            Initialize();
            LoadInitialWindows();
        }

        protected void Initialize()
        {
            if (initialized)
                return;

            allKnownWindows = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UIWindow>();
            allKnownGroups = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UIGroup>();
            allKnownLayers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<UILayer>();

            InitializeLayers();
            InitializeWindowIDs();
            InitializeHierarchyWindows(transform);
            InitializeAutoLoadedGroups();
            
            initialized = true;
        }

        private void OnApplicationQuit()
        {
            IsQuitting = true;
        }
        
        private void InitializeAutoLoadedGroups()
        {
            for (int i = 0; i < allKnownGroups.Count; i++)
            {
                UIGroup uiGroup = allKnownGroups[i];
                if (!uiGroup.AutoLoaded) 
                    continue;
                
                List<UIWindow> autoLoadedWindows = GetAllWindowsFromGroups(uiGroup);
                for (int j = 0; j < autoLoadedWindows.Count; j++)
                {
                    UIWindow autoLoadedUIWindow = autoLoadedWindows[j];
                    InstantiateWindow(autoLoadedUIWindow);
                }
                loadedGroups.Add(uiGroup);
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
                    Debug.LogWarning($"WindowController Instance {hierarchyWindowController} doesn't have a UIWindow assigned to it, will be destroyed");
                    toBeDestroyedWindow.Add(hierarchyWindowController);
                    continue;
                }

                if (hierarchyWindowController.UIWindow.HasWindowInstance && hierarchyWindowController.UIWindow.WindowInstance != hierarchyWindowController)
                {
                    UnloadWindow(hierarchyWindowController.UIWindow);
                    Debug.LogWarning($"WindowController Instance {hierarchyWindowController} has a UIWindow assigned to it, but it already has a WindowController Instance assigned to it, destroying the previous one ");
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
            for (int i = 0; i < allKnownLayers.Count; i++)
            {
                UILayer uiLayer = allKnownLayers[i];
                uiLayer.Initialize(this);
                CreateLayer(uiLayer);
            }
            
            foreach (var layerToRectTransform in layerToRectTransforms)
            {
                layerToRectTransform.Value.SetSiblingIndex(layerToRectTransform.Key.Index);
            }

            for (int i = 0; i < allKnownLayers.Count; i++)
            {
                UILayer uiLayer = allKnownLayers[i];

                RectTransform parentRectTransform = GetParentForLayer(uiLayer);
                parentRectTransform.SetSiblingIndex(uiLayer.Collection.IndexOf(uiLayer));
            }
        }

        private void InitializeWindowIDs()
        {
            for (int i = 0; i < allKnownWindows.Count; i++)
                allKnownWindows[i].Initialize(this);
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

            WindowController windowController = uiWindow.GetWindowPrefab();
            if (windowController == null)
                throw new NullReferenceException($"{uiWindow} is missing the prefab reference");
            
            WindowController windowControllerInstance = Instantiate(windowController);
            InitializeWindowInstance(windowControllerInstance);
        }

        private List<UIWindow> GetAllWindowsFromGroups(params UIGroup[] targetGroups)
        {
            List<UIWindow> resultWindows = new List<UIWindow>();
            for (int i = 0; i < allKnownWindows.Count; i++)
            {
                UIWindow uiWindow = allKnownWindows[i];
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

        public bool Open(UIWindow uiWindow)
        {
            if (IsQuitting)
                return false;

            if (IsWindowOpen(uiWindow))
                return false;

            Initialize();
            
            if (!uiWindow.HasWindowInstance)
                CreateWindowInstanceForWindowID(uiWindow);

            Coroutine openRoutine = StartCoroutine(OpenEnumerator(uiWindow));
            uiWindow.WindowInstance.SetCurrentActiveTransitionCoroutine(openRoutine);
            LastOpenedWindow = uiWindow;
            return true;
        }

        private IEnumerator OpenEnumerator(UIWindow targetUIWindow)
        {
            DispatchWindowEvent(WindowEvent.BeforeWindowOpen, targetUIWindow.WindowInstance);

            List<WindowController> previouslyOpenWindow = GetAllOpenWindows();
            UILayer windowUILayer = targetUIWindow.Layer;
            List<WindowController> layerOpenWindows = ListPool<WindowController>.Get();
            bool layerWasEmpty = !TryGetOpenWindowsOfLayer(windowUILayer, layerOpenWindows);
            if (!layerWasEmpty && windowUILayer.Behaviour == UILayerBehaviour.Exclusive)
            {
                for (int i = 0; i < layerOpenWindows.Count; i++)
                {
                    Close(layerOpenWindows[i].UIWindow);
                }
            }
            ListPool<WindowController>.Release(layerOpenWindows);
            
            targetUIWindow.WindowInstance.RectTransform.SetAsLastSibling();

            yield return targetUIWindow.WindowInstance.OpenEnumerator();
            if (targetUIWindow.WindowInstance != null && targetUIWindow.Layer.IncludedOnHistory)
            {
                if (history.Count == 0 || !ReferenceEquals(history[^1], targetUIWindow))
                {
                    history.Add(targetUIWindow);
                }            
            }
            
            if (targetUIWindow.WindowInstance != null)
            {
                OnWindowInstanceOpened(targetUIWindow.WindowInstance);
                if (layerWasEmpty)
                    OnLayerFirstWindowActivated(windowUILayer);
                DispatchTransition(previouslyOpenWindow, targetUIWindow.WindowInstance);
            }
            
            UpdateFocusedWindow();
            if (targetUIWindow.WindowInstance != null)
                targetUIWindow.WindowInstance.ClearCurrentActiveTransitionCoroutine();
        }


        public void CloseAllFromLayer(UILayer popup)
        {
            if (IsQuitting)
                return;

            List<WindowController> openWindows = ListPool<WindowController>.Get();
            if (TryGetOpenWindowsOfLayer(popup, openWindows))
            {
                for (int i = 0; i < openWindows.Count; i++)
                {
                    Close(openWindows[i].UIWindow);
                }
            }
            ListPool<WindowController>.Release(openWindows);
        }

        public bool Close(UIWindow uiWindow)
        {
            if (IsQuitting)
                return false;

            if (!IsWindowOpen(uiWindow))
                return false;

            Coroutine transitionEnumerator = StartCoroutine(CloseEnumerator(uiWindow));
            uiWindow.WindowInstance.SetCurrentActiveTransitionCoroutine(transitionEnumerator);
            return true;
        }

        private IEnumerator CloseEnumerator(UIWindow targetUIWindow)
        {
            Initialize();
            
            DispatchWindowEvent(WindowEvent.BeforeWindowClose, targetUIWindow.WindowInstance);

            yield return targetUIWindow.WindowInstance.CloseEnumerator();

            if (ReferenceEquals(LastOpenedWindow, targetUIWindow))
                LastOpenedWindow = null;
            
            if (targetUIWindow.WindowInstance != null)
                DispatchWindowEvent(WindowEvent.WindowClosed, targetUIWindow.WindowInstance);
            
            List<WindowController> remainingWindows = ListPool<WindowController>.Get();
            bool layerIsNowEmpty = !TryGetOpenWindowsOfLayer(targetUIWindow.Layer, remainingWindows);
            ListPool<WindowController>.Release(remainingWindows);
            if (layerIsNowEmpty)
            {
                OnLastWindowFromLayerClosed(targetUIWindow.Layer);
            }
            
            UpdateFocusedWindow();
            if (targetUIWindow.WindowInstance != null)
                targetUIWindow.WindowInstance.ClearCurrentActiveTransitionCoroutine();
        }

        private void OnWindowInstanceOpened(WindowController windowControllerInstance)
        {
            DispatchWindowEvent(WindowEvent.WindowOpened, windowControllerInstance);
        }
        
        protected virtual void OnLastWindowFromLayerClosed(UILayer uiLayer) { }
        protected virtual void OnLayerFirstWindowActivated(UILayer uiLayer) { }
        
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
            for (int i = 0; i < allKnownWindows.Count; i++)
            {
                UIWindow uiWindow = allKnownWindows[i];
                
                if (!uiWindow.HasWindowInstance)
                    continue;
                if (!uiWindow.WindowInstance.IsOpen)
                    continue;
                
                resultOpenWindows.Add(uiWindow.WindowInstance);
            }

            return resultOpenWindows;
        }
        
        public virtual void CloseLastOpenWindow()
        {
            if (LastOpenedWindow == null)
                return;

            Initialize();

            Close(LastOpenedWindow);

            if (history.Count > 0 && ReferenceEquals(history[^1], LastOpenedWindow))
            {
                history.RemoveAt(history.Count - 1);
            }

            LastOpenedWindow = null;
        }

        public virtual void CloseFocusedWindow()
        {
            if (focusedWindowController == null)
                return;

            Initialize();

            Close(focusedWindowController.UIWindow);

            if (ReferenceEquals(LastOpenedWindow, focusedWindowController.UIWindow))
                LastOpenedWindow = null;

            if (history.Count > 0 && ReferenceEquals(history[^1], focusedWindowController.UIWindow))
                history.RemoveAt(history.Count - 1);
        }

        public virtual void Back()
        {
            Initialize();

            if (!CanGoBack())
                return;

            CloseLastOpenWindow();

            if (history.Count == 0)
                return;

            UIWindow last = history.Last();
            if (IsWindowOpen(last))
                return;

            history.RemoveAt(history.Count - 1);
            Open(last);
        }
        
        public bool CanGoBack()
        {
            Initialize();

            if (!IsBackEnabled)
                return false;
            
            if (history.Count < 1)
                return false;
            
            return true;
        }

        public void SetBackEnabled(bool isEnabled)
        {
            IsBackEnabled = isEnabled;
        }
        
        private void UpdateFocusedWindow()
        {
            if (manuallyFocusedObjects.Count > 0)
            {
                SetFocusedWindow(null);
                return;
            }
            
            List<WindowController> openWindows = ListPool<WindowController>.Get();
            for (int i = allKnownLayers.Count - 1; i >= 0; i--)
            {
                UILayer uiLayer = allKnownLayers[i];
                if (TryGetOpenWindowsOfLayer(uiLayer, openWindows))
                {
                    openWindows.Sort((windowA, windowB) => windowA.RectTransform.GetSiblingIndex()
                        .CompareTo(windowB.RectTransform.GetSiblingIndex()));

                    WindowController found = null;
                    for (int j = openWindows.Count - 1; j >= 0; j--)
                    {
                        WindowController windowController = openWindows[j];
                        if (!windowController.UIWindow.CanReceiveFocus)
                            continue;

                        found = windowController;
                        break;
                    }

                    if (found != null)
                    {
                        ListPool<WindowController>.Release(openWindows);
                        SetFocusedWindow(found);
                        return;
                    }
                }
            }
            ListPool<WindowController>.Release(openWindows);
        }


        public void AddManuallyFocusedObject(object focusedObject)
        {
            manuallyFocusedObjects.Add(focusedObject);
            DispatchManuallyFocusedObjectAdded(focusedObject);
            UpdateFocusedWindow();
        }
        
        public void RemoveManuallyFocusedObject(object focusedObject)
        {
            manuallyFocusedObjects.Remove(focusedObject);
            DispatchManuallyFocusedObjectRemoved(focusedObject);
            
            UpdateFocusedWindow();
        }

        private void SetFocusedWindow(WindowController targetWindowController)
        {
            if (targetWindowController == focusedWindowController)
                return;
            
            UILayer previousLayer = focusedWindowController != null ? focusedWindowController.UIWindow.Layer : null;

            if (focusedWindowController != null)
            {
                DispatchWindowEvent(WindowEvent.WindowLostFocus, focusedWindowController);
                focusedWindowController.OnLostFocus();
            }

            focusedWindowController = targetWindowController;
            if (focusedWindowController != null)
            {
                DispatchWindowEvent(WindowEvent.WindowGainedFocus, focusedWindowController);
                focusedWindowController.OnGainFocus();
            }

            UILayer newLayer = focusedWindowController != null ? focusedWindowController.UIWindow.Layer : null;
            if (previousLayer != newLayer)
            {
                if (previousLayer != null)
                    DispatchLayerEvent(LayerEvent.LayerLostFocus, previousLayer);
                if (newLayer != null)
                    DispatchLayerEvent(LayerEvent.LayerGainedFocus, newLayer);
                
                DispatchAnyLayerFocusChanged(previousLayer, newLayer);
            }
        }

        private bool IsWindowOpen(UIWindow uiWindow)
        {
            if (!uiWindow.WindowInstance)
                return false;

            return uiWindow.WindowInstance.IsOpen;
        }

        public bool TryGetOpenWindowsOfLayer(UILayer uiLayer, List<WindowController> resultWindows)
        {
            if (resultWindows == null)
                resultWindows = new List<WindowController>();
            else
                resultWindows.Clear();

            for (int i = 0; i < allKnownWindows.Count; i++)
            {
                UIWindow uiWindow = allKnownWindows[i];

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
        
        protected bool HasAnythingAboveThisLayerOpen(UILayer targetLayer, params UILayer[] excludedLayers)
        {
            int initialLayerIndex = targetLayer.Index + 1;
            if (initialLayerIndex >= allKnownLayers.Count)
                return false;
            
            for (int i = initialLayerIndex; i < allKnownLayers.Count; i++)
            {
                UILayer uiLayer = allKnownLayers[i];

                bool shouldIgnoreLayer = false;
                foreach (UILayer excludedLayer in excludedLayers)
                {
                    if (excludedLayer == uiLayer)
                    {
                        shouldIgnoreLayer = true;
                        break;  
                    }
                }

                if (shouldIgnoreLayer)
                    continue;
                

                List<WindowController> layerWindows = ListPool<WindowController>.Get();
                if (TryGetOpenWindowsOfLayer(uiLayer, layerWindows))
                {
                    ListPool<WindowController>.Release(layerWindows);
                    return true;
                }
                ListPool<WindowController>.Release(layerWindows);
            }

            return false;
        }

        private bool TryGetFirstOpenWindowOfLayer(UILayer uiLayer, out WindowController resultWindowController)
        {
            List<WindowController> windows = ListPool<WindowController>.Get();
            if (TryGetOpenWindowsOfLayer(uiLayer, windows))
            {
                resultWindowController = windows[0];
                ListPool<WindowController>.Release(windows);
                return true;
            }

            ListPool<WindowController>.Release(windows);
            resultWindowController = null;
            return false;
        }

        protected bool TryGetHighestOpenExclusiveWindow(out WindowController windowController)
        {
            int biggestSiblingIndex = int.MinValue;
            WindowController resultWindowController = null;
            for (int i = 0; i < allKnownWindows.Count; i++)
            {
                UIWindow uiWindow = allKnownWindows[i];

                if (!uiWindow.HasWindowInstance)
                    continue;

                if (uiWindow.Layer.Behaviour != UILayerBehaviour.Exclusive)
                    continue;

                if (!uiWindow.IsOpen())
                    continue;

                int siblingIndex = uiWindow.WindowInstance.RectTransform.GetSiblingIndex() +
                                   uiWindow.WindowInstance.RectTransform.parent.GetSiblingIndex();

                if (siblingIndex > biggestSiblingIndex)
                {
                    biggestSiblingIndex = siblingIndex;
                    resultWindowController = uiWindow.WindowInstance;
                }
            }

            windowController = resultWindowController;
            return resultWindowController != null;
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
                uiLayer = allKnownLayers[0];

            RectTransform parentLayer = GetParentForLayer(uiLayer);
            if (windowControllerInstance.RectTransform.parent != parentLayer)
                windowControllerInstance.RectTransform.SetParent(parentLayer, false);
            
            windowControllerInstance.gameObject.SetActive(false);
            windowControllerInstance.Initialize(this, uiWindow);
            uiWindow.SetWindowInstance(windowControllerInstance);

            if (!instantiatedWindows.TryAdd(uiWindow, windowControllerInstance))
                throw new Exception("This UIWindow already has a WindowController Instance assigned to it");

            DispatchWindowEvent(WindowEvent.WindowInitialized, uiWindow.WindowInstance);
        }

        private RectTransform GetParentForLayer(UILayer uiLayer)
        {
            return layerToRectTransforms[uiLayer == null ? allKnownLayers[0] : uiLayer];
        }

        private RectTransform CreateLayer(UILayer targetUILayer)
        {
            if (layerToRectTransforms.TryGetValue(targetUILayer, out RectTransform rectTransform))
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

            layerToRectTransforms.Add(targetUILayer, rectTransform);
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

                    DispatchWindowEvent(WindowEvent.BeforeWindowLoad, uiWindow);
                    asyncPrefabLoader.LoadPrefab(() =>
                    {
                        DispatchWindowEvent(WindowEvent.WindowLoaded, uiWindow);
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
            {
                if (allWindows[i].HasWindowInstance)
                    continue;

                CreateWindowInstanceForWindowID(allWindows[i]);
            }

            loadedGroups.Add(targetUIGroupToLoad);
            onLoadedCallback?.Invoke();
        }

        public async UniTask LoadGroup_Async(UIGroup targetUIGroupToLoad, CancellationToken cancellationToken = default, Action onLoadedCallback = null)
        {
            Initialize();

            List<UIWindow> allWindows = GetAllWindowsFromGroups(targetUIGroupToLoad);

            List<UniTask> loadTasks = new List<UniTask>(allWindows.Count);
            for (int i = 0; i < allWindows.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                UIWindow uiWindow = allWindows[i];
                if (uiWindow.HasWindowInstance)
                    continue;

                if (uiWindow is IAsyncPrefabLoader asyncPrefabLoader)
                {
                    if (asyncPrefabLoader.IsLoaded())
                        continue;

                    DispatchWindowEvent(WindowEvent.BeforeWindowLoad, uiWindow);

                    // Await the UniTask-based loader directly
                    loadTasks.Add(asyncPrefabLoader.LoadPrefabAsync(cancellationToken, () =>
                    {
                        DispatchWindowEvent(WindowEvent.WindowLoaded, uiWindow);
                    }));
                }
            }

            // Wait for all prefabs to load
            if (loadTasks.Count > 0)
                await UniTask.WhenAll(loadTasks);

            for (int i = 0; i < allWindows.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (allWindows[i].HasWindowInstance)
                    continue;
                
                CreateWindowInstanceForWindowID(allWindows[i]);
            }

            loadedGroups.Add(targetUIGroupToLoad);
            onLoadedCallback?.Invoke();
        }

        public void UnloadGroup(params UIGroup[] targetGroupToUnload)
        {
            Initialize();

            for (int i = 0; i < targetGroupToUnload.Length; i++)
                loadedGroups.Remove(targetGroupToUnload[i]);

            List<UIWindow> allWindows = GetAllWindowsFromGroups(targetGroupToUnload);
            for (int i = 0; i < allWindows.Count; i++)
            {
                if (IsWindowNeededByLoadedGroup(allWindows[i]))
                    continue;

                UnloadWindow(allWindows[i]);
            }
        }

        private bool IsWindowNeededByLoadedGroup(UIWindow uiWindow)
        {
            foreach (UIGroup loadedGroup in loadedGroups)
            {
                if (uiWindow.Group.Contains(loadedGroup))
                    return true;
            }
            return false;
        }

        private void UnloadWindow(UIWindow targetUIWindow)
        {
            DestroyWindowInstance(targetUIWindow);

            if (targetUIWindow is IAsyncPrefabLoader asyncPrefabLoader)
                asyncPrefabLoader.UnloadPrefab();
        }

        private void DestroyWindowInstance(UIWindow uiWindow)
        {
            if (!uiWindow.HasWindowInstance)
                return;

            if (ReferenceEquals(focusedWindowController, uiWindow.WindowInstance))
                SetFocusedWindow(null);

            if (uiWindow.IsOpen())
            {
                DispatchWindowEvent(WindowEvent.BeforeWindowClose, uiWindow);
                DispatchWindowEvent(WindowEvent.WindowClosed, uiWindow);
            }

            DispatchWindowEvent(WindowEvent.BeforeWindowDestroy, uiWindow);
            DispatchWindowEvent(WindowEvent.WindowDestroyed, uiWindow);

            WindowController targetInstance = uiWindow.WindowInstance;
            uiWindow.ClearWindowInstance();
            instantiatedWindows.Remove(uiWindow);
            Destroy(targetInstance.gameObject);
        }
        
        public void ClearHistory()
        {
            history.Clear();
        }
    }
}
