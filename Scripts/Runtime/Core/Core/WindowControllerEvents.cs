using System.Collections.Generic;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class WindowControllerEvents : MonoBehaviour
    {
        private List<IOnWindowInitialized> onWindowInitializedListeners; 
        private List<IOnBeforeWindowOpen> onBeforeWindowOpenListeners; 
        private List<IOnWindowOpened> onWindowOpenedListeners; 
        private List<IOnBeforeWindowClose> onBeforeWindowCloseListeners; 
        private List<IOnWindowClosed> onWindowClosedListeners; 
        private List<IOnWindowLostFocus> onLostFocusListeners;
        private List<IOnWindowGainedFocus> onGainFocusListeners; 
        private List<IOnBeforeWindowLoad> onBeforeWindowLoadListeners; 
        private List<IOnWindowLoaded> onWindowLoaded; 
        private List<IOnBeforeWindowDestroyed> onBeforeWindowBeDestroyed;


        protected virtual bool CacheInterfaces => true;

        public static void UpdateListener<T>(GameObject targetGameObject, ref List<T> targetType, bool checkForCache = true) where T : class
        {
            if (targetType != null && checkForCache) 
                return;
            
            if (targetType == null)
                targetType = new List<T>();
            else
                targetType.Clear();

            if (targetGameObject != null)
                targetGameObject.GetComponentsInChildren(true, targetType);
        }
        
        public void DispatchWindowInitialized()
        {
            UpdateListener(gameObject, ref onWindowInitializedListeners, CacheInterfaces);

            for (int i = 0; i < onWindowInitializedListeners.Count; i++)
                onWindowInitializedListeners[i].OnWindowInitialized();
        }

        public void DispatchOnBeforeWindowOpen()
        {
            UpdateListener(gameObject, ref onBeforeWindowOpenListeners, CacheInterfaces);

            for (int i = 0; i < onBeforeWindowOpenListeners.Count; i++)
                onBeforeWindowOpenListeners[i].OnBeforeWindowOpen();
        }
        
        public void DispatchWindowOpened()
        {
            UpdateListener(gameObject, ref onWindowOpenedListeners, CacheInterfaces);

            for (int i = 0; i < onWindowOpenedListeners.Count; i++)
                onWindowOpenedListeners[i].OnWindowOpened();
        }
        
        public void DispatchOnBeforeWindowClose()
        {
            UpdateListener(gameObject, ref onBeforeWindowCloseListeners, CacheInterfaces);

            for (int i = 0; i < onBeforeWindowCloseListeners.Count; i++)
                onBeforeWindowCloseListeners[i].OnBeforeWindowClose();
        }
        
        public void DispatchOnWindowClosed()
        {
            UpdateListener(gameObject, ref onWindowClosedListeners, CacheInterfaces);

            for (int i = 0; i < onWindowClosedListeners.Count; i++)
                onWindowClosedListeners[i].OnWindowClosed();
        }

        public void DispatchOnGainFocus()
        {
            UpdateListener(gameObject, ref onGainFocusListeners, CacheInterfaces);

            for (int i = 0; i < onGainFocusListeners.Count; i++)
                onGainFocusListeners[i].OnWindowGainedFocus();
        }

        public void DispatchOnLostFocus()
        {
            UpdateListener(gameObject, ref onLostFocusListeners, CacheInterfaces);

            for (int i = 0; i < onLostFocusListeners.Count; i++)
                onLostFocusListeners[i].OnWindowLostFocus();
        }
        
        public void DispatchOnBeforeWindowBeDestroyed()
        {
            UpdateListener(gameObject, ref onBeforeWindowBeDestroyed, CacheInterfaces);

            for (int i = 0; i < onBeforeWindowBeDestroyed.Count; i++)
                onBeforeWindowBeDestroyed[i].OnWindowWillBeDestroyed();
        }
        
        public void DispatchOnBeforeWindowLoad()
        {
            UpdateListener(gameObject, ref onBeforeWindowLoadListeners, CacheInterfaces);

            for (int i = 0; i < onBeforeWindowLoadListeners.Count; i++)
                onBeforeWindowLoadListeners[i].OnBeforeWindowLoad();
        }
        
        public void DispatchOnWindowLoaded()
        {
            UpdateListener(gameObject, ref onWindowLoaded, CacheInterfaces);

            for (int i = 0; i < onWindowLoaded.Count; i++)
                onWindowLoaded[i].OnWindowLoaded();
        }
    }
}
