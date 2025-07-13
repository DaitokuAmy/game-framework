using System;
using GameFramework.Core;
using SampleGame.Infrastructure;
using UnityEngine;

namespace SampleGame.Domain.Battle {
    /// <summary>
    /// キャラ制御用のドメインサービス
    /// </summary>
    public class CharacterDomainService : IDisposable {
        private readonly IModelRepository _modelRepository;
        
        private DisposableScope _scope;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CharacterDomainService() {
            _modelRepository = Services.Resolve<IModelRepository>();
            
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
        /// 入力移動
        /// </summary>
        /// <param name="modelId">CharacterModelのId</param>
        /// <param name="input">入力量</param>
        public void InputMove(int modelId, Vector2 input) {
            var model = _modelRepository.GetAutoIdModel<CharacterModel>(modelId);
            if (model == null) {
                return;
            }

            // アクターに伝達
            var actorModel = model.ActorModelInternal;
            actorModel.Port.InputMove(input);
        }

        /// <summary>
        /// ジャンプ入力
        /// </summary>
        /// <param name="modelId">CharacterModelのId</param>
        public void InputJump(int modelId) {
            var model = _modelRepository.GetAutoIdModel<CharacterModel>(modelId);
            if (model == null) {
                return;
            }

            // アクターに伝達
            var actorModel = model.ActorModelInternal;
            actorModel.Port.InputJump();
        }
    }
}