using System;
using GameFramework.SituationSystems;
using SampleGame.Presentation;

namespace SampleGame {
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

        /// <summary>
        /// 遷移に使用する情報を取得
        /// </summary>
        private static (ITransition, ITransitionEffect[]) GetTransitionInfo(TransitionType type) {
            return type switch {
                TransitionType.ScreenDefault => (new OutInTransition(), new ITransitionEffect[]{ new BlockTransitionEffect() }),
                TransitionType.ScreenCross => (new CrossTransition(), new ITransitionEffect[]{ new BlockTransitionEffect() }),
                TransitionType.SceneDefault => (new OutInTransition(), new ITransitionEffect[]{ new BlockTransitionEffect(), new LoadingTransitionEffect() }),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}