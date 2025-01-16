using System;
using GameFramework.Core;
using GameFramework.SituationSystems;

namespace SampleGame {
    /// <summary>
    /// SituationServiceのstaticメソッド機能置き場
    /// </summary>
    // public partial class SituationService {
    //     /// <summary>
    //     /// Transitionの種類
    //     /// </summary>
    //     public enum TransitionType {
    //         /// <summary> 通常（OutIn/ブロッキング）</summary>
    //         Default,
    //         /// <summary>クロス（Cross/ブロッキング）</summary>
    //         Cross,
    //         /// <summary>シーンまたぎ通常</summary>
    //         Scene,
    //     }
    //
    //     /// <summary>
    //     /// Resetの種類
    //     /// </summary>
    //     public enum ResetType {
    //         /// <summary>ブロッキングのみ</summary>
    //         BlockingOnly,
    //         /// <summary>黒フェードリセット</summary>
    //         FadingBlack,
    //         /// <summary>基本ローディング</summary>
    //         Loading,
    //         /// <summary>リゾート用ローディング</summary>
    //         LoadingResort,
    //     }
    //
    //     /// <summary>
    //     /// 指定したSituationへ遷移する
    //     /// </summary>
    //     public static IProcess Transition<T>(Action<T> onSetup, TransitionType transitionType = TransitionType.Default) where T : Situation {
    //         var transition = (ITransition)(transitionType switch {
    //             TransitionType.Cross => new CrossTransition(),
    //             _ => new OutInTransition()
    //         });
    //         var effects = transitionType switch {
    //             TransitionType.Default => BlockingEffects,
    //             TransitionType.Cross => BlockingEffects,
    //             TransitionType.Scene => LoadingEffects,
    //             _ => throw new ArgumentOutOfRangeException()
    //         };
    //         return Services.Get<SituationService>().Transition(onSetup, transition, effects);
    //     }
    //
    //     /// <summary>
    //     /// 指定したSituationへ遷移する
    //     /// </summary>
    //     public static IProcess Transition<T>(TransitionType transitionType = TransitionType.Default) where T : Situation {
    //         return Transition<T>(null, transitionType);
    //     }
    //
    //     /// <summary>
    //     /// 戻り遷移
    //     /// </summary>
    //     public static IProcess Back(TransitionType transitionType = TransitionType.Default) {
    //         var transition = (ITransition)(transitionType switch {
    //             TransitionType.Cross => new CrossTransition(),
    //             _ => new OutInTransition()
    //         });
    //         var effects = transitionType switch {
    //             TransitionType.Default => BlockingEffects,
    //             TransitionType.Cross => BlockingEffects,
    //             TransitionType.Scene => LoadingEffects,
    //             _ => throw new ArgumentOutOfRangeException()
    //         };
    //         return Services.Get<SituationService>().Back(transition, effects);
    //     }
    //
    //     /// <summary>
    //     /// 現在のSituationNodeをリセット
    //     /// </summary>
    //     public static IProcess Reset(Action<Situation> onSetup, ResetType resetType = ResetType.BlockingOnly) {
    //         var effects = resetType switch {
    //             ResetType.BlockingOnly => BlockingEffects,
    //             ResetType.FadingBlack => FadingBlackEffects,
    //             ResetType.Loading => LoadingEffects,
    //             _ => throw new ArgumentOutOfRangeException()
    //         };
    //         return Services.Get<SituationService>().Reset(onSetup, effects);
    //     }
    //
    //     /// <summary>
    //     /// 現在のSituationNodeをリセット
    //     /// </summary>
    //     public static IProcess Reset(ResetType resetType = ResetType.BlockingOnly) {
    //         return Reset(null, resetType);
    //     }
    // }
}