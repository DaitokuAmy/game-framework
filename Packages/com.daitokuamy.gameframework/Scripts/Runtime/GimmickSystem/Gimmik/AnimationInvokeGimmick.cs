using UnityEngine;

namespace GameFramework.GimmickSystem {
    /// <summary>
    /// Invoke時にAnimationするGimmick
    /// </summary>
    public abstract class AnimationInvokeGimmick : InvokeGimmick {
        // 更新モード
        private enum UpdateMode {
            Update,
            LateUpdate,
        }

        [SerializeField, Tooltip("更新モード")]
        private UpdateMode _updateMode = UpdateMode.LateUpdate;

        // 現在時間
        private float _time;
        // 再生中か
        private bool _playing;

        /// <summary>トータル時間</summary>
        public abstract float Duration { get; }

        /// <inheritdoc/>
        protected sealed override void InvokeInternal() {
            _playing = true;
            _time = 0.0f;
        }

        /// <inheritdoc/>
        protected sealed override void UpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.Update) {
                UpdateAnimation(deltaTime);
            }
        }

        /// <inheritdoc/>
        protected sealed override void LateUpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.LateUpdate) {
                UpdateAnimation(deltaTime);
            }
        }

        /// <summary>
        /// 再生状態の反映
        /// </summary>
        protected abstract void Evaluate(float time);

        /// <summary>
        /// アニメーション更新
        /// </summary>
        private void UpdateAnimation(float deltaTime) {
            if (!_playing || Duration <= float.Epsilon) {
                return;
            }

            // 反映
            Evaluate(Mathf.Clamp(_time, 0.0f, Duration));

            // 再生完了チェック
            if (_time >= Duration) {
                _playing = false;
            }

            // 時間更新
            _time += deltaTime;
        }
    }
}
