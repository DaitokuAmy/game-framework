using System;
using GameFramework.Core;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Body(管理インターフェース用)
    /// </summary>
    public interface IBody : IDisposable, IScope {
        /// <summary>
        /// 初期化処理(Controllerが全て追加された後の処理)
        /// </summary>
        void Initialize();

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

        /// <summary>
        /// BodyControllerの追加
        /// </summary>
        /// <param name="controller">対象のController</param>
        void AddController(IBodyController controller);
    }
}