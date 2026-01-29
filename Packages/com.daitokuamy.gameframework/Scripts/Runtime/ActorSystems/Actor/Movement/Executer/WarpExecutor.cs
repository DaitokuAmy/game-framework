namespace GameFramework.ActorSystems {
    /// <summary>
    /// ワープ移動を実現する移動実行器
    /// </summary>
    public sealed class WarpExecutor : IMoveExecutor<WarpRequest> {
        /// <inheritdoc/>
        public int Priority { get; }

        private readonly IMovable _movable;
        private WarpRequest _request;
        private bool _running;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WarpExecutor(IMovable movable, int priority) {
            _movable = movable;
            Priority = priority;
            _request = default;
            _running = false;
        }

        /// <inheritdoc/>
        public StartResult TryStart(in WarpRequest request) {
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
            var delta = target - _movable.Position;
            delta.y = 0.0f;

            _movable.ApplyMove(delta, isWarp: true);

            _running = false;
            return RunResult.Succeeded;
        }

        /// <inheritdoc/>
        public void Cancel() {
            _running = false;
        }
    }
}