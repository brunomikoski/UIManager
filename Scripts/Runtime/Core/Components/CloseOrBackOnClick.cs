namespace BrunoMikoski.UIManager
{
    public class CloseOrBackOnClick : WindowButtonBase
    {
        protected override void OnClick()
        {
            if(ParentWindowController.UIWindow.Layer.IncludedOnHistory)
                ParentWindowController.Back();
            else
                ParentWindowController.Close();
        }
    }
}
