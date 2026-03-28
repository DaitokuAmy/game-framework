using System.Collections;
using UnityEngine;

namespace GameFramework.BootSystem {
    /// <summary>
    /// MainSystemの基底クラス
    /// </summary>
    public abstract class MainSystemBase : MonoBehaviour {
        /// <summary>開始処理中か</summary>
        protected bool IsWarming { get; private set; }

        /// <summary>
        /// 開始処理
        /// </summary>
        /// <param name="args">起動時に渡された引数</param>
        internal IEnumerator StartRoutine(object[] args) {
            IsWarming = true;
            try {
                PreStartInternal(args);
                yield return StartRoutineInternal(args);
                PostStartInternal(args);
            }
            finally {
                IsWarming = false;
            }
        }

        /// <summary>
        /// リブート処理
        /// </summary>
        /// <param name="args">リブート時に渡された引数</param>
        internal IEnumerator RebootRoutine(object[] args) {
            IsWarming = true;
            try {
                PreRebootInternal(args);
                yield return RebootRoutineInternal(args);
                PostRebootInternal(args);
            }
            finally {
                IsWarming = false;
            }
        }

        /// <summary>
        /// 開始処理（前処理）
        /// </summary>
        protected virtual void PreStartInternal(object[] args) {}
        
        /// <summary>
        /// 開始処理
        /// </summary>
        /// <param name="args">起動時に渡された引数</param>
        protected abstract IEnumerator StartRoutineInternal(object[] args);
        
        /// <summary>
        /// 開始処理（後処理）
        /// </summary>
        protected virtual void PostStartInternal(object[] args) {}

        /// <summary>
        /// リブート処理（前処理）
        /// </summary>
        protected virtual void PreRebootInternal(object[] args) {}
        
        /// <summary>
        /// リブート処理
        /// </summary>
        /// <param name="args">リブート時に渡された引数</param>
        protected abstract IEnumerator RebootRoutineInternal(object[] args);
        
        /// <summary>
        /// リブート処理（後処理）
        /// </summary>
        protected virtual void PostRebootInternal(object[] args) {}

        /// <summary>
        /// ウォーミング中状態の設定
        /// </summary>
        internal void SetWarning(bool warming) {
            IsWarming = warming;
        }
    }
}
