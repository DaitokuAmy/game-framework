using System;

namespace GameFramework.Core {
    /// <summary>
    /// アクターインターフェース用の共通
    /// </summary>
    public interface IActorInterface<TKey> : IDisposable {
        /// <summary>
        /// 有効状態になった際の処理
        /// </summary>
        void Activate();

        /// <summary>
        /// 無効状態になった際の処理
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Actorに設定された際の処理
        /// </summary>
        /// <param name="actor">所有者のアクター</param>
        void Attached(Actor<TKey> actor);

        /// <summary>
        /// Actorの設定から外された際の処理
        /// </summary>
        void Detached();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);
    }
}