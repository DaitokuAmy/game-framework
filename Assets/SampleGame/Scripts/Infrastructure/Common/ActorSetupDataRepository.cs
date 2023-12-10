using System;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using SampleGame.Domain.Common;
using SampleGame.Infrastructure.Battle;

namespace SampleGame.Infrastructure.Common {
    /// <summary>
    /// アクター初期化データ用のリポジトリ
    /// </summary>
    public class ActorSetupDataRepository : IActorSetupDataRepository, IDisposable {
        private DisposableScope _unloadScope;
        private AssetManager _assetManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ActorSetupDataRepository(AssetManager assetManager) {
            _unloadScope = new DisposableScope();
            _assetManager = assetManager;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            _assetManager = null;
            _unloadScope?.Dispose();
            _unloadScope = null;
        }
        
        /// <summary>
        /// BattleCharacterActorSetupDataの読み込み
        /// </summary>
        IProcess<BattleCharacterActorSetupData> IActorSetupDataRepository.LoadBattleCharacterActorSetupData(string assetKey) {
            return new BattleCharacterActorSetupAssetRequest(assetKey).LoadAsync(_assetManager, _unloadScope);
        }
    }
}
