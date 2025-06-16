using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace BrunoMikoski.UIManager
{
    internal static class TaskExtensions
    {
        public static IEnumerator AsCoroutine(this Task task)
        {
            yield return new WaitUntil(() => task.IsCompleted);
            task.GetAwaiter().GetResult();
        }
    }
}
