using System.Collections.Generic;
using GameFramework.Core;
using Unity.Mathematics;

namespace SampleGame.Domain.ModelViewer {
    /// <summary>
    /// 読み取り専用インターフェース
    /// </summary>
    public interface IReadOnlyPreviewActorModel {
        /// <summary>識別子</summary>
        int Id { get; }

        /// <summary>現在再生中のAnimationClipIndex</summary>
        int CurrentAnimationClipIndex { get; }
        /// <summary>現在再生中の加算AnimationClipIndex</summary>
        int CurrentAdditiveAnimationClipIndex { get; }
        /// <summary>現在設定されているMeshAvatarのパーツのIndex</summary>
        IReadOnlyDictionary<string, int> CurrentMeshAvatarIndices { get; }
        /// <summary>AnimationClipの総数</summary>
        int AnimationClipCount { get; }

        /// <summary>マスター</summary>
        IModelViewerActorMaster Master { get; }
        /// <summary>位置</summary>
        float3 Position { get; }
        /// <summary>向き</summary>
        quaternion Rotation { get; }
    }

    /// <summary>
    /// アクターモデル
    /// </summary>
    public class PreviewActorModel : AutoIdModel<PreviewActorModel>, IReadOnlyPreviewActorModel {
        private Dictionary<string, int> _currentMeshAvatarIndices;
        private IPreviewActorPort _previewActorPort;

        /// <summary>有効か</summary>
        public bool IsActive => _previewActorPort != null;

        /// <inheritdoc/>
        public int CurrentAnimationClipIndex { get; private set; }
        /// <inheritdoc/>
        public int CurrentAdditiveAnimationClipIndex { get; private set; }
        /// <inheritdoc/>
        public int AnimationClipCount => _previewActorPort?.AnimationClipCount ?? 0;
        /// <inheritdoc/>
        public IReadOnlyDictionary<string, int> CurrentMeshAvatarIndices => _currentMeshAvatarIndices;

        /// <inheritdoc/>
        public IModelViewerActorMaster Master { get; private set; }
        /// <inheritdoc/>
        public float3 Position => _previewActorPort?.Position ?? float3.zero;
        /// <inheritdoc/>
        public quaternion Rotation => _previewActorPort?.Rotation ?? quaternion.identity;

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            _currentMeshAvatarIndices = new Dictionary<string, int>();
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IModelViewerActorMaster master) {
            // マスター設定
            Master = master;

            // 情報初期化
            _currentMeshAvatarIndices.Clear();
            CurrentAnimationClipIndex = -1;
            CurrentAdditiveAnimationClipIndex = -1;
        }

        /// <summary>
        /// Portの設定
        /// </summary>
        public void SetPort(IPreviewActorPort port) {
            _previewActorPort = port;

            // 初期化
            ChangeAnimationClip(_previewActorPort.GetDefaultAnimationClipIndex(), true);

            var meshAvatarKeys = _previewActorPort.GetMeshAvatarKeys();
            foreach (var key in meshAvatarKeys) {
                ChangeMeshAvatar(key, _previewActorPort.GetDefaultMeshAvatarIndex(key));
            }
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// ※同じClipを設定したら再度再生
        /// </summary>
        public void ChangeAnimationClip(int clipIndex, bool reset) {
            if (!IsActive) {
                return;
            }

            var count = _previewActorPort.AnimationClipCount;
            clipIndex = IntMath.Clamp(clipIndex, -1, count - 1);

            if (reset) {
                _previewActorPort.ResetActor();
            }

            CurrentAnimationClipIndex = clipIndex;
            _previewActorPort.ChangeAnimationClip(CurrentAnimationClipIndex);
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ToggleAdditiveAnimationClip(int clipIndex) {
            if (!IsActive) {
                return;
            }

            var count = _previewActorPort.AnimationClipCount;
            clipIndex = IntMath.Clamp(clipIndex, -1, count - 1);

            var currentIndex = CurrentAdditiveAnimationClipIndex;
            var nextIndex = clipIndex;
            CurrentAdditiveAnimationClipIndex = -1;
            if (currentIndex != nextIndex) {
                CurrentAdditiveAnimationClipIndex = nextIndex;
            }

            _previewActorPort.ChangeAdditiveAnimationClip(CurrentAdditiveAnimationClipIndex);
        }

        /// <summary>
        /// MeshAvatarの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            if (IsActive) {
                return;
            }

            var count = _previewActorPort.GetMeshAvatarCount(key);
            if (count <= 0) {
                return;
            }

            index = IntMath.Clamp(index, -1, count - 1);

            _currentMeshAvatarIndices[key] = index;
            _previewActorPort.ChangeMeshAvatar(key, index);
        }

        /// <summary>
        /// 状態のリセット
        /// </summary>
        public void ResetActor() {
            _previewActorPort.ResetActor();
        }
    }
}