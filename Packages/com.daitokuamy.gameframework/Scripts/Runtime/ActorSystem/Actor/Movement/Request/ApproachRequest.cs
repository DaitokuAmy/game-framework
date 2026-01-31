namespace GameFramework.ActorSystem {
    /// <summary>
    /// 指定ターゲットへ接近するリクエスト
    /// </summary>
    public readonly struct ApproachRequest : IMoveRequest {
        /// <summary>接近対象ターゲット</summary>
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
        public ApproachRequest(MoveTarget target, float speedMultiplier, float stopDistance, MoveOptions options) {
            Target = target;
            SpeedMultiplier = speedMultiplier;
            StopDistance = stopDistance;
            Options = options;
        }
    }
}