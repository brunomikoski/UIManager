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
        private CollectionItemPicker<UIGroup> group = new();
        public CollectionItemPicker<UIGroup> Group => group;

        [NonSerialized]
        private WindowController windowInstance;
        public WindowController WindowInstance => windowInstance;

        public bool HasWindowInstance => windowInstance != null && windowInstance.Initialized;

        protected WindowsManager WindowsManager;

        public event Action OnInitializedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.WindowInitialized, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.WindowInitialized, this, value);
        }

        public event Action OnWillOpenEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.BeforeWindowOpen, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.BeforeWindowClose, this, value);
        }

        public event Action OnOpenedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.WindowOpened, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.WindowOpened, this, value);
        }

        public event Action OnWillCloseEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.BeforeWindowClose, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.BeforeWindowClose, this, value);
        }

        public event Action OnClosedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.WindowClosed, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.WindowClosed, this, value);
        }

        public event Action OnLostFocusEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.WindowLostFocus, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.WindowLostFocus, this, value);
        }

        public event Action OnGainFocusEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.WindowGainedFocus, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.WindowGainedFocus, this, value);
        }

        public event Action OnWillBeDestroyedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.BeforeWindowDestroy, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.BeforeWindowDestroy, this, value);
        }

        public event Action OnDestroyedEvent
        {
            add => WindowsManager.SubscribeToWindowEvent(WindowEvent.WindowDestroyed, this, value);
            remove => WindowsManager.UnsubscribeToWindowEvent(WindowEvent.WindowDestroyed, this, value);
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
