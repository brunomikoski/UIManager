namespace BrunoMikoski.UIManager
{
    public interface IOnWindowInitialized
    {
        void OnWindowInitialized();
    }

    public interface IOnBeforeWindowOpen
    {
        void OnBeforeWindowOpen();
    }

    public interface IOnWindowOpened
    {
        void OnWindowOpened();
    }
    
    public interface IOnBeforeWindowClose
    {
        void OnBeforeWindowClose();
    }
    
    public interface IOnWindowClosed
    {
        void OnWindowClosed();
    }
    
    public interface IOnWindowGainedFocus
    {
        void OnWindowGainedFocus();
    }
    
    public interface IOnWindowLostFocus
    {
        void OnWindowLostFocus();
    }
}
