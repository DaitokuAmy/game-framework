using System;

namespace GameFramework.ActorSystems {
    /// <summary>
    /// Body制御インターフェース
    /// </summary>
    public interface IBodyComponent : IDisposable {
        // 実行優先度
        int ExecutionOrder { get; }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="body">親Body</param>
        void Initialize(Body body);

        /// <summary>
        /// 更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void Update(float deltaTime);

        /// <summary>
        /// 後更新処理
        /// </summary>
        /// <param name="deltaTime">変位時間</param>
        void LateUpdate(float deltaTime);
    }
}