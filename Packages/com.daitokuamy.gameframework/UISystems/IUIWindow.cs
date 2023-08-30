using System;

namespace GameFramework.UISystems {    
    /// <summary>
    /// UIWindowインターフェース
    /// </summary>
    public interface IUIWindow : IDisposable {
        /// <summary>
        /// 初期化処理
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
    }
}