using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Infrastructure.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SampleGame {
    /// <summary>
    /// Sample用のAssetRequest基底
    /// </summary>
    public abstract class AssetRequest<T> : GameFramework.AssetSystems.AssetRequest<T>
        where T : Object {
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase };

        /// <summary>
        /// アセットの読み込み
        /// </summary>
        /// <param name="unloadScope">解放スコープ</param>
        /// <param name="ct">読み込みキャンセル用</param>
        public UniTask<T> LoadAsync(IScope unloadScope, CancellationToken ct) {
            return this.LoadAsync(Services.Get<AssetManager>(), unloadScope, cancellationToken: ct);
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
        public override int[] ProviderIndices => new[] { (int)AssetProviderType.AssetDatabase };

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
        public UniTask<Scene> LoadAsync(bool activate, IScope unloadScope, CancellationToken ct) {
            return this.LoadAsync(Services.Get<AssetManager>(), activate, unloadScope, cancellationToken: ct);
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected string GetProjectPath(string relativePath) {
            return $"Assets/SampleGame/{relativePath}";
        }
    }

    /// <summary>
    /// BodyPrefabのAssetRequest基底
    /// </summary>
    public abstract class BodyPrefabAssetRequest : AssetRequest<GameObject> {
        public override string Address { get; }

        public BodyPrefabAssetRequest(string relativePath) {
            Address = GetPath($"BodyAssets/{relativePath}");
        }
    }

    /// <summary>
    /// ActorAssetのAssetRequest基底
    /// </summary>
    public abstract class ActorAssetRequest<T> : AssetRequest<T>
        where T : Object {
        protected sealed override string GetPath(string relativePath) {
            return base.GetPath($"ActorAssets/{relativePath}");
        }
    }

    /// <summary>
    /// GameのAssetRequest基底
    /// </summary>
    public abstract class GameAssetRequest<T> : AssetRequest<T>
        where T : Object {
        protected sealed override string GetPath(string relativePath) {
            return base.GetPath($"GameAssets/{relativePath}");
        }
    }

    /// <summary>
    /// PlayerPrefabのAssetRequest
    /// </summary>
    public class PlayerPrefabAssetRequest : BodyPrefabAssetRequest {
        public PlayerPrefabAssetRequest(string assetKey)
            : base($"Player/{assetKey}/Models/prfb_{assetKey}.prefab") {
        }
    }

    /// <summary>
    /// BattleCharacterActorSetupData用のAssetRequest
    /// </summary>
    public class BattleCharacterActorSetupDataAssetRequest : ActorAssetRequest<BattleCharacterActorSetupData> {
        public override string Address { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActorSetupDataAssetRequest(string assetKey) {
            var actorId = assetKey.Substring(0, "pl000".Length);
            Address = GetPath($"Battle/{actorId}/dat_battle_character_actor_setup_{assetKey}.asset");
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
    public class UISceneAssetRequest : SceneAssetRequest {
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
    /// フィールドシーン用のAssetRequest
    /// </summary>
    public class FieldSceneAssetRequest : SceneAssetRequest {
        // 読み込みAddress
        public override string Address { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">fld000</param>
        public FieldSceneAssetRequest(string assetKey) : base(LoadSceneMode.Additive) {
            Address = GetProjectPath($"EnvironmentAssets/Field/{assetKey}/{assetKey}.unity");
        }
    }
}