using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using UnityEngine;

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
        /// <summary>AnimationClipの総数</summary>
        int AnimationClipCount { get; }

        /// <summary>マスター</summary>
        IPreviewActorMaster Master { get; }
        /// <summary>位置</summary>
        Vector3 Position { get; }
        /// <summary>向き</summary>
        Quaternion Rotation { get; }
        /// <summary>現在再生中のClip</summary>
        AnimationClip CurrentAnimationClip { get; }
        /// <summary>現在再生中の加算Clip</summary>
        AnimationClip CurrentAdditiveAnimationClip { get; }
        /// <summary>現在設定されているMeshAvatarのパーツ</summary>
        IReadOnlyDictionary<string, GameObject> CurrentMeshAvatars { get; }
    }

    /// <summary>
    /// アクターモデル
    /// </summary>
    public class PreviewActorModel : AutoIdModel<PreviewActorModel>, IReadOnlyPreviewActorModel {
        private Dictionary<string, GameObject> _currentMeshAvatars;
        private IPreviewActorPort _previewActorPort;

        /// <summary>有効か</summary>
        public bool IsActive => _previewActorPort != null;

        /// <summary>現在再生中のAnimationClipIndex</summary>
        public int CurrentAnimationClipIndex { get; private set; }
        /// <summary>現在再生中の加算AnimationClipIndex</summary>
        public int CurrentAdditiveAnimationClipIndex { get; private set; }
        /// <summary>AnimationClipの総数</summary>
        public int AnimationClipCount => Master?.AnimationClips.Count ?? 0;

        /// <summary>マスター</summary>
        public IPreviewActorMaster Master { get; private set; }
        /// <summary>位置</summary>
        public Vector3 Position => _previewActorPort?.Position ?? Vector3.zero;
        /// <summary>向き</summary>
        public Quaternion Rotation => _previewActorPort?.Rotation ?? Quaternion.identity;
        /// <summary>現在再生中のClip</summary>
        public AnimationClip CurrentAnimationClip { get; private set; }
        /// <summary>現在再生中の加算Clip</summary>
        public AnimationClip CurrentAdditiveAnimationClip { get; private set; }
        /// <summary>現在設定されているMeshAvatarのパーツ</summary>
        public IReadOnlyDictionary<string, GameObject> CurrentMeshAvatars => _currentMeshAvatars;

        /// <summary>
        /// 生成時処理
        /// </summary>
        protected override void OnCreatedInternal(IScope scope) {
            base.OnCreatedInternal(scope);

            _currentMeshAvatars = new Dictionary<string, GameObject>();
        }

        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(IPreviewActorMaster master) {
            // マスター設定
            Master = master;

            // 現在のアバター情報をクリアする
            _currentMeshAvatars.Clear();

            // Avatar情報更新
            foreach (var info in master.MeshAvatarInfos) {
                ChangeMeshAvatar(info.Key, info.DefaultIndex);
            }

            // 初期状態のクリップを設定
            ChangeAnimationClip(master.DefaultAnimationClipIndex, true);
            ToggleAdditiveAnimationClip(-1);
        }

        /// <summary>
        /// コントローラーの設定
        /// </summary>
        public void SetPort(IPreviewActorPort port) {
            _previewActorPort = port;

            // 初期化
            if (IsActive) {
                _previewActorPort.ChangeAnimationClip(CurrentAnimationClip);
                _previewActorPort.ChangeAdditiveAnimationClip(CurrentAdditiveAnimationClip);
                foreach (var pair in _currentMeshAvatars) {
                    var avatarInfo = Master.MeshAvatarInfos.FirstOrDefault(x => x.Key == pair.Key);
                    var locatorName = avatarInfo?.LocatorName ?? "";
                    _previewActorPort.ChangeMeshAvatar(pair.Key, pair.Value, locatorName);
                }
            }
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// ※同じClipを設定したら再度再生
        /// </summary>
        public void ChangeAnimationClip(int clipIndex, bool reset) {
            CurrentAnimationClipIndex = -1;
            CurrentAnimationClip = null;
            var clip = GetAnimationClip(clipIndex);
            if (clip != null) {
                CurrentAnimationClipIndex = clipIndex;
                CurrentAnimationClip = clip;
            }

            if (IsActive) {
                if (reset) {
                    _previewActorPort.ResetActor();
                }

                _previewActorPort.ChangeAnimationClip(CurrentAnimationClip);
            }
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ToggleAdditiveAnimationClip(int clipIndex) {
            var currentClip = CurrentAdditiveAnimationClip;
            var nextClip = GetAnimationClip(clipIndex);
            CurrentAdditiveAnimationClipIndex = -1;
            CurrentAdditiveAnimationClip = null;
            if (currentClip != nextClip && nextClip != null) {
                CurrentAdditiveAnimationClipIndex = clipIndex;
                CurrentAdditiveAnimationClip = nextClip;
            }

            if (IsActive) {
                _previewActorPort.ChangeAdditiveAnimationClip(CurrentAnimationClip);
            }
        }

        /// <summary>
        /// MeshAvatarの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            var avatarInfo = Master.MeshAvatarInfos.FirstOrDefault(x => x.Key == key);
            var prefab = default(GameObject);
            var locatorName = "";
            if (avatarInfo != null) {
                prefab = index >= 0 && index < avatarInfo.Prefabs.Count ? avatarInfo.Prefabs[index] : null;
                locatorName = avatarInfo.LocatorName;
                _currentMeshAvatars[key] = prefab;
            }

            if (IsActive) {
                _previewActorPort.ChangeMeshAvatar(key, prefab, locatorName);
            }
        }

        /// <summary>
        /// 状態のリセット
        /// </summary>
        public void ResetActor() {
            _previewActorPort.ResetActor();
        }

        /// <summary>
        /// AnimationClipの取得
        /// </summary>
        private AnimationClip GetAnimationClip(int index) {
            if (Master == null) {
                return null;
            }

            var clips = Master.AnimationClips;

            if (index < 0 || index >= clips.Count) {
                return null;
            }

            return clips[index];
        }
    }
}