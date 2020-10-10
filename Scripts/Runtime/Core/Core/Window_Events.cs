using System.Collections.Generic;

namespace BrunoMikoski.UIManager
{
    public partial class Window
    {
        private List<IOnWindowInitialized> listenersWindowInitialized; 
        private List<IOnBeforeOpenWindow> listenersBeforeOpenWindow; 
        private List<IOnAfterWindowOpen> listenersOnAfterWindowOpen; 
        private List<IOnBeforeWindowClose> listenersOnBeforeWindowClose; 
        private List<IOnAfterWindowClose> listenersOnAfterWindowClose; 
        private List<IOnGainFocus> listenersOnGainFocus; 
        private List<IOnLostFocus> listenersOnLostFocus; 
        
        private void DispatchWindowInitialized()
        {
            if (cacheInterfacesInstance && listenersWindowInitialized != null)
                return;

            if (listenersWindowInitialized == null)
                listenersWindowInitialized = new List<IOnWindowInitialized>();
            
            gameObject.GetComponentsInChildren(true, listenersWindowInitialized);

            for (int i = 0; i < listenersWindowInitialized.Count; i++)
            {
                listenersWindowInitialized[i].OnWindowInitialized();
            }
        }

        private void DispatchOnBeforeWindowOpen()
        {
            if (cacheInterfacesInstance && listenersBeforeOpenWindow != null)
                return;

            if (listenersBeforeOpenWindow == null)
                listenersBeforeOpenWindow = new List<IOnBeforeOpenWindow>();
            
            gameObject.GetComponentsInChildren(true, listenersBeforeOpenWindow);

            for (int i = 0; i < listenersBeforeOpenWindow.Count; i++)
            {
                listenersBeforeOpenWindow[i].OnBeforeOpenWindow();
            }
        }
        
        private void DispatchOnAfterWindowOpen()
        {
            if (cacheInterfacesInstance && listenersOnAfterWindowOpen != null)
                return;

            if (listenersOnAfterWindowOpen == null)
                listenersOnAfterWindowOpen = new List<IOnAfterWindowOpen>();
            
            gameObject.GetComponentsInChildren(true, listenersOnAfterWindowOpen);

            for (int i = 0; i < listenersOnAfterWindowOpen.Count; i++)
            {
                listenersOnAfterWindowOpen[i].OnAfterWindowOpen();
            }
        }
        
        private void DispatchOnBeforeWindowClose()
        {
            if (cacheInterfacesInstance && listenersOnBeforeWindowClose != null)
                return;

            if (listenersOnBeforeWindowClose == null)
                listenersOnBeforeWindowClose = new List<IOnBeforeWindowClose>();

            gameObject.GetComponentsInChildren(true, listenersOnBeforeWindowClose);

            for (int i = 0; i < listenersOnBeforeWindowClose.Count; i++)
            {
                listenersOnBeforeWindowClose[i].OnBeforeWindowClose();
            }
        }
        
        private void DispatchOnAfterWindowClose()
        {
            if (cacheInterfacesInstance && listenersOnAfterWindowClose != null)
                return;
            
            if (listenersOnAfterWindowClose == null)
                listenersOnAfterWindowClose = new List<IOnAfterWindowClose>();
            
            gameObject.GetComponentsInChildren(true, listenersOnAfterWindowClose);

            for (int i = 0; i < listenersOnAfterWindowClose.Count; i++)
            {
                listenersOnAfterWindowClose[i].OnAfterWindowClose();
            }
        }

        private void DispatchOnGainFocus()
        {
            if (cacheInterfacesInstance && listenersOnGainFocus != null)
                return;
            
            if (listenersOnGainFocus == null)
                listenersOnGainFocus = new List<IOnGainFocus>();
            
            gameObject.GetComponentsInChildren(true, listenersOnGainFocus);

            for (int i = 0; i < listenersOnGainFocus.Count; i++)
            {
                listenersOnGainFocus[i].OnGainFocus();
            }
        }

        private void DispatchOnLostFocus()
        {
            if (cacheInterfacesInstance && listenersOnLostFocus != null)
                return;
            
            if (listenersOnLostFocus == null)
                listenersOnLostFocus = new List<IOnLostFocus>();
            
            gameObject.GetComponentsInChildren(true, listenersOnLostFocus);

            for (int i = 0; i < listenersOnLostFocus.Count; i++)
            {
                listenersOnLostFocus[i].OnLostFocus();
            }
        }
    }
}
