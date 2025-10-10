using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BrunoMikoski.UIManager
{
    public interface IAsyncPrefabLoader
    {
        // Legacy APIs (kept for backward compatibility)
        void LoadPrefab(Action loadedCallback = null);
        void UnloadPrefab();
        bool IsLoaded();

        // New UniTask-based APIs
        UniTask LoadPrefabAsync(CancellationToken cancellationToken = default, Action loadedCallback = null);
        UniTask UnloadPrefabAsync(CancellationToken cancellationToken = default);
    }
}
