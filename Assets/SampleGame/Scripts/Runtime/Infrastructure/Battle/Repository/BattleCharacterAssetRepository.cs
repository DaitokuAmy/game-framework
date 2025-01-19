using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Presentation.Battle;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトルキャラアセット用のリポジトリ
    /// </summary>
    public class BattleCharacterAssetRepository : IDisposable {
        private readonly SimpleAssetStorage<BattleCharacterActorSetupData> _battleCharacterActorSetupDataStorage;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterAssetRepository(AssetManager assetManager) {
            _battleCharacterActorSetupDataStorage = new SimpleAssetStorage<BattleCharacterActorSetupData>(assetManager);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _battleCharacterActorSetupDataStorage.Dispose();
        }
        
        /// <summary>
        /// ActorSetupData読み込み
        /// </summary>
        public UniTask<BattleCharacterActorSetupData> LoadSetupDataAsync(string assetKey, CancellationToken ct) {
            var request = new BattleCharacterActorSetupDataRequest(assetKey);
            return _battleCharacterActorSetupDataStorage.LoadAssetAsync(request).ToUniTask(cancellationToken: ct);
        }
    }
}