namespace GameFramework.CollisionSystem {
    /// <summary>
    /// 衝突検知リスナー
    /// </summary>
    public interface IHitListener {
        /// <summary>
        /// 衝突開始
        /// </summary>
        void OnHitEnter(in HitEvent evt);

        /// <summary>
        /// 衝突中
        /// </summary>
        void OnHitStay(in HitEvent evt);

        /// <summary>
        /// 衝突終了
        /// </summary>
        void OnHitExit(in HitEvent evt);
    }
}