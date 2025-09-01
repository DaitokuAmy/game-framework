using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.Core;
using SampleGame.Domain.Battle;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// バトル用のアプリケーション層サービス
    /// </summary>
    public class BattleAppService : IDisposable, IServiceUser {
        private DisposableScope _scope;
        
        private BattleDomainService _battleDomainService;
        private IBattleTableRepository _battleTableRepository;

        /// <summary>バトルモデル</summary>
        public IReadOnlyBattleModel BattleModel => _battleDomainService.BattleModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleAppService() {
            
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
        void IServiceUser.ImportService(IServiceResolver resolver) {
            _battleDomainService = resolver.Resolve<BattleDomainService>();
            _battleTableRepository = resolver.Resolve<IBattleTableRepository>();
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

        /// <summary>
        /// フレーム更新
        /// </summary>
        public void UpdateFrame() {
            _battleDomainService.Update();
        }
    }
}