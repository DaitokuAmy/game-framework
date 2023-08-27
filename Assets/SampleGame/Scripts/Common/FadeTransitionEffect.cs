using System.Collections;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame {
    /// <summary>
    /// フェードを使った遷移エフェクト
    /// </summary>
    public class FadeTransitionEffect : ITransitionEffect {
        private Color _color;
        private float _duration;
        private DisposableScope _scope;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FadeTransitionEffect(Color color, float duration) {
            _color = color;
            _duration = duration;
        }

        /// <summary>
        /// 演出開始
        /// </summary>
        IEnumerator ITransitionEffect.EnterRoutine() {
            _scope = new DisposableScope();
            yield return Services.Get<FadeController>().FadeOutAsync(_color, _duration)
                .StartAsEnumerator(_scope);
        }

        /// <summary>
        /// 演出中
        /// </summary>
        void ITransitionEffect.Update() {
        }

        /// <summary>
        /// 演出終了
        /// </summary>
        IEnumerator ITransitionEffect.ExitRoutine() {
            _scope.Dispose();
            yield return Services.Get<FadeController>().FadeInAsync(_duration)
                .StartAsEnumerator(_scope);
        }
    }
}