using System;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移用インターフェース
    /// </summary>
    public interface ISituationFlow : IDisposable, IMonitoredFlow {
        /// <summary>現在のSituation</summary>
        Situation Current { get; }
        /// <summary>トランジション中か</summary>
        bool IsTransitioning { get; }

        /// <summary>
        /// 遷移可能なSituationのリストを取得
        /// </summary>
        Situation[] GetSituations();

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Transition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation;

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Transition<TSituation>(ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation;

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Transition<TSituation>(Action<TSituation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation;

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="type">遷移先を表すSituatonのType</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Transition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects);

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        TransitionHandle RefreshTransition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation;

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle RefreshTransition<TSituation>(ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation;

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle RefreshTransition<TSituation>(Action<TSituation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation;

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="type">遷移先を表すSituatonのType</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle RefreshTransition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects);

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Back(int depth, params ITransitionEffect[] effects);

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Back(params ITransitionEffect[] effects);

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Back(int depth, ITransition overrideTransition = null, params ITransitionEffect[] effects);

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Back(ITransition overrideTransition = null, params ITransitionEffect[] effects);

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Back(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects);

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Back(int depth, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects);

        /// <summary>
        /// 現在のSituationをリセットする
        /// </summary>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Reset(params ITransitionEffect[] effects);

        /// <summary>
        /// 現在のSituationをリセットする
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="effects">遷移演出</param>
        TransitionHandle Reset(Action<Situation> onSetup = null, params ITransitionEffect[] effects);
    }
}