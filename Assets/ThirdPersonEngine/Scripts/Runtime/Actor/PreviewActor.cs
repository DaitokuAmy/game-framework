using System;
using System.Linq;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.PlayableSystems;
using GluonGui.WorkspaceWindow.Views;
using UnityEngine;

namespace ThirdPersonEngine.ModelViewer {
    /// <summary>
    /// オブジェクトプレビュー用のアクター
    /// </summary>
    public class PreviewActor : Actor {
        private readonly PreviewActorData _actorData;
        private readonly MotionComponent _motionComponent;
        private readonly GimmickComponent _gimmickComponent;
        private readonly AvatarComponent _avatarComponent;

        private MotionHandle _additiveMotionHandle;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewActor(Body body, PreviewActorData actorData) : base(body) {
            _actorData = actorData;
            _motionComponent = body.GetBodyComponent<MotionComponent>();
            _gimmickComponent = body.GetBodyComponent<GimmickComponent>();
            _avatarComponent = body.GetBodyComponent<AvatarComponent>();
        }

        /// <summary>
        /// モーションの変更
        /// </summary>
        public void ChangeMotion(int index, bool transformReset = false) {
            if (_motionComponent == null) {
                return;
            }

            if (index < 0 || index >= _actorData.animationClips.Length) {
                return;
            }

            var clip = _actorData.animationClips[index];

            if (transformReset) {
                ResetTransform();
            }

            _motionComponent.Change(clip, 0.0f);
        }

        /// <summary>
        /// 加算モーションの変更
        /// </summary>
        public void ChangeAdditiveMotion(int index, bool transformReset = false) {
            if (!_additiveMotionHandle.IsValid) {
                return;
            }

            if (index < 0 || index >= _actorData.animationClips.Length) {
                return;
            }

            var clip = _actorData.animationClips[index];

            if (transformReset) {
                ResetTransform();
            }

            _additiveMotionHandle.Change(clip, 0.0f);
        }

        /// <summary>
        /// モーションクリップの一覧を取得
        /// </summary>
        public AnimationClip[] GetMotionClips() {
            return _actorData.animationClips;
        }

        /// <summary>
        /// デフォルトのMotionIndexを取得
        /// </summary>
        public int GetDefaultMotionIndex() {
            return _actorData.defaultAnimationClipIndex;
        }

        /// <summary>
        /// デフォルトのMeshAvatarIndexを取得
        /// </summary>
        public int GetDefaultMeshAvatarIndex(string key) {
            var info = _actorData.meshAvatarInfos.FirstOrDefault(x => x.key == key);
            if (info == null) {
                return 0;
            }

            return info.defaultIndex;
        }

        /// <summary>
        /// MeshAvatarのキー一覧を取得
        /// </summary>
        public string[] GetMeshAvatarKeys() {
            return _actorData.meshAvatarInfos.Select(x => x.key).ToArray();
        }

        /// <summary>
        /// MeshAvatarで取り替え可能なPrefab一覧を取得
        /// </summary>
        public GameObject[] GetMeshAvatarPrefabs(string key) {
            var info = _actorData.meshAvatarInfos.FirstOrDefault(x => x.key == key);
            if (info == null) {
                return Array.Empty<GameObject>();
            }

            return info.prefabs;
        }

        /// <summary>
        /// MeshAvatarの数を取得
        /// </summary>
        public int GetMeshAvatarCount(string key) {
            var info = _actorData.meshAvatarInfos.FirstOrDefault(x => x.key == key);
            if (info == null) {
                return 0;
            }

            return info.prefabs.Length;
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
        public void ChangeMeshAvatar(string key, int index) {
            if (_avatarComponent == null) {
                return;
            }

            if (index < 0) {
                _avatarComponent.ResetPart(key);
            }
            else {
                var info = _actorData.meshAvatarInfos.FirstOrDefault(x => x.key == key);
                if (info != null) {
                    _avatarComponent.ChangePart(new MeshAvatarResolver(key, info.prefabs[index], info.locatorName));
                }
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