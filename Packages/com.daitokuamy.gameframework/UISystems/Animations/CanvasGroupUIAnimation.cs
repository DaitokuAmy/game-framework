using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {
    /// <summary>
    /// CanvasGroupをUIAnimationで再生するためのクラス
    /// </summary>
    public class CanvasGroupUIAnimation : IUIAnimation {
        /// <summary>
        /// 設定用記述子
        /// </summary>
        [Serializable]
        public struct Descriptor {
            [Tooltip("開始α値")]
            public float startAlpha;
            [Tooltip("終了α値")]
            public float exitAlpha;
            [Tooltip("α値の補間タイプ")]
            public EaseType easeTypeAlpha;
            [Tooltip("開始スケール値")]
            public float startScale;
            [Tooltip("終了スケール値")]
            public float exitScale;
            [Tooltip("スケールの補間タイプ")]
            public EaseType easeTypeScale;
            [Tooltip("再生時間")]
            public float duration;
        }
        
        private readonly CanvasGroup _canvasGroup;
        private readonly Descriptor _descriptor;

        /// <summary>再生トータル時間</summary>
        public float Duration => _descriptor.duration;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public CanvasGroupUIAnimation(CanvasGroup canvasGroup, Descriptor descriptor) {
            _canvasGroup = canvasGroup;
            _descriptor = descriptor;
        }

        /// <summary>
        /// 時間の設定
        /// </summary>
        void IUIAnimation.SetTime(float time) {
            if (_canvasGroup != null) {
                var ratio = Duration > float.Epsilon ? time / Duration : 1.0f;
                _canvasGroup.alpha = _descriptor.easeTypeAlpha.Evaluate(_descriptor.startAlpha, _descriptor.exitAlpha, ratio);
                _canvasGroup.transform.localScale = _descriptor.easeTypeScale.Evaluate(_descriptor.startScale, _descriptor.exitScale, ratio) * Vector3.one;
            }
        }

        /// <summary>
        /// 再生開始通知
        /// </summary>
        void IUIAnimation.OnPlay() {
        }
    }
}