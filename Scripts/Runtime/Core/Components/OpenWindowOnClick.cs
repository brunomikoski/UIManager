using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class OpenWindowOnClick : WindowButtonBase
    {
        [SerializeField] 
        private WindowIDIndirectReference targetWindow;

        protected override void OnClick()
        {
            targetWindow.Ref.Open();
        }
    }
}
