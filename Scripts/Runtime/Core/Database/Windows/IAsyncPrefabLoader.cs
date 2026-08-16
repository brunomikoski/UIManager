using System;
#if UNITASK_AVAILABLE
using System.Threading;
using Cysharp.Threading.Tasks;
#endif

namespace BrunoMikoski.UIManager
{
    public interface IAsyncPrefabLoader
    {
        void LoadPrefab(Action loadedCallback = null);
        void UnloadPrefab();
        bool IsLoaded();

#if UNITASK_AVAILABLE
        UniTask LoadPrefabAsync(CancellationToken cancellationToken = default, Action loadedCallback = null);
        UniTask UnloadPrefabAsync(CancellationToken cancellationToken = default);
#endif
    }
}
