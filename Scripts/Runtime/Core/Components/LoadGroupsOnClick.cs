using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public class LoadGroupsOnClick : WindowButtonBase
    {
        [SerializeField]
        private GroupID[] targetGroupToLoad;
        
        protected override void OnClick()
        {
            foreach (GroupID groupID in targetGroupToLoad)
            {
                WindowsManager.LoadGroup(groupID);
            }
        }
    }
}
