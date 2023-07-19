using System.Collections.Generic;

namespace BrunoMikoski.UIManager
{
    public partial class WindowController
    {
        private List<IOnWindowInitialized> onWindowInitializedListeners; 
        private List<IOnBeforeOpenWindow> onBeforeOpenWindowListeners; 
        private List<IOnAfterWindowOpen> onAfterWindowOpenListeners; 
        private List<IOnBeforeWindowClose> onBeforeWindowCloseListeners; 
        private List<IOnAfterWindowClose> onAfterWindowCloseListeners; 
        private List<IOnGainFocus> onGainFocusListeners; 
        private List<IOnLostFocus> onLostFocusListeners;


        private void UpdateListener<T>(ref List<T> targetType) where T : class
        {
            if (targetType != null && cacheInterfacesInstance) 
                return;
            
            if (targetType == null)
                targetType = new List<T>();
            else
                targetType.Clear();
            
            gameObject.GetComponentsInChildren(true, targetType);
        }
        
        private void DispatchWindowInitialized()
        {
            UpdateListener(ref onWindowInitializedListeners);

            for (int i = 0; i < onWindowInitializedListeners.Count; i++)
                onWindowInitializedListeners[i].OnWindowInitialized();
        }

        private void DispatchOnBeforeWindowOpen()
        {
            UpdateListener(ref onBeforeOpenWindowListeners);

            for (int i = 0; i < onBeforeOpenWindowListeners.Count; i++)
                onBeforeOpenWindowListeners[i].OnBeforeOpenWindow();
        }
        
        private void DispatchOnAfterWindowOpen()
        {
            UpdateListener(ref onAfterWindowOpenListeners);

            for (int i = 0; i < onAfterWindowOpenListeners.Count; i++)
                onAfterWindowOpenListeners[i].OnAfterWindowOpen();
        }
        
        private void DispatchOnBeforeWindowClose()
        {
            UpdateListener(ref onBeforeWindowCloseListeners);

            for (int i = 0; i < onBeforeWindowCloseListeners.Count; i++)
                onBeforeWindowCloseListeners[i].OnBeforeWindowClose();
        }
        
        private void DispatchOnAfterWindowClose()
        {
            UpdateListener(ref onAfterWindowCloseListeners);

            for (int i = 0; i < onAfterWindowCloseListeners.Count; i++)
                onAfterWindowCloseListeners[i].OnAfterWindowClose();
        }

        private void DispatchOnGainFocus()
        {
            UpdateListener(ref onGainFocusListeners);

            for (int i = 0; i < onGainFocusListeners.Count; i++)
                onGainFocusListeners[i].OnGainFocus();
        }

        private void DispatchOnLostFocus()
        {
            UpdateListener(ref onLostFocusListeners);

            for (int i = 0; i < onLostFocusListeners.Count; i++)
                onLostFocusListeners[i].OnLostFocus();
        }
    }
}
