using GameFramework.ActorSystems;
using GameFramework.Core;
using SampleGame.Application.Battle;
using SampleGame.Domain.Battle;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SampleGame.Presentation.Battle {
    /// <summary>
    /// Player制御用コントローラー
    /// </summary>
    public class PlayerInputController : ActorEntityLogic {
        private readonly PlayerAppService _playerAppService;
        private readonly IReadOnlyCharacterModel _model;
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAtAction;
        private readonly InputAction _jumpAction;
        private readonly InputAction _attackAction;
        private readonly InputAction _sprintAction;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlayerInputController(IReadOnlyCharacterModel model) {
            _playerAppService = Services.Resolve<PlayerAppService>();
            
            _model = model;

            var playerInput = Services.Resolve<PlayerInput>();
            _moveAction = playerInput.actions["Move"];
            _lookAtAction = playerInput.actions["LookAt"];
            _jumpAction = playerInput.actions["Jump"];
            _attackAction = playerInput.actions["Attack"];
            _sprintAction = playerInput.actions["Sprint"];
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            _jumpAction.performed += OnJump;

            scope.ExpiredEvent += () => { _jumpAction.performed -= OnJump; };
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();
            
            // 移動入力
            _playerAppService.InputMove(_model.Id, _moveAction.ReadValue<Vector2>());
            _playerAppService.InputSprint(_model.Id, _sprintAction.IsPressed());
            
            // 注視移動入力
            _playerAppService.InputLookAt(_model.Id, _lookAtAction.ReadValue<Vector2>());
        }

        /// <summary>
        /// ジャンプ入力時
        /// </summary>
        private void OnJump(InputAction.CallbackContext context) {
            _playerAppService.InputJump(_model.Id);
        }
    }
}