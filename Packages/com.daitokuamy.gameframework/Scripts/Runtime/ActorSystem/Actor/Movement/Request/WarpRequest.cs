namespace GameFramework.ActorSystem {
    /// <summary>
    /// 即時移動（ワープ）リクエスト
    /// </summary>
    public readonly struct WarpRequest : IMoveRequest {
        /// <summary>ワープ先ターゲット</summary>
        public readonly MoveTarget Target;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WarpRequest(MoveTarget target) {
            Target = target;
        }
    }
}