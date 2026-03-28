using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// AnimationJobを制御するためのインターフェース
    /// </summary>
    public interface IAnimationJobComponent : IDisposable {
        /// <summary>初期化済みか</summary>
        bool IsInitialized { get; }
        /// <summary>廃棄済みか</summary>
        bool IsDisposed { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="animator">構築に使うAnimator</param>
        /// <param name="graph">構築に使うGraph</param>
        void Initialize(Animator animator, PlayableGraph graph);

        /// <summary>
        /// 更新処理
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Playableの取得
        /// </summary>
        AnimationScriptPlayable GetPlayable();
    }
}
