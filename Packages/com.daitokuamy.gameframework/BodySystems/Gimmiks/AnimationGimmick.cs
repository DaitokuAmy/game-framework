using UnityEngine;

namespace GameFramework.BodySystems {
    /// <summary>
    /// アニメーション制御するGimmickの基底
    /// </summary>
    public abstract class AnimationGimmick : Gimmick {
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
        // 逆再生状態か
        private bool _reverse;

        // トータル時間
        public abstract float Duration { get; }
        // ループ再生するか
        public abstract bool IsLooping { get; }

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="reverse">反転再生するか</param>
        /// <param name="immediate">即時反映するか</param>
        public void Play(bool reverse = false, bool immediate = false) {
            _playing = true;
            _reverse = reverse;
            _time = reverse ^ immediate ? Duration : 0.0f;
        }

        /// <summary>
        /// 再開
        /// </summary>
        /// <param name="reverse">反転再生するか</param>
        public void Resume(bool reverse = false) {
            _playing = true;
            _reverse = reverse;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected sealed override void UpdateInternal(float deltaTime) {
            if (_updateMode == UpdateMode.Update) {
                UpdateAnimation(deltaTime);
            }
        }

        /// <summary>
        /// 後更新処理
        /// </summary>
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
            if (_reverse) {
                if (_time <= 0.0f) {
                    if (IsLooping) {
                        do {
                            _time += Duration;
                        } while (_time <= 0.0f);
                    }
                    else {
                        _playing = false;
                    }
                }
            }
            else {
                if (_time >= Duration) {
                    if (IsLooping) {
                        do {
                            _time -= Duration;
                        } while (_time >= Duration);
                    }
                    else {
                        _playing = false;
                    }

                    _playing = false;
                }
            }

            // 時間更新
            _time += _reverse ? -deltaTime : deltaTime;
        }
    }
}