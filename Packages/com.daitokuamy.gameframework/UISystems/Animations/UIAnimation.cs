using UnityEngine;

namespace GameFramework.UISystems {    
    /// <summary>
    /// UIAnimationをScriptableObjectベースで制御するための基底
    /// </summary>
    public abstract class UIAnimation : IUIAnimation {
        /// <summary>制御対象オブジェクト</summary>
        protected GameObject RootObject { get; private set; }
        /// <summary>アニメーショントータル時間</summary>
        public abstract float Duration { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UIAnimation(GameObject rootObject) {
            RootObject = rootObject;
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
        /// 時間の設定
        /// </summary>
        protected abstract void SetTimeInternal(float time);

        /// <summary>
        /// 再生開始通知
        /// </summary>
        protected virtual void OnPlayInternal() {}
    }
}