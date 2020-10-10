using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class OpenWindowOnClick : WindowButtonBase
    {
        [SerializeField] 
        private WindowID targetWindow;

        protected override void OnClick()
        {
            targetWindow.Open();
        }
    }
}
