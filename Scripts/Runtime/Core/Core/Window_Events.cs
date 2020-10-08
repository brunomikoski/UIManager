namespace BrunoMikoski.UIManager
{
    public partial class Window
    {
        private void DispatchWindowInitialized()
        {
            IWindowInitialized[] listeners = gameObject.GetComponentsInChildren<IWindowInitialized>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnWindowInitialized();
            }
        }

        private void DispatchOnBeforeWindowOpen()
        {
            IBeforeOpenWindow[] listeners = gameObject.GetComponentsInChildren<IBeforeOpenWindow>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnBeforeOpenWindow();
            }
        }
        
        private void DispatchOnAfterWindowOpen()
        {
            IOnAfterWindowOpen[] listeners = gameObject.GetComponentsInChildren<IOnAfterWindowOpen>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnAfterWindowOpen();
            }
        }
        
        private void DispatchOnBeforeWindowClose()
        {
            IOnBeforeWindowClose[] listeners = gameObject.GetComponentsInChildren<IOnBeforeWindowClose>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnBeforeWindowClose();
            }
        }
        
        private void DispatchOnAfterWindowClose()
        {
            IOnAfterWindowClose[] listeners = gameObject.GetComponentsInChildren<IOnAfterWindowClose>(true);
            for (int i = 0; i < listeners.Length; i++)
            {
                listeners[i].OnAfterWindowClose();
            }
        }
    }
}