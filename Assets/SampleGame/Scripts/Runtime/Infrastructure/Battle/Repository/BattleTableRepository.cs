using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.AssetSystems;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using Object = UnityEngine.Object;

namespace SampleGame.Infrastructure.Battle {
    /// <summary>
    /// バトル用テーブルデータ管理クラス
    /// </summary>
    public partial class BattleTableRepository : IDisposable, IBattleTableRepository {
        private readonly AssetManager _assetManager;
        
        private DisposableScope _scope;
        private BattleTableData _battleTableData;
        private BattleFieldTableData _battleFieldTableData;
        private BattlePlayerTableData _battlePlayerTableData;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleTableRepository() {
            _assetManager = Services.Resolve<AssetManager>();
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _scope?.Dispose();
            _scope = null;
        }

        /// <inheritdoc/>
        UniTask IBattleTableRepository.LoadTablesAsync(CancellationToken ct) {
            return UniTask.WhenAll(
                LoadTableAsync<BattleTableData>("battle", ct)
                    .ContinueWith(x => _battleTableData = x),
                LoadTableAsync<BattleFieldTableData>("battle_field", ct)
                    .ContinueWith(x => _battleFieldTableData = x),
                LoadTableAsync<BattlePlayerTableData>("battle_player", ct)
                    .ContinueWith(x => _battlePlayerTableData = x)
            );
        }

        /// <inheritdoc/>
        IBattleMaster IBattleTableRepository.FindBattleById(int id) {
            return CreateBattleMaster(_battleTableData.FindById(id));
        }

        /// <inheritdoc/>
        IPlayerMaster IBattleTableRepository.FindPlayerById(int id) {
            return _battlePlayerTableData.FindById(id);
        }

        /// <summary>
        /// テーブルデータの読み込み
        /// </summary>
        private UniTask<T> LoadTableAsync<T>(string assetKey, CancellationToken ct)
            where T : Object {
            return new TableDataRequest<T>(assetKey).LoadAsync(_assetManager, _scope, cancellationToken:ct);
        }
    }
}