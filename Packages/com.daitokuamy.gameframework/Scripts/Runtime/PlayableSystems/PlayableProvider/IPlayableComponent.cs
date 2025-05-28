using System;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Playableを制御するためのインターフェース
    /// </summary>
    public interface IPlayableComponent : IDisposable {
        // 初期化済みか
        bool IsInitialized { get; }
        // 廃棄済みか
        bool IsDisposed { get; }
        // 自動廃棄するか
        bool AutoDispose { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="graph">構築に使うGraph</param>
        void Initialize(PlayableGraph graph);

        /// <summary>
        /// Playableの取得
        /// </summary>
        Playable GetPlayable();

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// 再生時間の設定
        /// </summary>
        void SetTime(float time);

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        void SetSpeed(float speed);
    }
}