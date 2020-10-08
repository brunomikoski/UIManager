using System.Collections;
using System.Collections.Generic;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;
using UnityEngine.UI;

namespace BrunoMikoski.UIManager
{
    [RequireComponent(typeof(Canvas))]
    public class WindowsManager : MonoBehaviour
    {
        [SerializeField]
        private WindowID initialWindowID;


        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new Dictionary<LayerID, RectTransform>();
        private Dictionary<LayerID, List<WindowID>> layerToWindows = new Dictionary<LayerID, List<WindowID>>();
        private Dictionary<GroupID, List<WindowID>> groupToWindows = new Dictionary<GroupID, List<WindowID>>();
        private Dictionary<WindowID, Window> windowIDToWindowInstance = new Dictionary<WindowID, Window>();

        
        private List<WindowID> history = new List<WindowID>();

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            InitializeLayers();
            InitializeGroups();
            InitializeWindows();

            if (initialWindowID != null)
                Open(initialWindowID);
        }

        public void Open(WindowID windowID)
        {
            if (IsWindowOpen(windowID))
                return;
            
            StartCoroutine(OpenEnumerator(windowID));
        }

        private bool IsWindowOpen(WindowID windowID)
        {
            if (windowIDToWindowInstance.TryGetValue(windowID, out Window window))
                return window.IsOpen;
            return false;
        }

        private IEnumerator OpenEnumerator(WindowID windowID)
        {
            LayerID windowLayer = windowID.LayerID;
            if (windowLayer.Behaviour == LayerBehaviour.Exclusive)
            {
                if (TryGetOpenWindowsOfLayer(windowLayer, out List<Window> openWindows))
                {
                    for (int i = 0; i < openWindows.Count; i++)
                    {
                        openWindows[i].Close();
                        history.Add(openWindows[i].WindowID);
                    }
                }
            }

            if (windowIDToWindowInstance.TryGetValue(windowID, out Window windowInstance))
            {
                windowInstance.Open();
            }
            
            yield break;
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

        private bool TryGetOpenWindowOfLayer(LayerID layerID, out Window resultWindow)
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
                if (windows[i] is PrefabBasedWindowID prefabBasedWindow)
                {
                    if (!prefabBasedWindow.InstantiateOnInitialization)
                        continue;

                    InitializeWindow(prefabBasedWindow);
                }
            }
        }

        private void InitializeWindow(PrefabBasedWindowID windowID)
        {
            if (windowIDToWindowInstance.ContainsKey(windowID))
                return;
            
            RectTransform targetLayer = GetParentForLayer(windowID.LayerID);

            Window windowInstance = Instantiate(windowID.WindowPrefab, targetLayer, false);
            windowInstance.Initialize(this, windowID);
            windowIDToWindowInstance.Add(windowID, windowInstance);

            windowInstance.gameObject.SetActive(false);


            if (!layerToWindows.ContainsKey(windowID.LayerID))
                layerToWindows.Add(windowID.LayerID, new List<WindowID>());

            layerToWindows[windowID.LayerID].Add(windowID);
            
            
            windowID.SetWindowsManager(this);
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
