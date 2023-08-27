using System;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Entity拡張用コンポーネントインターフェース
    /// </summary>
    public interface IComponent : IDisposable {
        /// <summary>
        /// Entityに接続された時の処理
        /// </summary>
        /// <param name="actorEntity">接続対象のEntity</param>
        void Attached(ActorEntity actorEntity);

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        void Activate();

        /// <summary>
        /// 非アクティブ時処理
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Entityから接続外れた時の処理
        /// </summary>
        /// <param name="actorEntity">接続外れたEntity</param>
        void Detached(ActorEntity actorEntity);
    }
}