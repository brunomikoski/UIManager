using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class UnloadGroupsOnClick : WindowButtonBase
    {
        [SerializeField]
        private GroupID[] targetGroupToUnload;

        protected override void OnClick()
        {
            WindowsManager.UnloadGroup(targetGroupToUnload);
        }
    }
}
