using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.AssetSystems;
using GameFramework.Core;
using UnityEngine;

namespace SampleGame.Infrastructure {
    /// <summary>
    /// ボディプレファブアセット用のリポジトリ
    /// </summary>
    public class BodyPrefabRepository : IDisposable, IServiceUser {
        private readonly DisposableScope _scope;
        
        private SimpleAssetStorage<GameObject> _bodyPrefabAssetStorage;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BodyPrefabRepository() {
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _scope.Dispose();
        }

        /// <inheritdoc/>
        void IServiceUser.ImportService(IServiceResolver resolver) {
            var assetManager = resolver.Resolve<AssetManager>();
            _bodyPrefabAssetStorage = new SimpleAssetStorage<GameObject>(assetManager).RegisterTo(_scope);
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
