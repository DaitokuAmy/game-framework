using System;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Playableを制御するためのインターフェース
    /// </summary>
    public interface IPlayableComponent : IDisposable {
        /// <summary>初期化済みか</summary>
        bool IsInitialized { get; }
        /// <summary>廃棄済みか</summary>
        bool IsDisposed { get; }

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