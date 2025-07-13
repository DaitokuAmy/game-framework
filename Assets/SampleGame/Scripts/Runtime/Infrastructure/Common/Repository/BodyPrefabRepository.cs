using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// ボディプレファブアセット用のリポジトリ
    /// </summary>
    public class BodyPrefabRepository : IDisposable {
        private readonly SimpleAssetStorage<GameObject> _bodyPrefabAssetStorage;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BodyPrefabRepository() {
            var assetManager = Services.Resolve<AssetManager>();
            _bodyPrefabAssetStorage = new SimpleAssetStorage<GameObject>(assetManager);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _bodyPrefabAssetStorage.Dispose();
        }

        /// <summary>
        /// キャラプレファブの読み込み
        /// </summary>
        public UniTask<GameObject> LoadCharacterPrefabAsync(string assetKey, CancellationToken ct) {
            var request = new CharacterPrefabAssetRequest(assetKey);
            var handle = _bodyPrefabAssetStorage.LoadAssetAsync(request);
            return handle.ToUniTask(cancellationToken:ct);
        }

        /// <summary>
        /// キャラプレファブのアンロード
        /// </summary>
        public void UnloadCharacterPrefabScene(string assetKey) {
            var request = new CharacterPrefabAssetRequest(assetKey);
            _bodyPrefabAssetStorage.UnloadAsset(request.Address);
        }
    }
}
