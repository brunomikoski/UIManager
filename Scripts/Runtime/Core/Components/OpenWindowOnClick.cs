using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public sealed class OpenWindowOnClick : WindowButtonBase
    {
        [SerializeField] 
        private UIWindowIndirectReference targetWindow;

        protected override void OnClick()
        {
            targetWindow.Ref.Open();
        }
    }
}
