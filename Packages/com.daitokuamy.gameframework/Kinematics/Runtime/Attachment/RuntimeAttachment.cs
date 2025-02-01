using UnityEngine;

namespace GameFramework.Kinematics {
    /// <summary>
    /// Attachmentの基底(Runtime追加用)
    /// </summary>
    [ExecuteAlways]
    public abstract class RuntimeAttachment : IAttachment {
        // 制御対象
        public Transform Owner => Resolver.Owner;

        // ターゲットリスト
        public AttachmentResolver.TargetSource[] Sources {
            set => Resolver.Sources = value;
        }
        // Transform制御用インスタンス
        protected abstract AttachmentResolver Resolver { get; }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void ManualUpdate() {
            Resolver.Resolve();
        }
    }
}