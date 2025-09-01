using System;
using GameFramework;
using SampleGame.Presentation;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// SituationServiceのstaticメソッド機能置き場
    /// </summary>
    public partial class SituationService {
        /// <summary>
        /// Transitionの種類
        /// </summary>
        public enum TransitionType {
            /// <summary>通常画面遷移</summary>
            ScreenDefault,
            /// <summary>クロス画面遷移</summary>
            ScreenCross,
            /// <summary>通常シーン遷移</summary>
            SceneDefault,
        }

        private static ITransition OutInTransition => new OutInTransition();
        private static ITransition CrossTransition => new CrossTransition();
        private static ITransitionEffect[] BlockOnlyEffects => new ITransitionEffect[] { new BlockTransitionEffect() };
        private static ITransitionEffect[] LoadingEffects => new ITransitionEffect[] { new BlockTransitionEffect(), new LoadingTransitionEffect() };

        /// <summary>
        /// 遷移に使用する情報を取得
        /// </summary>
        private static (ITransition, ITransitionEffect[]) GetTransitionInfo(TransitionType type) {
            return type switch {
                TransitionType.ScreenDefault => (OutInTransition, BlockOnlyEffects),
                TransitionType.ScreenCross => (CrossTransition, BlockOnlyEffects),
                TransitionType.SceneDefault => (OutInTransition, LoadingEffects),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}