using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using GameFramework.AssetSystems;
using ThirdPersonEngine;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトルキャラアセット用のリポジトリ
    /// </summary>
    public class BattleCharacterAssetRepository : IDisposable {
        private readonly SimpleAssetStorage<BattleCharacterActorData> _battleCharacterActorDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterAssetRepository() {
            var assetManager = Services.Resolve<AssetManager>();
            _battleCharacterActorDataStorage = new SimpleAssetStorage<BattleCharacterActorData>(assetManager);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _battleCharacterActorDataStorage.Dispose();
        }
        
        /// <summary>
        /// ActorData読み込み
        /// </summary>
        public UniTask<BattleCharacterActorData> LoadActorDataAsync(string assetKey, CancellationToken ct) {
            var request = new BattleCharacterActorDataRequest(assetKey);
            return _battleCharacterActorDataStorage.LoadAssetAsync(request).ToUniTask(cancellationToken: ct);
        }
    }
}