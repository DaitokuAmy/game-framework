using System;
using GameFramework.AssetSystems;
using GameFramework.Core;
using Opera.Infrastructure.Common;
using SampleGame.Domain.Common;

namespace SampleGame.Infrastructure.Common {
    /// <summary>
    /// ユーザープレイヤー情報管理用リポジトリ
    /// </summary>
    public class PlayerMasterDataRepository : IPlayerMasterDataRepository, IDisposable {
        private DisposableScope _unloadScope;
        private AssetManager _assetManager;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerMasterDataRepository(AssetManager assetManager) {
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
        IProcess<IPlayerMasterData> IPlayerMasterDataRepository.LoadPlayer(int id) {
            return new PlayerMasterAssetRequest(id).LoadAsync(_assetManager, _unloadScope)
                .Cast<PlayerMasterData, IPlayerMasterData>();
        }
    }
}
