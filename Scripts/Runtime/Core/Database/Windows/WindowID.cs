using System;
using System.Collections;
using BrunoMikoski.ScriptableObjectCollections;
using BrunoMikoski.ScriptableObjectCollections.Picker;
using UnityEngine;
using UnityEngine.Serialization;

namespace BrunoMikoski.UIManager
{
    public  class WindowID : ScriptableObjectCollectionItem
    {
        [SerializeField]
        private LayerID layerID;
        public LayerID LayerID => layerID;

        [FormerlySerializedAs("groupID")]
        [SerializeField]
        private CollectionItemPicker<GroupID> group;
        public CollectionItemPicker<GroupID> Group => group;

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

        public IEnumerator OpenEnumerator()
        {
            yield return windowsManager.OpenEnumerator(this);
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

    }
}
