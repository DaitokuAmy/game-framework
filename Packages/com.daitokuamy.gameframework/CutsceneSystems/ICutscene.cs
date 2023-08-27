using System;

namespace GameFramework.CutsceneSystems {
    /// <summary>
    /// カットシーン用インターフェース
    /// </summary>
    public interface ICutscene : IDisposable {
        /// <summary>再生中か</summary>
        bool IsPlaying { get; }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize(bool updateGameTime);

        /// <summary>
        /// Poolに戻る際の処理
        /// </summary>
        void OnReturn();

        /// <summary>
        /// 再生処理
        /// </summary>
        void Play();

        /// <summary>
        /// 停止処理
        /// </summary>
        void Stop();
        
        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        /// <param name="speed">再生速度</param>
        void SetSpeed(float speed);
    }
}