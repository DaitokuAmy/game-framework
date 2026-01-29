using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// MoveTo を ルートモーション（ブレンドツリー）で実現する移動実行器
    /// </summary>
    public sealed class RootMotionMoveToExecutor : IMoveExecutor<MoveToRequest> {
        /// <inheritdoc/>
        public int Priority { get; }

        private readonly IMovable _movable;
        private readonly IRootMotionDriver _driver;
        private readonly float _baseSpeed;
        private readonly float _turnSpeedDeg;

        private MoveToRequest _request;
        private bool _running;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RootMotionMoveToExecutor(IMovable movable, IRootMotionDriver driver, int priority, float baseSpeed, float turnSpeedDeg) {
            _movable = movable;
            _driver = driver;
            Priority = priority;
            _baseSpeed = baseSpeed;
            _turnSpeedDeg = turnSpeedDeg;
            _request = default;
            _running = false;
        }

        /// <summary>
        /// 移動開始を試行する
        /// </summary>
        public StartResult TryStart(in MoveToRequest request) {
            _request = request;
            _running = true;
            return StartResult.Accepted;
        }

        /// <summary>
        /// 移動処理を進行する
        /// </summary>
        public RunResult Tick(float deltaTime) {
            if (!_running) {
                return RunResult.Cancelled;
            }

            var target = _request.Target.GetWorldPosition();
            var pos = _movable.Position;

            var toTarget = target - pos;
            toTarget.y = 0.0f;

            var stopDistance = Mathf.Max(_request.StopDistance, 0.0001f);
            if (toTarget.sqrMagnitude <= stopDistance * stopDistance) {
                _driver.ResetLocomotion();
                _running = false;
                return RunResult.Succeeded;
            }

            var dir = toTarget.normalized;
            var desiredSpeed = _baseSpeed * _request.SpeedMultiplier;

            if (_request.Options.Rotate) {
                var targetRot = Quaternion.LookRotation(dir, Vector3.up);
                var rot = Quaternion.RotateTowards(_movable.Rotation, targetRot, _turnSpeedDeg * deltaTime);
                _movable.ApplyRotation(rot);
            }

            var localDir = Quaternion.Inverse(_movable.Rotation) * dir;
            var localVelocity = localDir * desiredSpeed;

            _driver.SetLocomotion(localVelocity, desiredSpeed, false);

            return RunResult.Running;
        }

        /// <summary>
        /// 移動をキャンセルする
        /// </summary>
        public void Cancel() {
            _driver.ResetLocomotion();
            _running = false;
        }
    }
}