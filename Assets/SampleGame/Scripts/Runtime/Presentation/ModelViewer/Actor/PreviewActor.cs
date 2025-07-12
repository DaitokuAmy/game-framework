using System;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.PlayableSystems;
using UnityEngine;

namespace SampleGame.Presentation.ModelViewer {
    /// <summary>
    /// オブジェクトプレビュー用のアクター
    /// </summary>
    public class PreviewActor : Actor {
        private MotionComponent _motionComponent;
        private GimmickComponent _gimmickComponent;
        private AvatarComponent _avatarComponent;

        private MotionHandle _additiveMotionHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActor(Body body) : base(body) {
            _motionComponent = body.GetBodyComponent<MotionComponent>();
            _gimmickComponent = body.GetBodyComponent<GimmickComponent>();
            _avatarComponent = body.GetBodyComponent<AvatarComponent>();
        }

        /// <summary>
        /// モーションの変更
        /// </summary>
        public void ChangeMotion(AnimationClip clip, bool transformReset = false) {
            if (_motionComponent == null) {
                return;
            }

            if (transformReset) {
                ResetTransform();
            }

            _motionComponent.Change(clip, 0.0f);
        }

        /// <summary>
        /// 加算モーションの変更
        /// </summary>
        public void ChangeAdditiveMotion(AnimationClip clip, bool transformReset = false) {
            if (!_additiveMotionHandle.IsValid) {
                return;
            }

            if (transformReset) {
                ResetTransform();
            }

            _additiveMotionHandle.Change(clip, 0.0f);
        }

        /// <summary>
        /// ギミックキーの一覧を取得
        /// </summary>
        /// <returns></returns>
        public string[] GetGimmickKeys() {
            if (_gimmickComponent == null) {
                return Array.Empty<string>();
            }

            return _gimmickComponent.GetKeys();
        }

        /// <summary>
        /// メッシュアバターの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, GameObject prefab, string locatorName) {
            if (_avatarComponent == null) {
                return;
            }

            if (prefab == null) {
                _avatarComponent.ResetPart(key);
            }
            else {
                _avatarComponent.ChangePart(new MeshAvatarResolver(key, prefab, locatorName));
            }
        }

        /// <summary>
        /// Transformのリセット
        /// </summary>
        public void ResetTransform() {
            Body.LocalPosition = Vector3.zero;
            Body.LocalRotation = Quaternion.identity;

            var animator = Body.GetComponent<Animator>();
            if (animator != null) {
                animator.rootPosition = Body.Position;
                animator.rootRotation = Body.Rotation;
            }

            var rigidbody = Body.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.isKinematic = false;
                rigidbody.Move(Body.Position, Body.Rotation);
                rigidbody.isKinematic = true;
            }
        }

        /// <summary>
        /// アクティブ時処理
        /// </summary>
        protected override void ActivateInternal(IScope scope) {
            base.ActivateInternal(scope);

            // 物理無効化
            var rigidbody = Body.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.isKinematic = true;
                rigidbody.interpolation = RigidbodyInterpolation.None;
            }

            // 加算レイヤーの追加
            if (_motionComponent != null) {
                _additiveMotionHandle = _motionComponent.AddExtensionLayer(true, weight: 1.0f);
                _additiveMotionHandle.RegisterTo(scope);
            }
        }
    }
}