using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UISystems {    
    /// <summary>
    /// CanvasGroupを操作するシンプルなUIAnimation
    /// </summary>
    [CreateAssetMenu(fileName = "ui_animation_canvas_group.asset", menuName = "GameFramework/UI Animation/Canvas Group")]
    public class CanvasGroupUIAnimationData : UIAnimationData<CanvasGroupUIAnimationData, CanvasGroupUIAnimationData.Animation> {
        /// <summary>
        /// 再生実行用のAnimationクラス
        /// </summary>
        public new class Animation : UIAnimationData<CanvasGroupUIAnimationData, Animation>.Animation {
            private CanvasGroup _canvasGroup;
            
            /// <summary>
            /// 初期化処理
            /// </summary>
            protected override void InitializeInternal() {
                _canvasGroup = RootObject.GetComponent<CanvasGroup>();
            }

            /// <summary>
            /// 時間の設定
            /// </summary>
            protected override void SetTimeInternal(float time) {
                if (_canvasGroup != null) {
                    var ratio = Duration > float.Epsilon ? time / Duration : 1.0f;
                    _canvasGroup.alpha = Data.easeTypeAlpha.Evaluate(Data.startAlpha, Data.exitAlpha, ratio);
                    _canvasGroup.transform.localScale = Data.easeTypeScale.Evaluate(Data.startScale, Data.exitScale, ratio) * Vector3.one;
                }
            }
        }

        [SerializeField, Tooltip("開始α値")]
        public float startAlpha = 0.0f;
        [SerializeField, Tooltip("終了α値")]
        public float exitAlpha = 1.0f;
        [SerializeField, Tooltip("α値の補間タイプ")]
        public EaseType easeTypeAlpha = EaseType.Linear;
        [SerializeField, Tooltip("開始スケール値")]
        public float startScale = 1.0f;
        [SerializeField, Tooltip("終了スケール値")]
        public float exitScale = 1.0f;
        [SerializeField, Tooltip("スケールの補間タイプ")]
        public EaseType easeTypeScale = EaseType.Linear;
        [SerializeField, Tooltip("再生時間")]
        public float duration = 1.0f;

        /// <summary>再生トータル時間</summary>
        protected override float Duration => duration;
    }
}