using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame.Presentation {
    /// <summary>
    /// 画面遷移のユーティリティクラス
    /// </summary>
    public static class TransitionUtility {
        /// <summary>
        /// ローディング用の遷移効果を生成する
        /// </summary>
        public static ITransitionEffect[] CreateLoadingTransitionEffects() {
            return new ITransitionEffect[] {
                new BlockTransitionEffect(),
                new LoadingTransitionEffect(),
            };
        }

        /// <summary>
        /// 黒フェード用の遷移効果を生成する
        /// </summary>
        public static ITransitionEffect[] CreateBlackTransitionEffects(bool immediate = false) {
            return new ITransitionEffect[] {
                new BlockTransitionEffect(),
                new FaderTransitionEffect(Color.black, immediate ? 0.0f : 0.2f),
            };
        }
    }
}