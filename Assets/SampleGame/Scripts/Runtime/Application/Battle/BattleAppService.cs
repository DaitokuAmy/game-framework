using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Domain.Battle;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// バトル用のアプリケーション層サービス
    /// </summary>
    public class BattleAppService : IDisposable {
        private readonly BattleDomainService _battleDomainService;
        private readonly IBattleTableRepository _battleTableRepository;
        
        private DisposableScope _scope;

        /// <summary>バトルモデル</summary>
        public IReadOnlyBattleModel BattleModel => _battleDomainService.BattleModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleAppService() {
            _battleDomainService = Services.Resolve<BattleDomainService>();
            _battleTableRepository = Services.Resolve<IBattleTableRepository>();
            
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            _scope?.Dispose();
            _scope = null;
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public async UniTask SetupAsync(int battleId, int playerId, CancellationToken ct) {
            // マスターの読み込み
            await _battleTableRepository.LoadTablesAsync(ct);
            
            // マスター取得
            var battleMaster = _battleTableRepository.FindBattleById(battleId);
            var playerMaster = _battleTableRepository.FindPlayerById(playerId);
            
            // 初期化
            await _battleDomainService.SetupAsync(battleMaster, playerMaster, ct);
        }

        /// <summary>
        /// クリーンアップ
        /// </summary>
        public void Cleanup() {
            _battleDomainService.Cleanup();
        }
    }
}