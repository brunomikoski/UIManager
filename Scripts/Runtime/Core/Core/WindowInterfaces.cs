namespace BrunoMikoski.UIManager
{
    public interface IWindowInitialized
    {
        void OnWindowInitialized();
    }

    public interface IBeforeOpenWindow
    {
        void OnBeforeOpenWindow();
    }

    public interface IOnAfterWindowOpen
    {
        void OnAfterWindowOpen();
    }
    
    public interface IOnBeforeWindowClose
    {
        void OnBeforeWindowClose();
    }
    
    public interface IOnAfterWindowClose
    {
        void OnAfterWindowClose();
    }
}
