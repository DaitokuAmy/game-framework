using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// Sample用のAssetRequest基底
    /// </summary>
    public abstract class AssetRequest<T> : GameFramework.AssetSystems.AssetRequest<T> where T : Object {
#if UNITY_EDITOR
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase, (int)AssetProviderType.Addressables };
#else
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.Addressables };
#endif

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="unloadScope">解放スコープ</param>
        /// <param name="ct">読み込みキャンセル用</param>
        public async UniTask<T> LoadAsync(IScope unloadScope, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var handle = LoadAsync(Services.Resolve<AssetManager>(), unloadScope);
            await handle.ToUniTask(cancellationToken: ct);
            if (!handle.IsValid) {
                Debug.LogException(new KeyNotFoundException($"Load failed. {Address}"));
                return null;
            }

            return handle.Asset;
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected virtual string GetPath(string relativePath) {
            return $"Assets/SampleGame/{relativePath}";
        }
    }

    /// <summary>
    /// Sample用のSceneAssetRequest基底
    /// </summary>
    public abstract class SceneAssetRequest : GameFramework.AssetSystems.SceneAssetRequest {
        private LoadSceneMode _mode;
        private string _address;

        public override LoadSceneMode Mode => _mode;
#if UNITY_EDITOR
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase, (int)AssetProviderType.Addressables };
#else
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.Addressables };
#endif

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="mode">Sceneの読み込みモード</param>
        public SceneAssetRequest(LoadSceneMode mode) {
            _mode = mode;
        }

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="activate">アクティブ化するか</param>
        /// <param name="unloadScope">解放スコープ</param>
        /// <param name="ct">Taskキャンセル用Token</param>
        public async UniTask<Scene> LoadAsync(bool activate, IScope unloadScope, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            var handle = LoadAsync(Services.Resolve<AssetManager>(), unloadScope);
            if (!handle.IsValid) {
                Debug.LogException(new KeyNotFoundException($"Load failed. {Address}"));
                return default;
            }

            await handle.ToUniTask(cancellationToken: ct);

            if (handle.Exception != null) {
                Debug.LogException(handle.Exception);
                return default;
            }

            var holder = handle.Scene;
            if (activate) {
                await handle.ActivateAsync().ToUniTask(cancellationToken: ct);
                if (unloadScope != null) {
                    unloadScope.ExpiredEvent += () => {
                        SceneManager.UnloadSceneAsync(handle.Scene);
                    };
                }
            }

            return holder;
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected virtual string GetPath(string relativePath) {
            return $"Assets/SampleGame/{relativePath}";
        }
    }
    
    /// <summary>
    /// SystemAsset用のRequest基底
    /// </summary>
    public abstract class SystemAssetRequest<T> : AssetRequest<T> where T : Object {
        public override string Address { get; }

        public SystemAssetRequest(string relativePath) {
            Address = base.GetPath($"System/{relativePath}");
        }
    }

    /// <summary>
    /// UI用のAssetRequest基底
    /// </summary>
    public abstract class UIAssetRequest<T> : AssetRequest<T>
        where T : Object {
        public override string Address { get; }

        protected UIAssetRequest(string relativePath) {
            Address = base.GetPath($"UI/{relativePath}");
        }
    }

    /// <summary>
    /// UIプレファブ用のAssetRequest
    /// </summary>
    public sealed class UIPrefabAssetRequest : AssetRequest<GameObject> {
        public override string Address { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">battle</param>
        public UIPrefabAssetRequest(string assetKey) {
            Address = GetPath($"UI/Root/pfb_ui_{assetKey}.prefab");
        }
    }

    /// <summary>
    /// UIシーン用のAssetRequest
    /// </summary>
    public sealed class UISceneAssetRequest : SceneAssetRequest {
        public override string Address { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">battle</param>
        public UISceneAssetRequest(string assetKey) : base(LoadSceneMode.Additive) {
            Address = GetPath($"UI/Root/scn_ui_{assetKey}.unity");
        }
    }

    /// <summary>
    /// TableData用のAssetRequest
    /// </summary>
    public class TableDataRequest<T> : SystemAssetRequest<T>
        where T : Object {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TableDataRequest(string assetKey) : base($"Tables/dat_table_{assetKey}.asset") {
        }
    }
}