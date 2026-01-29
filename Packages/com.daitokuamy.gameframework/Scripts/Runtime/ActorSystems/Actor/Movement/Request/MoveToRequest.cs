namespace GameFramework.ActorSystems {
    /// <summary>
    /// 指定ターゲットへ移動するリクエスト
    /// </summary>
    public readonly struct MoveToRequest : IMoveRequest {
        /// <summary>移動先ターゲット</summary>
        public readonly MoveTarget Target;
        /// <summary>速度倍率</summary>
        public readonly float SpeedMultiplier;
        /// <summary>到達判定距離</summary>
        public readonly float StopDistance;
        /// <summary>オプション</summary>
        public readonly MoveOptions Options;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MoveToRequest(MoveTarget target, float speedMultiplier, float stopDistance, MoveOptions options) {
            Target = target;
            SpeedMultiplier = speedMultiplier;
            StopDistance = stopDistance;
            Options = options;
        }
    }
}