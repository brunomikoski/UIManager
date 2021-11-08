using System.Collections.Generic;
using System.Linq;
using BrunoMikoski.ScriptableObjectCollections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    [DisallowMultipleComponent]
    public partial class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private WindowID[] initialWindows;


        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new Dictionary<LayerID, RectTransform>();
        
        private List<WindowID> history = new List<WindowID>();
        
        private Window focusedWindow;
        
        private List<WindowID> availableWindows;
        private List<GroupID> availableGroups;
        private List<LayerID> availableLayers;
        private bool initialized;

        private void Awake()
        {
            Initialize();
            LoadInitialWindows();
        }

        private void Initialize()
        {
            if (initialized)
                return;

            availableWindows = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<WindowID>();
            availableGroups = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<GroupID>();
            availableLayers = CollectionsRegistry.Instance.GetAllCollectionItemsOfType<LayerID>();

            InitializeLayers();
            InitializeWindowIDs();
            InitializeHierarchyWindows();
            InitializeAutoLoadedGroups();
            
            initialized = true;
        }

        private void InitializeAutoLoadedGroups()
        {
            for (int i = 0; i < availableGroups.Count; i++)
            {
                GroupID groupID = availableGroups[i];
                if (!groupID.AutoLoaded) 
                    continue;
                
                List<WindowID> autoLoadedWindows = GetAllWindowsFromGroups(groupID);
                for (int j = 0; j < autoLoadedWindows.Count; j++)
                    InstantiateWindow(autoLoadedWindows[j]);
            }
        }

        private void InitializeHierarchyWindows()
        {
            Window[] hierarchyWindows = transform.GetComponentsInChildren<Window>(true);
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

                SetupWindowInstance(hierarchyWindow);
            }

            for (int i = 0; i < toBeDestroyedWindow.Count; i++)
            {
                Destroy(toBeDestroyedWindow[i].gameObject);
            }
        }

        private void InitializeLayers()
        {
            for (int i = 0; i < availableLayers.Count; i++)
                CreateLayer(availableLayers[i]);

            foreach (var layerToRectTransform in layerToRectTransforms)
            {
                layerToRectTransform.Value.SetSiblingIndex(
                    layerToRectTransform.Key.Collection.IndexOf(layerToRectTransform.Key));
            }
        }

        private void InitializeWindowIDs()
        {
            for (int i = 0; i < availableWindows.Count; i++)
                availableWindows[i].Initialize(this);
        }

        private void LoadInitialWindows()
        {
            for (int i = 0; i < initialWindows.Length; i++)
                Open(initialWindows[i]);
        }

        private void InstantiateWindow(WindowID windowID)
        {
            InstantiateWindowAsync(windowID).Forget();
        }

        private async UniTask InstantiateWindowAsync(WindowID windowID)
        {
            if (windowID.HasWindowInstance)
                return;

            await windowID.InstantiateAsync();
            SetupWindowInstance(windowID.WindowInstance);
        }

        private List<WindowID> GetAllWindowsFromGroups(params GroupID[] targetGroups)
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
        
        public void Open(Window window)
        {
            Open(window.WindowID);
        }

        public void Open(WindowID windowID)
        {
           OpenAsync(windowID).Forget();
        }

        public async UniTask OpenAsync(WindowID windowID)
        {
            Initialize();
            if (!windowID.HasWindowInstance)
                await InstantiateWindowAsync(windowID);

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
            Initialize();
            
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

        private void SetupWindowInstance(Window windowInstance)
        {
            WindowID windowID = windowInstance.WindowID;

            LayerID layerID = windowID.LayerID;
            if (layerID == null)
            {
                layerID = availableLayers[0];
                Debug.LogWarning($"WindowID {windowID} doesn't have a LayerID assignedToID, using first one available {layerID}");
            }
            
            windowID.SetWindowInstance(windowInstance);
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

            Transform layerTransform = transform.Find(targetLayer.name);

            if (layerTransform == null)
            {
                GameObject layerGO = new GameObject(targetLayer.name, typeof(RectTransform));
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

        public void LoadGroup(params GroupID[] targetGroupToLoad)
        {
            LoadGroupAsync(targetGroupToLoad).Forget();
        }

        private async UniTask LoadGroupAsync(params GroupID[] targetGroupToLoad)
        {
            Initialize();
            
            List<WindowID> allWindows = GetAllWindowsFromGroups(targetGroupToLoad);
            List<UniTask> loadingTasks = new List<UniTask>();

            for (int i = 0; i < allWindows.Count; i++)
            {
                loadingTasks.Add(InstantiateWindowAsync(allWindows[i]));
            }

            await UniTask.WhenAll(loadingTasks);
        }

        public void UnloadGroup(params GroupID[] targetGroupToUnload)
        {
            Initialize();
            UnloadGroupAsync(targetGroupToUnload).Forget();
        }

        private async UniTask UnloadGroupAsync(params GroupID[] targetGroupToUnload)
        {
            List<WindowID> allWindows = GetAllWindowsFromGroups(targetGroupToUnload);
            List<UniTask> unloadingTasks = new List<UniTask>();
            for (int i = 0; i < allWindows.Count; i++)
            {
                unloadingTasks.Add(UnloadWindowAsync(allWindows[i]));
            }

            await UniTask.WhenAll(unloadingTasks);
        }

        private async UniTask UnloadWindowAsync(WindowID windowID)
        {
            if (!windowID.HasWindowInstance)
                return;

            await windowID.DestroyAsync();
        }

    }
}
