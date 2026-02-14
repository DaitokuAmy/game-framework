using UnityEngine;

namespace GameFramework.TweenSystem {
    /// <summary>
    /// 1つの補間を行うTweenerの基底
    /// </summary>
    public abstract class Tweener : Tween {
        /// <summary>Ease 種別</summary>
        public EaseType Ease { get; private set; } = EaseType.Linear;
        /// <summary>経過時間</summary>
        protected float Elapsed { get; private set; }
        /// <summary>補間時間</summary>
        protected float Length { get; private set; }

        /// <inheritdoc/>
        public sealed override float Duration => Length;

        /// <inheritdoc/>
        protected sealed override void OnTick(float deltaTime) {
            Elapsed += deltaTime;

            var t01 = Mathf.Clamp01(Elapsed / Length);
            if (t01 >= 1f) {
                Apply(1f);
                CompleteInternal();
                return;
            }

            var eased = Ease.Evaluate(t01);
            Apply(eased);
        }

        /// <inheritdoc/>
        protected sealed override void OnForceComplete() {
            Apply(1.0f);
        }

        /// <summary>
        /// 補間値を適用（eased: 0..1）
        /// </summary>
        protected abstract void Apply(float eased);

        /// <inheritdoc/>
        protected override void OnReset() {
            Elapsed = 0.0f;
            Length = 0.0f;
            Ease = EaseType.Linear;
            OnResetTweener();
        }

        /// <summary>
        /// Tweener固有のReset処理
        /// </summary>
        protected virtual void OnResetTweener() { }

        /// <summary>
        /// Easeを設定
        /// </summary>
        public Tweener SetEase(EaseType ease) {
            Ease = ease;
            return this;
        }

        /// <summary>
        /// durationを設定（Poolから借りた直後に呼び出し）
        /// </summary>
        protected void SetupDuration(float duration) {
            Length = Mathf.Max(0.0001f, duration);
        }
    }
}
