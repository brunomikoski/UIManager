using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public sealed class OpenWindowOnClick : WindowButtonBase
    {
        [SerializeField] 
        private WindowIDIndirectReference targetWindow;

        protected override void OnClick()
        {
            targetWindow.Ref.Open();
        }
    }
}
