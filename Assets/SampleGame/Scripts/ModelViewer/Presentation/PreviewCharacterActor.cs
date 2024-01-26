using System;
using System.Linq;
using GameFramework.BodySystems;
using GameFramework.Core;
using GameFramework.PlayableSystems;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// オブジェクトプレビュー用のアクター
    /// </summary>
    public class PreviewCharacterActor : CharacterActor {
        private MotionController _motionController;
        private GimmickController _gimmickController;
        private AvatarController _avatarController;

        private MotionHandle _additiveMotionHandle;
        
        public PreviewActorSetupData SetupData { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PreviewCharacterActor(Body body, PreviewActorSetupData setupData)
            : base(body) {
            SetupData = setupData;
            _motionController = body.GetController<MotionController>();
            _gimmickController = body.GetController<GimmickController>();
            _avatarController = body.GetController<AvatarController>();
        }

        /// <summary>
        /// モーションの変更
        /// </summary>
        public void ChangeMotion(AnimationClip clip) {
            if (_motionController == null) {
                return;
            }

            ResetTransform();
            _motionController.Change(clip, 0.0f);
        }

        /// <summary>
        /// 加算モーションの変更
        /// </summary>
        public void ChangeAdditiveMotion(AnimationClip clip) {
            if (!_additiveMotionHandle.IsValid) {
                return;
            }
            
            ResetTransform();
            _additiveMotionHandle.Change(clip, 0.0f);
        }

        /// <summary>
        /// ギミックキーの一覧を取得
        /// </summary>
        /// <returns></returns>
        public string[] GetGimmickKeys() {
            if (_gimmickController == null) {
                return Array.Empty<string>();
            }
            
            return _gimmickController.GetKeys();
        }

        /// <summary>
        /// メッシュアバターの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, GameObject prefab) {
            if (_avatarController == null) {
                return;
            }
            
            if (prefab == null) {
                _avatarController.ResetPart(key);
            }
            else {
                _avatarController.ChangePart(new MeshAvatarResolver(key, prefab));
            }
        }

        /// <summary>
        /// Transformのリセット
        /// </summary>
        public void ResetTransform() {
            Body.LocalPosition = Vector3.zero;
            Body.LocalRotation = Quaternion.identity;
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
            }
            
            // 加算レイヤーの追加
            if (_motionController != null) {
                _additiveMotionHandle = _motionController.AddExtensionLayer(true, weight:1.0f);
                _additiveMotionHandle.ScopeTo(scope);
            }
        }
    }
}
