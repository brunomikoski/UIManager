using System.Collections.Generic;

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


        private void UpdateListener<T>(ref List<T> targetType) where T : class
        {
            if (targetType != null && cacheInterfacesInstance) 
                return;
            
            if (targetType == null)
                targetType = new List<T>();
            else
                targetType.Clear();

            if (gameObject != null)
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
                onBeforeOpenWindowListeners[i].OnBeforeWindowOpen();
        }
        
        private void DispatchOnAfterWindowOpen()
        {
            UpdateListener(ref onAfterWindowOpenListeners);

            for (int i = 0; i < onAfterWindowOpenListeners.Count; i++)
                onAfterWindowOpenListeners[i].OnWindowOpened();
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
                onAfterWindowCloseListeners[i].OnWindowClosed();
        }

        private void DispatchOnGainFocus()
        {
            UpdateListener(ref onGainFocusListeners);

            for (int i = 0; i < onGainFocusListeners.Count; i++)
                onGainFocusListeners[i].OnWindowGainedFocus();
        }

        private void DispatchOnLostFocus()
        {
            UpdateListener(ref onLostFocusListeners);

            for (int i = 0; i < onLostFocusListeners.Count; i++)
                onLostFocusListeners[i].OnWindowLostFocus();
        }
    }
}
