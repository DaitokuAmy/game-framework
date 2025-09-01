using System;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// プレイヤー関連のアプリケーション層サービス
    /// </summary>
    public class PlayerAppService : IDisposable, IServiceUser {
        private DisposableScope _scope;
        
        private BattleDomainService _battleDomainService;
        private CharacterDomainService _characterDomainService;

        /// <summary>プレイヤーモデル</summary>
        public IReadOnlyPlayerModel PlayerModel => _battleDomainService.BattleModel.PlayerModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerAppService() {
            
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
            _characterDomainService = resolver.Resolve<CharacterDomainService>();
        }

        /// <summary>
        /// 移動入力
        /// </summary>
        public void InputMove(int playerId, Vector2 input) {
            _characterDomainService.InputMove(playerId, input);
        }

        /// <summary>
        /// 注視移動入力
        /// </summary>
        public void InputLookAt(int playerId, Vector2 input) {
            _characterDomainService.InputLookAt(playerId, input);
        }

        /// <summary>
        /// スプリント入力
        /// </summary>
        public void InputSprint(int playerId, bool sprint) {
            _characterDomainService.InputSprint(playerId, sprint);
        }

        /// <summary>
        /// ジャンプ入力
        /// </summary>
        public void InputJump(int playerId) {
            _characterDomainService.InputJump(playerId);
        }
    }
}