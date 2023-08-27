using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimationJobを提供するためのインターフェース
    /// </summary>
    public interface IAnimationJobProvider : IDisposable {
        // 初期化済みか
        bool IsInitialized { get; }
        // 廃棄済みか
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