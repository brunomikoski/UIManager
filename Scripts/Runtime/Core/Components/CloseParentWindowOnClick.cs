namespace BrunoMikoski.UIManager
{
    public class CloseParentWindowOnClick : WindowButtonBase
    {
        protected override void OnClick()
        {
            WindowsManager.Close(ParentWindow.WindowID);
        }
    }
}
