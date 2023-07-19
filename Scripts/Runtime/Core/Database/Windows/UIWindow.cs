using System;
using System.Collections;
using BrunoMikoski.ScriptableObjectCollections;
using BrunoMikoski.ScriptableObjectCollections.Picker;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public  class UIWindow : ScriptableObjectCollectionItem
    {
        [SerializeField]
        private UILayer layer;
        public UILayer Layer => layer;

        [SerializeField]
        private CollectionItemPicker<UIGroup> group;
        public CollectionItemPicker<UIGroup> Group => group;

        [NonSerialized]
        private WindowController windowInstance;
        public WindowController WindowInstance => windowInstance;

        public bool HasWindowInstance => windowInstance != null && windowInstance.Initialized;

        protected WindowsManager WindowsManager;

        public event Action OnInitializedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnWindowInitialized, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWindowInitialized, this, value);
        }

        public event Action OnWillOpenEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnWillOpen, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWillClose, this, value);
        }

        public event Action OnOpenedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnOpened, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnOpened, this, value);
        }

        public event Action OnWillCloseEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnWillClose, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWillClose, this, value);
        }

        public event Action OnClosedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnClosed, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnClosed, this, value);
        }

        public event Action OnLostFocusEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnLostFocus, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnLostFocus, this, value);
        }

        public event Action OnGainFocusEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnGainFocus, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnGainFocus, this, value);
        }

        public event Action OnWillBeDestroyedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnWillBeDestroyed, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWillBeDestroyed, this, value);
        }

        public event Action OnDestroyedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.OnDestroyed, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.OnDestroyed, this, value);
        }

        public void Initialize(WindowsManager targetWindowsManager)
        {
            WindowsManager = targetWindowsManager;
        }
        
        public bool IsOpen()
        {
            if (!HasWindowInstance)
                return false;

            return windowInstance.IsOpen;
        }

        public void Open()
        {
            WindowsManager.Open(this);
        }

        public IEnumerator OpenEnumerator()
        {
            yield return WindowsManager.OpenEnumerator(this);
        }
        
        public void Close()
        {
            WindowsManager.Close(this);
        }

        public IEnumerator CloseEnumerator()
        {
            yield return WindowsManager.CloseEnumerator(this);
        }
        
        public void SetWindowInstance(WindowController targetWindowControllerInstance)
        {
            windowInstance = targetWindowControllerInstance;
        }
        
        public void ClearWindowInstance()
        {
            windowInstance = null;
        }

        public virtual WindowController GetWindowPrefab()
        {
            return windowInstance;
        }
        
#if UNITY_EDITOR        
        private void OnEnable()
        {
            if (layer == null)
            {
                if (CollectionsRegistry.Instance.TryGetCollectionOfType(out UILayerCollection layerIDs))
                {
                    layer = layerIDs[0];
                    ObjectUtility.SetDirty(this);
                }
            }

            if (group == null)
                group = new CollectionItemPicker<UIGroup>();
            
            if (group.Count == 0)
            {
                if (CollectionsRegistry.Instance.TryGetCollectionOfType(out UIGroupCollection groupIDs))
                {
                    group.Add(groupIDs[0]);
                    ObjectUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}
