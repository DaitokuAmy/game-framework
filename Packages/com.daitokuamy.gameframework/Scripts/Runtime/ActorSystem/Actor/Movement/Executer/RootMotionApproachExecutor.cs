using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// ApproachRequest をルートモーション（ブレンドツリー入力）で実現する移動実行器
    /// </summary>
    public sealed class RootMotionApproachExecutor : IMoveExecutor<ApproachRequest> {
        /// <inheritdoc/>
        public int Priority { get; }

        private readonly IMovable _movable;
        private readonly IRootMotionDriver _driver;
        private readonly float _turnSpeedDeg;

        private ApproachRequest _request;
        private bool _running;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RootMotionApproachExecutor(
            IMovable movable,
            IRootMotionDriver driver,
            int priority,
            float turnSpeedDeg
        ) {
            _movable = movable;
            _driver = driver;
            Priority = priority;
            _turnSpeedDeg = turnSpeedDeg;
            _request = default;
            _running = false;
        }

        /// <inheritdoc/>
        public StartResult TryStart(in ApproachRequest request) {
            _request = request;
            _running = true;
            return StartResult.Accepted;
        }

        /// <inheritdoc/>
        public RunResult Tick(float deltaTime) {
            if (!_running) {
                return RunResult.Cancelled;
            }

            var target = _request.Target.GetWorldPosition();
            var position = _movable.Position;

            var toTarget = target - position;
            toTarget.y = 0.0f;

            var stopDistance = Mathf.Max(_request.StopDistance, 0.0001f);
            if (toTarget.sqrMagnitude <= stopDistance * stopDistance) {
                _driver.ResetLocomotion();
                _running = false;
                return RunResult.Succeeded;
            }

            var dir = toTarget.normalized;

            if (_request.Options.Rotate) {
                var targetRot = Quaternion.LookRotation(dir, Vector3.up);
                var rot = Quaternion.RotateTowards(_movable.Rotation, targetRot, _turnSpeedDeg * deltaTime);
                _movable.ApplyRotation(rot);
            }

            var inv = Quaternion.Inverse(_movable.Rotation);
            var local = inv * dir;
            var localMove = new Vector2(local.x, local.z);

            _driver.SetLocomotion(localMove, _request.SpeedMultiplier, run: false);
            return RunResult.Running;
        }

        /// <inheritdoc/>
        public void Cancel(bool skip) {
            _driver.ResetLocomotion();

            if (skip) {
                var target = _request.Target.GetWorldPosition();
                var position = _movable.Position;
                var delta = target - position;
                _movable.ApplyMove(delta, true);
            }
            
            _running = false;
        }
    }
}
