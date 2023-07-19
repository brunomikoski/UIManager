namespace BrunoMikoski.UIManager
{
    public class BackOnClick : WindowButtonBase
    {
        protected override void OnClick()
        {
            ParentWindowController.WindowsManager.Back();
        }
    }
}