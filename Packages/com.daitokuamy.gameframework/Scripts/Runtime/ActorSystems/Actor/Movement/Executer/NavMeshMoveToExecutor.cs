using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// MoveTo を NavMesh で実現する移動実行器
    /// </summary>
    public sealed class NavMeshMoveToExecutor : IMoveExecutor<MoveToRequest> {
        /// <inheritdoc/>
        public int Priority { get; }

        private readonly IMovable _movable;
        private readonly INavMoveAgent _agent;
        private readonly float _turnSpeedDeg;

        private MoveToRequest _request;
        private bool _running;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NavMeshMoveToExecutor(IMovable movable, INavMoveAgent agent, int priority, float turnSpeedDeg) {
            _movable = movable;
            _agent = agent;
            Priority = priority;
            _turnSpeedDeg = turnSpeedDeg;
            _request = default;
            _running = false;
        }

        /// <summary>
        /// 移動開始を試行する
        /// </summary>
        public StartResult TryStart(in MoveToRequest request) {
            var destination = request.Target.GetWorldPosition();
            if (!_agent.TrySetDestination(destination)) {
                return StartResult.Rejected;
            }

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

            var stopDistance = Mathf.Max(_request.StopDistance, 0.0001f);
            if (_agent.HasPath && _agent.RemainingDistance <= stopDistance) {
                _agent.ResetPath();
                _running = false;
                return RunResult.Succeeded;
            }

            var velocity = _agent.Velocity;
            velocity.y = 0.0f;

            if (_request.Options.Rotate && velocity.sqrMagnitude > 0.0001f) {
                var targetRot = Quaternion.LookRotation(velocity, Vector3.up);
                var rot = Quaternion.RotateTowards(_movable.Rotation, targetRot, _turnSpeedDeg * deltaTime);
                _movable.ApplyRotation(rot);
            }

            return RunResult.Running;
        }

        /// <summary>
        /// 移動をキャンセルする
        /// </summary>
        public void Cancel() {
            _agent.ResetPath();
            _running = false;
        }
    }
}