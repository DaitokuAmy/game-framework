using System;
using GameFramework.Core;
using SampleGame.Domain.Battle;
using UnityEngine;

namespace SampleGame.Application.Battle {
    /// <summary>
    /// プレイヤー関連のアプリケーション層サービス
    /// </summary>
    public class PlayerAppService : IDisposable {
        private readonly BattleDomainService _battleDomainService;
        private readonly CharacterDomainService _characterDomainService;
        
        private DisposableScope _scope;

        /// <summary>プレイヤーモデル</summary>
        public IReadOnlyPlayerModel PlayerModel => _battleDomainService.BattleModel.PlayerModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerAppService() {
            _battleDomainService = Services.Resolve<BattleDomainService>();
            _characterDomainService = Services.Resolve<CharacterDomainService>();
            
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
        /// 移動入力
        /// </summary>
        public void InputMove(int playerId, Vector2 input) {
            _characterDomainService.InputMove(playerId, input);
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