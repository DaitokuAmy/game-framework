using UnityEngine;

namespace GameFramework.ActorSystem {
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
        private Vector3 _destination;

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

        /// <inheritdoc/>
        public StartResult TryStart(in MoveToRequest request) {
            _destination = request.Target.GetWorldPosition();
            if (!_agent.TrySetDestination(_destination)) {
                return StartResult.Rejected;
            }

            _request = request;
            _running = true;
            return StartResult.Accepted;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Cancel(bool skip) {
            _agent.ResetPath();
            
            if (skip) {
                var delta = _destination - _movable.Position;
                _movable.ApplyMove(delta, true);
            }
            
            _running = false;
        }
    }
}