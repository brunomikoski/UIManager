using System.Collections.Generic;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public partial class WindowController
    {
        private List<IOnWindowInitialized> onWindowInitializedListeners; 
        private List<IOnBeforeWindowOpen> onBeforeOpenWindowListeners; 
        private List<IOnWindowOpened> onAfterWindowOpenListeners; 
        private List<IOnBeforeWindowClose> onBeforeWindowCloseListeners; 
        private List<IOnWindowClosed> onAfterWindowCloseListeners; 
        private List<IOnWindowGainedFocus> onGainFocusListeners; 
        private List<IOnWindowLostFocus> onLostFocusListeners;
        private List<IOnWindowWillBeDestroyed> onWindowWillBeDestroyed;


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
        
        private void DispatchWindowInitialized()
        {
            UpdateListener(gameObject, ref onWindowInitializedListeners, cacheInterfacesInstance);

            for (int i = 0; i < onWindowInitializedListeners.Count; i++)
                onWindowInitializedListeners[i].OnWindowInitialized();
        }

        private void DispatchOnBeforeWindowOpen()
        {
            UpdateListener(gameObject, ref onBeforeOpenWindowListeners, cacheInterfacesInstance);

            for (int i = 0; i < onBeforeOpenWindowListeners.Count; i++)
                onBeforeOpenWindowListeners[i].OnBeforeWindowOpen();
        }
        
        private void DispatchOnAfterWindowOpen()
        {
            UpdateListener(gameObject, ref onAfterWindowOpenListeners, cacheInterfacesInstance);

            for (int i = 0; i < onAfterWindowOpenListeners.Count; i++)
                onAfterWindowOpenListeners[i].OnWindowOpened();
        }
        
        private void DispatchOnBeforeWindowClose()
        {
            UpdateListener(gameObject, ref onBeforeWindowCloseListeners, cacheInterfacesInstance);

            for (int i = 0; i < onBeforeWindowCloseListeners.Count; i++)
                onBeforeWindowCloseListeners[i].OnBeforeWindowClose();
        }
        
        private void DispatchOnAfterWindowClose()
        {
            UpdateListener(gameObject, ref onAfterWindowCloseListeners, cacheInterfacesInstance);

            for (int i = 0; i < onAfterWindowCloseListeners.Count; i++)
                onAfterWindowCloseListeners[i].OnWindowClosed();
        }

        private void DispatchOnGainFocus()
        {
            UpdateListener(gameObject, ref onGainFocusListeners, cacheInterfacesInstance);

            for (int i = 0; i < onGainFocusListeners.Count; i++)
                onGainFocusListeners[i].OnWindowGainedFocus();
        }

        private void DispatchOnLostFocus()
        {
            UpdateListener(gameObject, ref onLostFocusListeners, cacheInterfacesInstance);

            for (int i = 0; i < onLostFocusListeners.Count; i++)
                onLostFocusListeners[i].OnWindowLostFocus();
        }
        
        private void DispatchOnWillBeDestroyed()
        {
            UpdateListener(gameObject, ref onWindowWillBeDestroyed, cacheInterfacesInstance);

            for (int i = 0; i < onWindowWillBeDestroyed.Count; i++)
                onWindowWillBeDestroyed[i].OnWindowWillBeDestroyed();
        }
    }
}
