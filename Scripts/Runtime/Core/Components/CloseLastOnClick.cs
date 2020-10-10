namespace BrunoMikoski.UIManager
{
    public class CloseLastOnClick : WindowButtonBase
    {
        protected override void OnClick()
        {
            ParentWindow.WindowsManager.CloseLast();
        }

    }
}
