using System;

namespace GameFramework.CollisionSystems {
    /// <summary>
    /// コリジョン管理用ハンドル
    /// </summary>
    public struct CollisionHandle : IDisposable {
        private readonly CollisionManager _manager;

        /// <summary>有効なハンドルか</summary>
        internal bool IsValid => CollisionInfo != null && CollisionInfo.IsValid;
        /// <summary>コリジョン情報</summary>
        internal CollisionManager.ICollisionInfo CollisionInfo { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal CollisionHandle(CollisionManager manager, CollisionManager.ICollisionInfo collisionInfo) {
            _manager = manager;
            CollisionInfo = collisionInfo;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (!IsValid) {
                return;
            }
            
            _manager?.Unregister(this);
            CollisionInfo = null;
        }
    }
}