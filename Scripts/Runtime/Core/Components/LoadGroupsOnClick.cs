using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class LoadGroupsOnClick : WindowButtonBase
    {
        [SerializeField]
        private UIGroup[] targetGroupToLoad;
        
        protected override void OnClick()
        {
            foreach (UIGroup groupID in targetGroupToLoad)
            {
                WindowsManager.LoadGroup(groupID);
            }
        }
    }
}
