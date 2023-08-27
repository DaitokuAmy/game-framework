using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// 値を変更するGimmickの基底
    /// </summary>
    public abstract class ChangeGimmick<T> : Gimmick {
        // 更新モード
        private enum UpdateMode {
            Update,
            LateUpdate,
        }

        [SerializeField, Tooltip("更新モード")]
        private UpdateMode _updateMode = UpdateMode.LateUpdate;

        // ターゲットの値
        private T _target;
        // 残り時間
        private float _timer;

        /// <summary>
        /// 値の設定
        /// </summary>
        /// <param name="val">設定する値</param>
        /// <param name="duration">反映にかける時間</param>
        public void Change(T val, float duration = 0.0f) {
            _target = val;
            _timer = duration;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected sealed override void UpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.Update) {
                UpdateValue(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
        protected sealed override void LateUpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.LateUpdate) {
                UpdateValue(deltaTime);
            }
        }

        /// <summary>
        /// 値の反映
        /// </summary>
        /// <param name="val">反映したい値</param>
        /// <param name="rate">反映率</param>
        protected abstract void SetValue(T val, float rate);

        /// <summary>
        /// 値の更新
        /// </summary>
        private void UpdateValue(float deltaTime) {
            if (_timer < 0.0f) {
                return;
            }

            // 現在の値に対しての反映率
            var rate = _timer > float.Epsilon ? Mathf.Clamp01(deltaTime / _timer) : 1.0f;
            SetValue(_target, rate);

            // 時間更新
            _timer -= deltaTime;
        }
    }
}