using System;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Motion制御用ハンドル
    /// </summary>
    public struct MotionHandle : IDisposable {
        public static readonly MotionHandle Null = new();

        private LayerMixerPlayableComponent _parentComponent;
        private MotionCrossFader _crossFader;
        private bool _disposed;
        
        /// <summary>有効か</summary>
        public bool IsValid => _crossFader != null && _crossFader.IsValid;
        /// <summary>クロスフェーダー本体</summary>
        internal MotionCrossFader CrossFader => _crossFader;
        /// <summary>登録親のComponent</summary>
        internal LayerMixerPlayableComponent ParentComponent => _parentComponent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal MotionHandle(LayerMixerPlayableComponent parentComponent, MotionCrossFader crossFader) {
            _parentComponent = parentComponent;
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
            
            if (_parentComponent != null) {
                // 接続停止
                _parentComponent.RemoveExtensionLayer(this);
                _parentComponent = null;
            }

            _crossFader = null;
        }

        /// <summary>
        /// 再生対象のPlayableProviderを変更
        /// </summary>
        /// <param name="component">変更対象のPlayableを返すProvider</param>
        /// <param name="blendDuration">ブレンド時間</param>
        public void Change(IPlayableComponent component, float blendDuration = 1.0f) {
            if (!IsValid) {
                return;
            }
            
            _crossFader.Change(component, blendDuration);
        }

        /// <summary>
        /// ウェイトの変更
        /// </summary>
        public void SetWeight(float weight) {
            if (!IsValid) {
                return;
            }

            if (_parentComponent == null) {
                return;
            }
            
            _parentComponent.SetLayerWeight(this, weight);
        }

        /// <summary>
        /// ウェイトの取得
        /// </summary>
        public float GetWeight() {
            if (!IsValid) {
                return 0.0f;
            }

            if (_parentComponent == null) {
                return 1.0f;
            }
            
            return _parentComponent.GetLayerWeight(this);
        }
    }
}