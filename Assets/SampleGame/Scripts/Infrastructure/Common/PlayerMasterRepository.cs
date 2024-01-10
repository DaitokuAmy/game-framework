using System;
using GameFramework.AssetSystems;
using GameFramework.Core;
using Opera.Infrastructure.Common;
using SampleGame.Domain.Common;

namespace SampleGame.Infrastructure.Common {
    /// <summary>
    /// ユーザープレイヤー情報管理用リポジトリ
    /// </summary>
    public class PlayerMasterRepository : IPlayerMasterRepository, IDisposable {
        private DisposableScope _unloadScope;
        private AssetManager _assetManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerMasterRepository(AssetManager assetManager) {
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
        /// PlayerMasterの読み込み
        /// </summary>
        IProcess<IPlayerMaster> IPlayerMasterRepository.LoadPlayer(int id) {
            return new PlayerMasterAssetRequest(id).LoadAsync(_assetManager, _unloadScope)
                .Cast<PlayerMasterData, IPlayerMaster>();
        }
    }
}
