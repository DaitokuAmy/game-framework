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

            var handle = LoadAsync(Services.Get<AssetManager>(), unloadScope);
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
        /// <param name="path">読み込みパス</param>
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

            var handle = LoadAsync(Services.Get<AssetManager>(), unloadScope);
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
                    unloadScope.OnExpired += () => {
                        SceneManager.UnloadSceneAsync(handle.Scene);
                    };
                }
            }

            return holder;
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected string GetProjectPath(string relativePath) {
            return $"Assets/SampleGame/{relativePath}";
        }
    }

    /// <summary>
    /// 加算シーン用のAssetRequest基底
    /// </summary>
    public abstract class AdditiveSceneAssetRequest : SceneAssetRequest {
        public override string Address { get; }

        public AdditiveSceneAssetRequest(string relativePath) : base(LoadSceneMode.Additive) {
            Address = GetProjectPath($"EnvironmentAssets/{relativePath}");
        }
    }

    /// <summary>
    /// BodyPrefabのAssetRequest基底
    /// </summary>
    public abstract class BodyPrefabAssetRequest : AssetRequest<GameObject> {
        public override string Address { get; }

        public BodyPrefabAssetRequest(string relativePath) {
            Address = base.GetPath($"BodyAssets/{relativePath}");
        }
    }

    /// <summary>
    /// ActorAssetのAssetRequest基底
    /// </summary>
    public abstract class ActorAssetRequest<T> : AssetRequest<T> where T : Object {
        public override string Address { get; }

        public ActorAssetRequest(string relativePath) {
            Address = base.GetPath($"ActorAssets/{relativePath}");
        }
    }

    /// <summary>
    /// GameAssets用のRequest基底
    /// </summary>
    public abstract class GameAssetRequest<T> : AssetRequest<T> where T : Object {
        public override string Address { get; }

        public GameAssetRequest(string relativePath) {
            Address = base.GetPath($"GameAssets/{relativePath}");
        }
    }

    /// <summary>
    /// UI用のAssetRequest基底
    /// </summary>
    public abstract class UIAssetRequest<T> : AssetRequest<T>
        where T : Object {
        public override string Address { get; }

        protected UIAssetRequest(string relativePath) {
            Address = base.GetPath($"UIAssets/Dynamic/{relativePath}");
        }
    }

    /// <summary>
    /// UIプレファブ用のAssetRequest
    /// </summary>
    public sealed class UIPrefabAssetRequest : AssetRequest<GameObject> {
        // 読み込みAddress
        public override string Address { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">ui_battle</param>
        public UIPrefabAssetRequest(string assetKey) {
            Address = GetPath($"UIAssets/Dynamic/Prefabs/prfb_{assetKey}.prefab");
        }
    }

    /// <summary>
    /// UIシーン用のAssetRequest
    /// </summary>
    public sealed class UISceneAssetRequest : SceneAssetRequest {
        // 読み込みAddress
        public override string Address { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">ui_battle</param>
        public UISceneAssetRequest(string assetKey) : base(LoadSceneMode.Additive) {
            Address = GetProjectPath($"UIAssets/Dynamic/Scenes/{assetKey}.unity");
        }
    }

    /// <summary>
    /// TableData用のAssetRequest
    /// ※Commonに移動予定
    /// </summary>
    public class TableDataRequest<T> : GameAssetRequest<T>
        where T : Object {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TableDataRequest(string assetKey) : base($"Tables/dat_table_{assetKey}.asset") {
        }
    }

    /// <summary>
    /// キャラプレファブ(ch000_00)用のAssetRequest
    /// </summary>
    public class CharacterPrefabAssetRequest : BodyPrefabAssetRequest {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">ch000_00</param>
        public CharacterPrefabAssetRequest(string assetKey) : base($"Character/{GetBoneId(assetKey)}/prfb_{assetKey}.prefab") {
        }

        /// <summary>
        /// 骨IDの取得
        /// </summary>
        private static string GetBoneId(string assetKey) {
            return assetKey.Split("_")[0];
        }
    }

    /// <summary>
    /// フィールドシーン用のAssetRequest
    /// </summary>
    public class FieldSceneAssetRequest : AdditiveSceneAssetRequest {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">fld000</param>
        public FieldSceneAssetRequest(string assetKey) : base($"Field/{assetKey}/{assetKey}.unity") {
        }
    }
}