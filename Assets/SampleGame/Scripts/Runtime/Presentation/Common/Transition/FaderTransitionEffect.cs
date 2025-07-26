using System.Collections;
using GameFramework;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// Faderを使ったTransitionEffect
    /// </summary>
    public class FaderTransitionEffect : ITransitionEffect {
        private readonly Color _color;
        private readonly float _fadeInDuration;
        private readonly float _fadeOutDuration;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FaderTransitionEffect(Color color, float fadeOutDuration = 0.2f, float fadeInDuration = 0.2f) {
            _color = color;
            _fadeOutDuration = fadeOutDuration;
            _fadeInDuration = fadeInDuration;
        }
        
        /// <summary>
        /// 遷移開始
        /// </summary>
        void ITransitionEffect.BeginTransition(){
        }

        /// <summary>
        /// 開始ルーチン
        /// </summary>
        IEnumerator ITransitionEffect.EnterEffectRoutine() {
            yield return ResidentUIUtility.ColorFadeOutAsync(_color, _fadeOutDuration);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        void ITransitionEffect.Update() {
        }

        /// <summary>
        /// 終了ルーチン
        /// </summary>
        IEnumerator ITransitionEffect.ExitEffectRoutine() {
            yield return ResidentUIUtility.ColorFadeInAsync(_fadeInDuration);
        }
        
        /// <summary>
        /// 遷移終了
        /// </summary>
        void ITransitionEffect.EndTransition(){
        }
    }
}