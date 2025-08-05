using System;
using System.Collections;
using GameFramework.Core;

namespace GameFramework.UISystems {
    /// <summary>
    /// アニメーション用ハンドル
    /// </summary>
    public struct AnimationHandle : IProcess {
        private readonly AnimationStatus _animationStatus;

        /// <summary>アニメーション完了しているか</summary>
        public bool IsDone => _animationStatus == null || _animationStatus.IsDone;
        /// <summary>エラー終了している場合、エラーの内容</summary>
        public Exception Exception => _animationStatus?.Exception;

        /// <summary>未使用</summary>
        object IEnumerator.Current => null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal AnimationHandle(AnimationStatus animationStatus) {
            _animationStatus = animationStatus;
        }

        /// <summary>
        /// IEnumerator用
        /// </summary>
        bool IEnumerator.MoveNext() {
            return !IsDone;
        }

        /// <summary>
        /// IEnumerator用
        /// </summary>
        void IEnumerator.Reset() {
        }
    }

    /// <summary>
    /// アニメーション状態管理用
    /// </summary>
    internal class AnimationStatus {
        public bool IsDone { get; private set; }
        public Exception Exception { get; private set; }

        /// <summary>
        /// 成功
        /// </summary>
        public void Complete() {
            IsDone = true;
            Exception = null;
        }

        /// <summary>
        /// 失敗
        /// </summary>
        public void Abort(Exception exception) {
            IsDone = true;
            Exception = exception;
        }
    }
    
    /// <summary>
    /// UIViewインターフェース
    /// </summary>
    public interface IUIView : IDisposable {
        /// <summary>
        /// 初期化処理
        /// </summary>
        void Initialize(UIService service);
        
        /// <summary>
        /// 開始処理
        /// </summary>
        void Start();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);
        
        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void LateUpdate(float deltaTime);
    }
}