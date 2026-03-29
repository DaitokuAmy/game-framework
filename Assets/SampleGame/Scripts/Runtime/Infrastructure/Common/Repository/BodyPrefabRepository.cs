using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystem;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// ボディプレファブアセット用のリポジトリ
    /// </summary>
    public sealed class BodyPrefabRepository : System.IDisposable {
        private readonly SimpleAssetStorage _bodyPrefabAssetStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BodyPrefabRepository() {
            _bodyPrefabAssetStorage = new SimpleAssetStorage(AssetUtility.CreateAssetLoaders());
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public void Dispose() {
            _bodyPrefabAssetStorage.Dispose();
        }

        /// <summary>
        /// キャラプレファブの読み込み
        /// </summary>
        public UniTask<GameObject> LoadCharacterPrefabAsync(string assetKey, CancellationToken ct) {
            return _bodyPrefabAssetStorage
                .LoadAsync<GameObject, CharacterPrefabAssetRequest>(new CharacterPrefabAssetRequest(assetKey))
                .ToUniTask<GameObject>(cancellationToken: ct);
        }

        /// <summary>
        /// キャラプレファブのアンロード
        /// </summary>
        public void UnloadCharacterPrefabScene(string assetKey) {
            _bodyPrefabAssetStorage.Unload<GameObject, CharacterPrefabAssetRequest>(new CharacterPrefabAssetRequest(assetKey));
        }
    }
}
