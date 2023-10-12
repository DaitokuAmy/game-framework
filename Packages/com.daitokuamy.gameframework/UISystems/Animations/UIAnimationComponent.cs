using UnityEngine;

namespace GameFramework.UISystems {    
    /// <summary>
    /// UIAnimationを再生するためのComponent
    /// </summary>
    public abstract class UIAnimationComponent : MonoBehaviour, IUIAnimation {
        [SerializeField, Tooltip("表示名")]
        private string _label = "";
        
        private bool _initialized;
        
        /// <summary>トータル時間</summary>
        public abstract float Duration { get; }

        /// <summary>表示名</summary>
        public string Label => _label;

        /// <summary>
        /// 時間の設定
        /// </summary>
        /// <param name="time">現在時間</param>
        void IUIAnimation.SetTime(float time) {
            Initialize();
            SetTimeInternal(time);
        }

        /// <summary>
        /// 再生開始通知
        /// </summary>
        void IUIAnimation.OnPlay() {
            Initialize();
            OnPlayInternal();
        }
        
        /// <summary>
        /// 生成時処理
        /// </summary>
        protected virtual void AwakeInternal() {}

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeInternal() {}
        
        /// <summary>
        /// 時間の設定
        /// </summary>
        /// <param name="time">現在時間</param>
        protected virtual void SetTimeInternal(float time) {}
        
        /// <summary>
        /// 再生開始通知
        /// </summary>
        protected virtual void OnPlayInternal() {}

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize() {
            if (_initialized) {
                return;
            }

            _initialized = true;
            InitializeInternal();
        }

        /// <summary>
        /// 生成時処理
        /// </summary>
        private void Awake() {
            Initialize();
            AwakeInternal();
        }
    }
}