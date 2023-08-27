namespace GameFramework.ProjectileSystems {
    /// <summary>
    /// 飛翔体用制御用インターフェース
    /// </summary>
    public interface IProjectile {
        /// <summary>
        /// 飛翔開始
        /// </summary>
        void Start();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        bool Update(float deltaTime);

        /// <summary>
        /// 飛翔終了
        /// </summary>
        void Stop();
    }
}