using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public sealed class OpenWindowOnClick : WindowButtonBase
    {
        [SerializeField] 
        private WindowIDIndirectReference targetWindow;

        protected override void OnClick()
        {
            if (!targetWindow.IsValid())
            {
                Debug.LogError($"{nameof(targetWindow)} is not defined at {name}", this);
                return;
            }
            
            targetWindow.Ref.Open();
        }
    }
}
