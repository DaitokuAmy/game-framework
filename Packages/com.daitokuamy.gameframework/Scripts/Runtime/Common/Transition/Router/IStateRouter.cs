using System;

namespace GameFramework {
    /// <summary>
    /// ステート遷移ルーティング用インターフェース
    /// </summary>
    public interface IStateRouter<TKey, TState, in TOption> : IDisposable, IMonitoredStateRouter
        where TState : class {
        /// <summary>現在のState</summary>
        TState Current { get; }
        /// <summary>現在のStateKey</summary>
        TKey CurrentKey { get; }
        /// <summary>トランジション中か</summary>
        bool IsTransitioning { get; }

        /// <summary>
        /// 遷移可能なStateのリストを取得
        /// </summary>
        TState[] GetStates();
        
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="key">遷移ターゲットを決めるキー</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="step">終了ステップ</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        TransitionHandle<TState> Transition(TKey key, TOption option = default, TransitionStep step = TransitionStep.Complete, Action<TState> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects);
        
        /// <summary>
        /// 戻り遷移処理
        /// </summary>
        /// <param name="depth">戻り階層数(1～)</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        TransitionHandle<TState> Back(int depth = 1, TOption option = default, Action<TState> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects);
        
        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="effects">遷移時演出</param>
        TransitionHandle<TState> Reset(Action<TState> setupAction = null, params ITransitionEffect[] effects);
    }
}