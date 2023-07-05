using System;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public  class WindowID : ScriptableObjectCollectionItem
    {
        [SerializeField]
        private LayerID layerID;
        public LayerID LayerID => layerID;

        [SerializeField]
        private GroupID groupID;
        public GroupID GroupID => groupID;

        [NonSerialized]
        protected Window windowInstance;
        public Window WindowInstance => windowInstance;

       private WindowsManager windowsManager;
       
        public bool HasWindowInstance
        {
            get
            {
                if (windowInstance == null)
                    return false;
                return windowInstance.Initialized;
            }
        }

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
        
        public void Initialize(WindowsManager targetWindowsManager)
        {
            windowsManager = targetWindowsManager;
        }
        
        public void SetWindowInstance(Window targetWindowInstance)
        {
            windowInstance = targetWindowInstance;
        }

        public virtual Window GetWindowPrefab()
        {
            return windowInstance;
        }
    }
}
