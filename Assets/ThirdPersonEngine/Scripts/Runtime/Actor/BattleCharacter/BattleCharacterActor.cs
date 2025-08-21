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
        private readonly BattleCharacterActorData _data;
        private readonly CharacterController _characterController;
        private readonly VelocityActorComponent _velocityComponent;
        private readonly SensorActorComponent _sensorComponent;
        private readonly LookAtControlActorComponent _lookAtControlComponent;

        /// <summary>地上にいるか</summary>
        public override bool IsGrounded => _characterController.isGrounded;

        /// <summary>注視向き</summary>
        public Quaternion LookAtRotation => _lookAtControlComponent.LookAtRotation;
        /// <summary>空中状態か</summary>
        public bool IsAir => _sensorComponent.IsAir;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BattleCharacterActor(Body body, BattleCharacterActorData data)
            : base(body, data) {
            _data = data;
            _characterController = body.GetComponent<CharacterController>();
            _velocityComponent = AddComponent(new VelocityActorComponent(this, _data.moveActionInfo));
            _sensorComponent = AddComponent(new SensorActorComponent(this, _data.sensorInfo, LayerUtility.GetLayerMask(LayerType.Environment)));
            _lookAtControlComponent = AddComponent(new LookAtControlActorComponent(this));
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
        /// 注視動入力
        /// </summary>
        public void InputLookAt(Vector2 input) {
            _lookAtControlComponent.Rotate(input.x);
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
        /// StateActionの再生
        /// </summary>
        private UniTask PlayStateActionAsync(StateType stateType, CancellationToken ct) {
            _fsm.Change(stateType, true);

            // Stateを抜けるまで待機
            return UniTask.WaitUntil(() => _fsm.CurrentKey != stateType, cancellationToken: ct);
        }
    }
}