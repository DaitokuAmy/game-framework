using UnityEngine;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// DriveRequest を運動学的（Kinematic）に処理する移動実行器
    /// </summary>
    public sealed class KinematicDriveExecutor : IUpdatableMoveExecutor<DriveRequest> {
        /// <inheritdoc/>
        public int Priority { get; }

        private readonly IMovable _movable;
        private readonly float _baseSpeed;
        private readonly float _baseAccel;
        private readonly float _baseDecel;
        private readonly float _turnSpeedDeg;

        private DriveRequest _request;
        private Vector3 _velocity;
        private bool _running;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public KinematicDriveExecutor(
            IMovable movable,
            int priority,
            float baseSpeed,
            float baseAccel,
            float baseDecel,
            float turnSpeedDeg
        ) {
            _movable = movable;
            Priority = priority;
            _baseSpeed = baseSpeed;
            _baseAccel = baseAccel;
            _baseDecel = baseDecel;
            _turnSpeedDeg = turnSpeedDeg;
            _request = default;
            _velocity = Vector3.zero;
            _running = false;
        }

        /// <inheritdoc/>
        public StartResult TryStart(in DriveRequest request) {
            _request = request;
            _velocity = Vector3.zero;
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

            var input = _request.Move;
            var inputMag = Mathf.Clamp01(input.magnitude);

            var forward = _movable.Rotation * Vector3.forward;
            var right = _movable.Rotation * Vector3.right;

            var desiredDir = right * input.x + forward * input.y;
            desiredDir.y = 0.0f;

            var desiredVelocity = desiredDir.sqrMagnitude > 0.0001f
                ? desiredDir.normalized * (_baseSpeed * _request.SpeedMultiplier * inputMag)
                : Vector3.zero;

            var accel = _request.Options.Accel ?? _baseAccel;
            var decel = _request.Options.Decel ?? _baseDecel;
            var useAccel = Vector3.Dot(desiredVelocity, _velocity) >= 0.0f ? accel : decel;

            var velRef = _velocity;
            _velocity = Vector3.SmoothDamp(
                _velocity,
                desiredVelocity,
                ref velRef,
                CalcSmoothTime(useAccel),
                Mathf.Infinity,
                deltaTime
            );

            _movable.ApplyMove(_velocity * deltaTime);

            if (_request.Options.Rotate && _velocity.sqrMagnitude > 0.0001f) {
                var targetRot = Quaternion.LookRotation(_velocity, Vector3.up);
                var rot = Quaternion.RotateTowards(
                    _movable.Rotation,
                    targetRot,
                    _turnSpeedDeg * deltaTime
                );
                _movable.ApplyRotation(rot);
            }

            return RunResult.Running;
        }

        /// <inheritdoc/>
        public void Cancel() {
            _velocity = Vector3.zero;
            _running = false;
        }

        /// <summary>
        /// 加速度（または減速度）を SmoothDamp 用の時間パラメータに変換する
        /// </summary>
        private static float CalcSmoothTime(float accel) {
            return accel <= 0.0f ? 0.0f : 1.0f / accel;
        }
    }
}