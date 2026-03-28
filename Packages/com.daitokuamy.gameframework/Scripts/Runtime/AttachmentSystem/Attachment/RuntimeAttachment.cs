using UnityEngine;

namespace GameFramework.AttachmentSystem {
    /// <summary>
    /// Attachmentの基底(Runtime追加用)
    /// </summary>
    [ExecuteAlways]
    public abstract class RuntimeAttachment : IAttachment {
        /// <summary>制御対象</summary>
        public Transform Owner => Resolver.Owner;

        /// <summary>ターゲットリスト</summary>
        public AttachmentResolver.TargetSource[] Sources {
            set => Resolver.Sources = value;
        }
        /// <summary>Transform制御用インスタンス</summary>
        protected abstract AttachmentResolver Resolver { get; }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void ManualUpdate() {
            Resolver.Resolve();
        }
    }
}
