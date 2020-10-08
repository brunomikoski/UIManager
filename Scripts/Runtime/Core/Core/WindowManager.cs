using System.Collections.Generic;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class WindowManager : MonoBehaviour
    {
        [SerializeField]
        private WindowID initialWindowID;


        private Dictionary<LayerID, RectTransform> layerToRectTransforms = new Dictionary<LayerID, RectTransform>();
        private Dictionary<GroupID, List<WindowID>> groupToWindows = new Dictionary<GroupID, List<WindowID>>();
        private Dictionary<WindowID, Window> windowIDToWindowInstance = new Dictionary<WindowID, Window>();


        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            InitializeLayers();
            InitializeGroups();
            InitializeWindows();
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

            if (windowID.GroupID != null)
            {
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
