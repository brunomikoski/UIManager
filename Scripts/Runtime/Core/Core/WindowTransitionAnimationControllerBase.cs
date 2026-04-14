using System.Collections;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    public abstract class WindowTransitionAnimationControllerBase : MonoBehaviour
    {
        public abstract IEnumerator TransitionEnumerator();
        

        public virtual void BeforeTransitionStart(WindowController windowController)
        {
            
        }

        public virtual void AfterTransitionFinished(WindowController windowController)
        {
            
        }
    }
}
