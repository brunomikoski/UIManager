using System.Collections;
using BrunoMikoski.ScriptableObjectCollections;

namespace BrunoMikoski.UIManager
{
    public abstract class TransitionBase : CollectableScriptableObject
    {
        public abstract IEnumerator ExecuteEnumerator(Window targetWindow);

        public virtual void BeforeTransition(Window targetWindow){}
    }
}