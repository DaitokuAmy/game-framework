namespace GameFramework.VfxSystem {
    /// <summary>
    /// Vfx制御用コンポーネント
    /// </summary>
    public interface IVfxComponent {
        /// <summary>再生中か</summary>
        bool IsPlaying { get; }

        /// <summary>
        /// フレーム更新
        /// </summary>
        void Tick(float deltaTime);

        /// <summary>
        /// 再生
        /// </summary>
        void Play();

        /// <summary>
        /// 停止
        /// </summary>
        void Stop();

        /// <summary>
        /// 即時停止
        /// </summary>
        void StopImmediate();

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void SetSpeed(float speed);

        /// <summary>
        /// LodLevelの設定
        /// </summary>
        void SetLodLevel(int level);
    }
}