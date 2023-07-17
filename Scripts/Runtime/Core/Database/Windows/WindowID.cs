using System;
using System.Collections;
using BrunoMikoski.ScriptableObjectCollections;
using BrunoMikoski.ScriptableObjectCollections.Picker;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public  class WindowID : ScriptableObjectCollectionItem
    {
        [SerializeField]
        private LayerID layerID;
        public LayerID LayerID => layerID;

        [SerializeField]
        private CollectionItemPicker<GroupID> group;
        public CollectionItemPicker<GroupID> Group => group;

        [NonSerialized]
        protected Window windowInstance;
        public Window WindowInstance => windowInstance;

        protected WindowsManager windowsManager;
       
       
       public event Action OnInitializedEvent
       {
           add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnWindowInitialized, this, value);
           remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWindowInitialized, this, value);
       }
       
        public event Action OnWillOpenEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnWillOpen, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWillClose, this, value);
        }  
       
        public event Action OnOpenedEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnOpened, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnOpened, this, value);
        }  
        
        public event Action OnWillCloseEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnWillClose, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWillClose, this, value);
        }
        
        public event Action OnClosedEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnClosed, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnClosed, this, value);
        }
        
        public event Action OnLostFocusEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnLostFocus, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnLostFocus, this, value);
        }
        
        public event Action OnGainFocusEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnGainFocus, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnGainFocus, this, value);
        }
        
        public event Action OnWillBeDestroyedEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnWillBeDestroyed, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnWillBeDestroyed, this, value);
        }
        
        public event Action OnDestroyedEvent
        {
            add => windowsManager.SubscribeToWindowEvent(WindowEvent.OnDestroyed, this, value);
            remove => windowsManager.UnsubscribeToWindowEvent(WindowEvent.OnDestroyed, this, value);
        }

        public bool HasWindowInstance => windowInstance != null;

        public bool IsOpen()
        {
            if (!HasWindowInstance)
                return false;

            return windowInstance.IsOpen;
        }

        public void Open()
        {
            windowsManager.Open(this);
        }

        public IEnumerator OpenEnumerator()
        {
            yield return windowsManager.OpenEnumerator(this);
        }
        
        public void Close()
        {
            windowsManager.Close(this);
        }

        public IEnumerator CloseEnumerator()
        {
            yield return windowsManager.CloseEnumerator(this);
        }
        
        public void Initialize(WindowsManager targetWindowsManager)
        {
            windowsManager = targetWindowsManager;
        }
        
        public void SetWindowInstance(Window targetWindowInstance)
        {
            windowInstance = targetWindowInstance;
        }
        
        public void ClearWindowInstance()
        {
            windowInstance = null;
        }

        public virtual Window GetWindowPrefab()
        {
            return windowInstance;
        }
        
#if UNITY_EDITOR        
        private void OnEnable()
        {
            if (layerID == null)
            {
                if (CollectionsRegistry.Instance.TryGetCollectionOfType(out LayerIDs layerIDs))
                {
                    layerID = layerIDs[0];
                    ObjectUtility.SetDirty(this);
                }
            }

            if (group == null)
                group = new CollectionItemPicker<GroupID>();
            
            if (group.Count == 0)
            {
                if (CollectionsRegistry.Instance.TryGetCollectionOfType(out GroupIDs groupIDs))
                {
                    group.Add(groupIDs[0]);
                    ObjectUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}
