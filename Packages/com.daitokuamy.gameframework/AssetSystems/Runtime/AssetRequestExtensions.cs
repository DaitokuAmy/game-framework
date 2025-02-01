using System.Collections.Generic;
using System.Threading;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
#if USE_UNI_TASK
using Cysharp.Threading.Tasks;
#endif

namespace GameFramework.AssetSystems {
    /// <summary>
    /// AssetRequest用の拡張メソッド
    /// </summary>
    public static class AssetRequestExtensions {
#if USE_UNI_TASK
        /// <summary>
        /// AssetRequestを使ってUniTaskで読み込み
        /// </summary>
        /// <param name="source">読み込み用のRequest</param>
        /// <param name="assetManager">アセット読み込み用マネージャ</param>
        /// <param name="unloadScope">解放スコープ</param>
        /// <param name="timing">更新タイミング</param>
        /// <param name="cancellationToken">Taskキャンセル用Token</param>
        public static async UniTask<T> LoadAsync<T>(this AssetRequest<T> source, AssetManager assetManager, IScope unloadScope, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
            where T : Object {
            cancellationToken.ThrowIfCancellationRequested();

            var handle = source.LoadAsync(assetManager, unloadScope);
            if (!handle.IsValid) {
                Debug.LogException(new KeyNotFoundException($"Load failed. {source.Address}"));
                return default;
            }

            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (handle.Exception != null) {
                Debug.LogException(handle.Exception);
                return default;
            }

            return handle.Asset;
        }
        
        /// <summary>
        /// SceneAssetRequestを使ってUniTaskで読み込み
        /// </summary>
        /// <param name="source">読み込み用のRequest</param>
        /// <param name="assetManager">アセット読み込み用マネージャ</param>
        /// <param name="activate">アクティブ化するか</param>
        /// <param name="unloadScope">解放スコープ</param>
        /// <param name="timing">更新タイミング</param>
        /// <param name="cancellationToken">Taskキャンセル用Token</param>
        public static async UniTask<Scene> LoadAsync(this SceneAssetRequest source, AssetManager assetManager, bool activate, IScope unloadScope, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            var handle = source.LoadAsync(assetManager, unloadScope);
            if (!handle.IsValid) {
                Debug.LogException(new KeyNotFoundException($"Load failed. {source.Address}"));
                return default;
            }

            await handle.ToUniTask(cancellationToken: cancellationToken);

            if (handle.Exception != null) {
                Debug.LogException(handle.Exception);
                return default;
            }

            var scene = handle.Scene;
            if (activate) {
                await handle.ActivateAsync()
                    .ToUniTask(cancellationToken: cancellationToken);
            }

            return scene;
        }
#endif
    }
}