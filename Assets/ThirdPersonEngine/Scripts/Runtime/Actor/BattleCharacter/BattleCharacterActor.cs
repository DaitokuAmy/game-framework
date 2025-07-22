using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework.ActorSystems;
using GameFramework.Core;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// バトルキャラアクター
    /// </summary>
    public partial class BattleCharacterActor : CharacterActor {
        
        private readonly VelocityActorComponent _velocityComponent;
        private readonly BattleCharacterActorData _data;
        private readonly CharacterController _characterController;

        private bool _isAir;
        private float _groundHeight;

        /// <summary>地上にいるか</summary>
        public override bool IsGrounded => _characterController.isGrounded;
        /// <summary>地面の高さ</summary>
        public override float GroundHeight => _groundHeight;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActor(Body body, BattleCharacterActorData data)
            : base(body, data) {
            _data = data;
            _characterController = body.GetComponent<CharacterController>();
            _velocityComponent = AddComponent(new VelocityActorComponent(this, _data.moveActionInfo));
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            SetupSequenceEvents(scope);
            SetupStates(scope);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal() {
            base.UpdateInternal();

            var deltaTime = Body.DeltaTime;

            // 地上状態の更新
            UpdateGroundStatus();

            // 状態更新
            UpdateState(deltaTime);
        }

        /// <summary>
        /// 現在座標の更新
        /// </summary>
        protected override void SetPosition(Vector3 position) {
            _characterController.Move(position - Body.Position);
        }

        /// <summary>
        /// 移動入力
        /// </summary>
        public void InputMove(Vector2 input) {
            CurrentState.InputMove(input);
        }

        /// <summary>
        /// スプリント入力
        /// </summary>
        public void InputSprint(bool sprint) {
            CurrentState.InputSprint(sprint);
        }

        /// <summary>
        /// ジャンプ入力
        /// </summary>
        public void InputJump() {
            CurrentState.InputJump();
        }

        /// <summary>
        /// 空中状態の更新
        /// </summary>
        private void UpdateGroundStatus() {
            var results = new RaycastHit[4];
            var groundMask = LayerMask.GetMask("Ground");
            if (Physics.RaycastNonAlloc(Body.Position + Vector3.up, Vector3.down, results, 3.0f, groundMask) > 0) {
                _isAir = results[0].distance > 1.0 + _data.airHeight;
                _groundHeight = results[0].point.y;
            }
            else {
                _isAir = true;
                _groundHeight = 0.0f;
            }
        }

        /// <summary>
        /// StateActionの再生
        /// </summary>
        private UniTask PlayStateActionAsync(StateType stateType, CancellationToken ct) {
            _stateContainer.Change(stateType, true);

            // Stateを抜けるまで待機
            return UniTask.WaitUntil(() => _stateContainer.CurrentKey != stateType, cancellationToken: ct);
        }
    }
}