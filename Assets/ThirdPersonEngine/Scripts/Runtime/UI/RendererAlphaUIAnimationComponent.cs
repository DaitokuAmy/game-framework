using System.Linq;
using GameFramework;
using GameFramework.Core;
using GameFramework.UISystems;
using UnityEngine;

namespace ThirdPersonEngine {
    /// <summary>
    /// Rendererの透明度を一括でUIAnimationで制御するためのクラス
    /// </summary>
    public class RendererAlphaUIAnimationComponent : UIAnimationComponent {
        private enum AlphaLayer {
            System = 1 << 0,
            Animation = 1 << 1,
        }

        private class RendererInfo {
            public MaterialInstance MaterialInstance;
            public LayeredScale LayeredAlpha;
        }

        [Header("Target")]
        [SerializeField, Tooltip("制御プロパティ名")]
        private string propertyName = "";
        [SerializeField, Tooltip("ターゲット")]
        private RendererMaterial[] targets;
        [SerializeField, Tooltip("Materialの制御タイプ")]
        private MaterialInstance.ControlType controlType = MaterialInstance.ControlType.Auto;

        [Header("Animation")]
        [SerializeField, Tooltip("開始α値")]
        private float startAlpha = 0.0f;
        [SerializeField, Tooltip("終了α値")]
        private float exitAlpha = 1.0f;

        /// <summary>トータル時間</summary>
        public override float Duration => _duration;

        private EaseType _easeType;
        private float _duration;
        
        private int _propertyId;
        private RendererInfo[] _rendererInfos;

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void AwakeInternal() {
            Refresh();
        }

        /// <summary>
        /// 時間の設定
        /// </summary>
        /// <param name="time">現在時間</param>
        protected override void SetTimeInternal(float time) {
            if (_rendererInfos.Length <= 0) {
                return;
            }
            
            var ratio = Duration > float.Epsilon ? time / Duration : 1.0f;
            var animationAlpha = _easeType.Evaluate(startAlpha, exitAlpha, ratio);
            foreach (var rendererInfo in _rendererInfos) {
                SetAnimationAlpha(rendererInfo, animationAlpha);
                ApplyAlpha(rendererInfo);
            }
        }

        /// <summary>
        /// イージングのパラメータをセットする
        /// </summary>
        public void SetEaseParameter(EaseType easeType, float duration) {
            _easeType = easeType;
            _duration = duration;
        }

        /// <summary>
        /// システムでの透明度をセットする
        /// </summary>
        public void SetSystemAlpha(float alpha) {
            if (_rendererInfos.Length <= 0) {
                return;
            }
            
            foreach (var rendererInfo in _rendererInfos) {
                rendererInfo.LayeredAlpha.Set(AlphaLayer.System, alpha);
                ApplyAlpha(rendererInfo);
            }
        }

        /// <summary>
        /// Material情報のリフレッシュ
        /// </summary>
        private void Refresh() {
            _propertyId = Shader.PropertyToID(propertyName);
            
            // RendererInfoが存在していたら、クリアする前に元の透明度を適用してあげる
            if (_rendererInfos.Length > 0) {
                foreach (var rendererInfo in _rendererInfos) {
                    SetAnimationAlpha(rendererInfo, 1f);
                    ApplyAlpha(rendererInfo);
                }
            }

            _rendererInfos = null;
            if (targets.Length <= 0) {
                return;
            }

            _rendererInfos = targets.Where(x => x.IsValid).Select(x => {
                var materialInstance = new MaterialInstance(x.renderer, x.materialIndex, controlType);
                var layeredAlpha = new LayeredScale();
                layeredAlpha.Set(AlphaLayer.System, materialInstance.GetVector(_propertyId).w);
                return new RendererInfo {
                    MaterialInstance = materialInstance,
                    LayeredAlpha = layeredAlpha,
                };
            }).ToArray();
        }

        /// <summary>
        /// アニメーションする透明度をセットする
        /// </summary>
        private void SetAnimationAlpha(RendererInfo rendererInfo, float alpha) {
            rendererInfo.LayeredAlpha.Set(AlphaLayer.Animation, alpha);
        }

        /// <summary>
        /// 透明度を適用する
        /// </summary>
        private void ApplyAlpha(RendererInfo rendererInfo) {
            var color = rendererInfo.MaterialInstance.GetVector(_propertyId);
            color.w = rendererInfo.LayeredAlpha.Value;
            rendererInfo.MaterialInstance.SetVector(_propertyId, color);
        }
    }
}