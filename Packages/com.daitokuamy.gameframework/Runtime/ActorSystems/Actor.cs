using System;
using GameFramework.BodySystems;
using GameFramework.LogicSystems;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body制御用ロジックのInterface
    /// </summary>
    public interface IActor : IDisposable {
    }

    /// <summary>
    /// Body制御用ロジック
    /// </summary>
    public abstract class Actor : Logic, IActor {
        // 制御対象のBody
        public Body Body { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="body">制御対象のBody</param>
        public Actor(Body body) {
            Body = body;
        }
    }
}