using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// Environment用のAssetRequest基底
    /// </summary>
    public abstract class EnvironmentSceneAssetRequest : SceneAssetRequest {
        public override string Address { get; }

        public EnvironmentSceneAssetRequest(string relativePath) : base(LoadSceneMode.Additive) {
            Address = GetPath($"Environment/{relativePath}");
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected override string GetPath(string relativePath) {
            return $"Assets/SampleGame/RemoteAssets/{relativePath}";
        }
    }

    /// <summary>
    /// ActorAssetのAssetRequest基底
    /// </summary>
    public abstract class ActorAssetRequest<T> : AssetRequest<T> where T : Object {
        public override string Address { get; }

        public ActorAssetRequest(string relativePath) {
            Address = GetPath($"Actor/{relativePath}");
        }

        /// <summary>
        /// Projectフォルダ相対パスを絶対パスにする
        /// </summary>
        protected override string GetPath(string relativePath) {
            return $"Assets/SampleGame/RemoteAssets/{relativePath}";
        }
    }

    /// <summary>
    /// キャラプレファブ(ch000_00)用のAssetRequest
    /// </summary>
    public class CharacterPrefabAssetRequest : ActorAssetRequest<GameObject> {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">ch000_00</param>
        public CharacterPrefabAssetRequest(string assetKey) : base($"Character/{GetBoneId(assetKey)}/prfb_act_{assetKey}.prefab") {
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
    public class FieldSceneAssetRequest : EnvironmentSceneAssetRequest {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetKey">fld000</param>
        public FieldSceneAssetRequest(string assetKey) : base($"Field/{assetKey}/scn_env_{assetKey}.unity") {
        }
    }
}