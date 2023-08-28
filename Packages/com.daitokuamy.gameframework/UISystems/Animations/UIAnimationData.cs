using UnityEngine;

namespace GameFramework.UISystems {    
    /// <summary>
    /// UIAnimationをScriptableObjectベースで制御するための基底
    /// </summary>
    public abstract class UIAnimationData<TData, TAnimation> : ScriptableObject
        where TData : UIAnimationData<TData, TAnimation>
        where TAnimation : UIAnimationData<TData, TAnimation>.Animation, new() {
        /// <summary>
        /// 再生実行用のAnimationクラス
        /// </summary>
        public abstract class Animation : IUIAnimation {
            private bool _initialized;
            
            /// <summary>アクセスデータ</summary>
            protected TData Data { get; private set; }
            /// <summary>制御対象オブジェクト</summary>
            protected GameObject RootObject { get; private set; }
            /// <summary>アニメーショントータル時間</summary>
            public float Duration => Data != null ? Data.Duration : 0.0f;

            /// <summary>
            /// 初期化処理
            /// </summary>
            public void Initialize(TData data, GameObject rootObject) {
                if (_initialized) {
                    return;
                }

                _initialized = true;
                
                Data = data;
                RootObject = rootObject;
                InitializeInternal();
            }

            /// <summary>
            /// 時間の設定
            /// </summary>
            void IUIAnimation.SetTime(float time) {
                SetTimeInternal(time);
            }

            /// <summary>
            /// 再生開始通知
            /// </summary>
            void IUIAnimation.OnPlay() {
                OnPlayInternal();
            }

            /// <summary>
            /// 初期化処理
            /// </summary>
            protected abstract void InitializeInternal();

            /// <summary>
            /// 時間の設定
            /// </summary>
            protected abstract void SetTimeInternal(float time);

            /// <summary>
            /// 再生開始通知
            /// </summary>
            protected virtual void OnPlayInternal() {}
        }
        
        /// <summary>再生トータル時間</summary>
        protected abstract float Duration { get; }

        /// <summary>
        /// UIAnimationの生成
        /// </summary>
        public TAnimation CreateAnimation(GameObject rootObject) {
            var animation = new TAnimation();
            animation.Initialize((TData)this, rootObject);
            return animation;
        }
    }
}