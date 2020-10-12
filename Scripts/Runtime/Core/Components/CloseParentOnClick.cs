namespace BrunoMikoski.UIManager
{
    public class CloseParentOnClick : WindowButtonBase
    {
        protected override void OnClick()
        {
            WindowsManager.Close(ParentWindow.WindowID);
        }
    }
}
