using System.Collections;
using System.Linq;
using GameFramework;
using GameFramework.Core;

namespace SampleGame.Presentation {
    /// <summary>
    /// ロード画面を表示するTransitionEffect
    /// </summary>
    public class LoadingTransitionEffect : ITransitionEffect {
        // 表示するロード画面のキー
        private readonly string[] _loadKeys;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LoadingTransitionEffect(string[] loadKeys) {
            _loadKeys = loadKeys;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LoadingTransitionEffect() {
            _loadKeys = new[] {
                 "Normal",
            };
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
            var loadKey = _loadKeys.OrderBy(key => RandomUtil.Range(0.0f, 1.0f)).FirstOrDefault();
            yield return ResidentUIUtility.ShowLoading(loadKey);
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
            yield return ResidentUIUtility.HideLoading();
        }
        
        /// <summary>
        /// 遷移終了
        /// </summary>
        void ITransitionEffect.EndTransition(){
        }
    }
}