using System;
using GameFramework.TaskSystems;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SampleGame.Battle {
    /// <summary>
    /// バトル用入力管理
    /// </summary>
    public class BattleInput : TaskBehaviour {
        [SerializeField, Tooltip("入力ソース")]
        private PlayerInput _playerInput;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;

        private Subject<Unit> _jumpSubject = new Subject<Unit>();
        private Subject<Unit> _attackSubject = new Subject<Unit>();

        // 移動ベクトル
        public Vector2 MoveVector { get; private set; }
        // ジャンプ通知
        public IObservable<Unit> JumpSubject => _jumpSubject;
        // 攻撃通知
        public IObservable<Unit> AttackSubject => _attackSubject;

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            MoveVector = _moveAction.ReadValue<Vector2>();

            if (_jumpAction.triggered) {
                _jumpSubject.OnNext(Unit.Default);
            }

            if (_attackAction.triggered) {
                _attackSubject.OnNext(Unit.Default);
            }
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            _moveAction = _playerInput.actions["Move"];
            _jumpAction = _playerInput.actions["Jump"];
            _attackAction = _playerInput.actions["Attack"];
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        private void OnDestroy() {
            _jumpSubject.SafeDispose();
            _attackSubject.SafeDispose();
        }
    }
}