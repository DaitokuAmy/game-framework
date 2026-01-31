using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// DriveRequest をルートモーション（ブレンドツリー入力）で実現する移動実行器
    /// </summary>
    public sealed class RootMotionDriveExecutor : IUpdatableMoveExecutor<DriveRequest> {
        /// <inheritdoc/>
        public int Priority { get; }

        private readonly IMovable _movable;
        private readonly IRootMotionDriver _driver;
        private readonly float _turnSpeedDeg;

        private DriveRequest _request;
        private bool _running;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RootMotionDriveExecutor(IMovable movable, IRootMotionDriver driver, int priority, float turnSpeedDeg) {
            _movable = movable;
            _driver = driver;
            Priority = priority;
            _turnSpeedDeg = turnSpeedDeg;
            _request = default;
            _running = false;
        }

        /// <inheritdoc/>
        public StartResult TryStart(in DriveRequest request) {
            _request = request;
            _running = true;
            return StartResult.Accepted;
        }

        /// <inheritdoc/>
        public void Update(in DriveRequest request) {
            _request = request;
        }

        /// <inheritdoc/>
        public RunResult Tick(float deltaTime) {
            if (!_running) {
                return RunResult.Cancelled;
            }

            var move = _request.Move;
            var magnitude = Mathf.Clamp01(move.magnitude);

            if (magnitude <= 0.0f) {
                _driver.SetLocomotion(Vector2.zero, _request.SpeedMultiplier, _request.Run);
                return RunResult.Running;
            }

            if (_request.Options.Rotate) {
                var forward = _movable.Rotation * Vector3.forward;
                var right = _movable.Rotation * Vector3.right;

                var desiredWorld = (right * move.x + forward * move.y);
                desiredWorld.y = 0.0f;

                if (desiredWorld.sqrMagnitude > 0.0001f) {
                    var targetRot = Quaternion.LookRotation(desiredWorld, Vector3.up);
                    var rot = Quaternion.RotateTowards(_movable.Rotation, targetRot, _turnSpeedDeg * deltaTime);
                    _movable.ApplyRotation(rot);
                }
            }

            var inv = Quaternion.Inverse(_movable.Rotation);
            var local3 = inv * new Vector3(move.x, 0.0f, move.y);
            var localMove = new Vector2(local3.x, local3.z);

            _driver.SetLocomotion(localMove, _request.SpeedMultiplier * magnitude, _request.Run);
            return RunResult.Running;
        }

        /// <inheritdoc/>
        public void Cancel() {
            _driver.ResetLocomotion();
            _running = false;
        }
    }
}