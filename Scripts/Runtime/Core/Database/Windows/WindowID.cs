using System;
using System.Collections;
using BrunoMikoski.ScriptableObjectCollections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public abstract class WindowID : ScriptableObjectCollectionItem
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

        public void Open()
        {
            this.windowsManager.Open(this);
        }
        
        public void Initialize(WindowsManager targetWindowsManager)
        {
            this.windowsManager = targetWindowsManager;
        }

        public abstract IEnumerator InstantiateEnumerator(WindowsManager windowsManager);
        public abstract IEnumerator DestroyEnumerator();


        public void SetWindowInstance(Window targetWindowInstance)
        {
            windowInstance = targetWindowInstance;
        }
    }
}
