using System;
using GameFramework.Core;

namespace GameFramework {
    /// <summary>
    /// 状態定義用インターフェース
    /// </summary>
    /// <typeparam name="TKey">識別キー</typeparam>
    public interface IState<TKey>
        where TKey : IComparable {
        // 識別キー
        TKey Key { get; }

        /// <summary>
        /// 状態に入った際の処理
        /// </summary>
        /// <param name="prevKey">入る前の状態キー</param>
        /// <param name="back">戻り遷移</param>
        /// <param name="scope">ExitまでのScope</param>
        void OnEnter(TKey prevKey, bool back, IScope scope);

        /// <summary>
        /// 状態に入っている際の更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// 状態を抜ける際の処理
        /// </summary>
        /// <param name="nextKey">抜ける先の状態キー</param>
        /// <param name="back">戻り遷移</param>
        void OnExit(TKey nextKey, bool back);
    }
}