namespace GameFramework.CollisionSystems {
    /// <summary>
    /// 衝突検知リスナー
    /// </summary>
    public interface ICollisionListener {
        /// <summary>
        /// 衝突開始
        /// </summary>
        void OnCollisionEnter(in HitEvent evt);

        /// <summary>
        /// 衝突中
        /// </summary>
        void OnCollisionStay(in HitEvent evt);

        /// <summary>
        /// 衝突終了
        /// </summary>
        void OnCollisionExit(in HitEvent evt);
    }
}