using System;
using UnityEngine;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// Motion制御用ハンドル
    /// </summary>
    public struct MotionHandle : IDisposable {
        public static readonly MotionHandle Null = new();

        private MotionLayerHandler _parentLayerHandler;
        private MotionCrossFader _crossFader;
        private bool _disposed;
        
        /// <summary>有効か</summary>
        public bool IsValid => _crossFader != null && _crossFader.IsValid;
        /// <summary>PlayableGraph情報</summary>
        public PlayableGraph Graph => _crossFader.Graph;
        /// <summary>再生に利用しているAnimator</summary>
        public Animator Animator => _crossFader.Animator;
        /// <summary>クロスフェーダー本体</summary>
        internal MotionCrossFader CrossFader => _crossFader;
        /// <summary>登録親のMotionLayer</summary>
        internal MotionLayerHandler ParentLayerHandler => _parentLayerHandler;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal MotionHandle(MotionLayerHandler parentLayerHandler, MotionCrossFader crossFader) {
            _parentLayerHandler = parentLayerHandler;
            _crossFader = crossFader;
            _disposed = false;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            
            if (_parentLayerHandler != null) {
                // 接続停止
                _parentLayerHandler.RemoveExtensionLayer(this);
                _parentLayerHandler = null;
            }

            _crossFader = null;
        }

        /// <summary>
        /// Playableを変更
        /// </summary>
        /// <param name="playable">再生するPlayable</param>
        /// <param name="blendDuration">ブレンド時間</param>
        /// <param name="autoDispose">自動廃棄するか</param>
        public void Change(Playable? playable, float blendDuration, bool autoDispose) {
            if (!IsValid) {
                return;
            }
            
            _crossFader.Change(playable, blendDuration, autoDispose);
        }

        /// <summary>
        /// ウェイトの変更
        /// </summary>
        public void SetWeight(float weight) {
            if (!IsValid) {
                return;
            }

            if (_parentLayerHandler == null) {
                return;
            }
            
            _parentLayerHandler.SetLayerWeight(this, weight);
        }

        /// <summary>
        /// ウェイトの取得
        /// </summary>
        public float GetWeight() {
            if (!IsValid) {
                return 0.0f;
            }

            if (_parentLayerHandler == null) {
                return 1.0f;
            }
            
            return _parentLayerHandler.GetLayerWeight(this);
        }
    }
}
