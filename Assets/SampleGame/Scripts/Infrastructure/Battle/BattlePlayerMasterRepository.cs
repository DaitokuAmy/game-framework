using System;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Application.Battle;
using SampleGame.Domain.Battle;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// ユーザープレイヤー情報管理用リポジトリ
    /// </summary>
    public class BattlePlayerMasterRepository : IBattlePlayerMasterRepository, IDisposable {
        private DisposableScope _unloadScope;
        private AssetManager _assetManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattlePlayerMasterRepository(AssetManager assetManager) {
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
        /// BattlePlayerMasterの読み込み
        /// </summary>
        IProcess<IBattlePlayerMaster> IBattlePlayerMasterRepository.LoadPlayer(int id) {
            return new BattlePlayerMasterAssetRequest(id).LoadAsync(_assetManager, _unloadScope)
                .Cast<BattlePlayerMasterData, IBattlePlayerMaster>();
        }
    }
}
