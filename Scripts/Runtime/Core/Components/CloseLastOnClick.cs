namespace BrunoMikoski.UIManager
{
    public class CloseLastOnClick : WindowButtonBase
    {
        protected override void OnClick()
        {
            ParentWindowController.WindowsManager.CloseLastOpenWindow();
        }

    }
}
