using System;

namespace GameFramework.ActorSystem {
    /// <summary>
    /// アクターインターフェース用の共通部分
    /// </summary>
    public interface IActorInterface : IDisposable {
        /// <summary>
        /// 有効状態になった際の処理
        /// </summary>
        void Activate();

        /// <summary>
        /// 無効状態になった際の処理
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);
    }
}
