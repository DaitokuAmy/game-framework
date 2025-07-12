using UnityEngine;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body構築用インターフェース
    /// </summary>
    public interface IBodyBuilder {
        /// <summary>
        /// 構築処理
        /// </summary>
        /// <param name="body">構築対象のBody</param>
        /// <param name="gameObject">制御対象のGameObject</param>
        void Build(Body body, GameObject gameObject);
    }
}