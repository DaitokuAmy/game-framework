using System;

namespace GameFramework {
    /// <summary>
    /// ステート管理用コンテナーインターフェース
    /// </summary>
    public interface IStateContainer<in TKey, TState, in TOption> : IDisposable
        where TState : class {
        /// <summary>現在のState</summary>
        TState Current { get; }
        /// <summary>遷移中か</summary>
        bool IsTransitioning { get; }

        /// <summary>
        /// Keyに登録されたStateを検索
        /// </summary>
        TState FindState(TKey key);

        /// <summary>
        /// 遷移可能なStateのリストを取得
        /// </summary>
        TState[] GetStates();
        
        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="key">遷移ターゲットを決めるキー</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="back">戻り遷移か</param>
        /// <param name="endStep">終了ステップ</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        TransitionHandle<TState> Transition(TKey key, TOption option, bool back, TransitionStep endStep, Action<TState> setupAction, ITransition transition, params ITransitionEffect[] effects);
        
        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="effects">遷移時演出</param>
        TransitionHandle<TState> Reset(Action<TState> setupAction, params ITransitionEffect[] effects);
    }
}