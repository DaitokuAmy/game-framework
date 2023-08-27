﻿using System.Collections.Generic;
using System.Linq;
using GameFramework.Kinematics;

namespace GameFramework.BodySystems {
    /// <summary>
    /// Attachment制御クラス
    /// </summary>
    public class AttachmentController : BodyController {
        // Attachmentリスト
        private List<IAttachment> _attachments = new List<IAttachment>();
        // 外部から追加されたAttachmentリスト
        private readonly List<IAttachment> _customAttachments = new List<IAttachment>();

        // 実行優先度
        public override int ExecutionOrder => 16;

        /// <summary>
        /// Attachment追加
        /// </summary>
        public void RegisterConstraint(IAttachment attachment) {
            _customAttachments.Add(attachment);
        }

        /// <summary>
        /// Attachmentの解除
        /// </summary>
        public void UnregisterAttachment(IAttachment attachment) {
            _customAttachments.Remove(attachment);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected override void InitializeInternal() {
            var meshController = Body.GetController<MeshController>();

            if (meshController != null) {
                meshController.OnRefreshed += RefreshAttachments;
            }

            RefreshAttachments();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void LateUpdateInternal(float deltaTime) {
            // 各種Transform更新
            foreach (var attachment in _attachments) {
                attachment.ManualUpdate();
            }

            foreach (var attachment in _customAttachments) {
                attachment.ManualUpdate();
            }
        }

        /// <summary>
        /// Attachmentリストのリフレッシュ
        /// </summary>
        private void RefreshAttachments() {
            // 階層に含まれているAttachmentを探す
            var bodyTransform = Body.Transform;
            _attachments = bodyTransform.GetComponentsInChildren<IAttachment>(true)
                .Where(x => x != null)
                .ToList();

            // ターゲット情報の更新
            foreach (var attachment in _attachments) {
                if (attachment is Attachment a) {
                    a.UpdateMode = Attachment.Mode.Manual;
                }
            }
        }
    }
}