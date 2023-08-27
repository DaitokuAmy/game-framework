using System.Linq;
using GameFramework.ModelSystems;
using UniRx;
using UnityEngine;

namespace SampleGame.ModelViewer {
    /// <summary>
    /// 表示用アクターモデル
    /// </summary>
    public class PreviewActorModel : AutoIdModel<PreviewActorModel> {
        private readonly ReactiveProperty<PreviewActorSetupData> _setupData = new();
        private readonly ReactiveProperty<AnimationClip> _currentAnimationClip = new();
        private readonly ReactiveProperty<AnimationClip> _currentAdditiveAnimationClip = new();
        private readonly ReactiveDictionary<string, GameObject> _currentMeshAvatars = new();

        /// <summary>初期化データID</summary>
        public string SetupDataId { get; private set; }
        /// <summary>現在再生中のAnimationClipIndex</summary>
        public int CurrentAnimationClipIndex { get; private set; }
        /// <summary>現在再生中の加算AnimationClipIndex</summary>
        public int CurrentAdditiveAnimationClipIndex { get; private set; }
        /// <summary>AnimationClipの総数</summary>
        public int AnimationClipCount => _setupData != null ? _setupData.Value.animationClips.Length : 0;

        /// <summary>Actor初期化用データ</summary>
        public IReadOnlyReactiveProperty<PreviewActorSetupData> SetupData => _setupData;
        /// <summary>現在再生中のClip</summary>
        public IReadOnlyReactiveProperty<AnimationClip> CurrentAnimationClip => _currentAnimationClip;
        /// <summary>現在再生中の加算Clip</summary>
        public IReadOnlyReactiveProperty<AnimationClip> CurrentAdditiveAnimationClip => _currentAdditiveAnimationClip;
        /// <summary>現在設定されているMeshAvatarのパーツ</summary>
        public IReadOnlyReactiveDictionary<string, GameObject> CurrentMeshAvatars => _currentMeshAvatars;

        /// <summary>
        /// 削除時処理
        /// </summary>
        protected override void OnDeletedInternal() {
            _setupData.Dispose();
            _currentAnimationClip.Dispose();
            _currentAdditiveAnimationClip.Dispose();
            _currentMeshAvatars.Dispose();
            
            base.OnDeletedInternal();
        }
        
        /// <summary>
        /// データの設定
        /// </summary>
        public void Setup(string setupDataId, PreviewActorSetupData actorSetupData) {
            // 現在のクリップをクリアする
            ChangeClip(-1);
            ToggleAdditiveClip(-1);
            
            // 現在のアバター情報をクリアする
            _currentMeshAvatars.Clear();

            // ID記憶
            SetupDataId = setupDataId;

            // ActorData更新
            _setupData.Value = actorSetupData;

            // Avatar情報更新
            if (actorSetupData != null) {
                foreach (var info in actorSetupData.meshAvatarInfos) {
                    ChangeMeshAvatar(info.key, info.defaultIndex);
                }
            }

            // 初期状態のクリップを設定
            ChangeClip(0);
            ToggleAdditiveClip(-1);
        }

        /// <summary>
        /// アニメーションクリップの変更
        /// ※同じClipを設定したら再度再生
        /// </summary>
        public void ChangeClip(int clipIndex) {
            CurrentAnimationClipIndex = -1;
            _currentAnimationClip.Value = null;
            var clip = GetAnimationClip(clipIndex);
            if (clip != null) {
                CurrentAnimationClipIndex = clipIndex;
                _currentAnimationClip.Value = clip;
            }
        }

        /// <summary>
        /// 加算用アニメーションクリップの変更
        /// ※同じClipを設定したらトグル
        /// </summary>
        public void ToggleAdditiveClip(int clipIndex) {
            var currentClip = _currentAdditiveAnimationClip.Value;
            var nextClip = GetAnimationClip(clipIndex);
            CurrentAdditiveAnimationClipIndex = -1;
            _currentAdditiveAnimationClip.Value = null;
            if (currentClip != nextClip && nextClip != null) {
                CurrentAdditiveAnimationClipIndex = clipIndex;
                _currentAdditiveAnimationClip.Value = nextClip;
            }
        }

        /// <summary>
        /// MeshAvatarの変更
        /// </summary>
        public void ChangeMeshAvatar(string key, int index) {
            var avatarInfo = _setupData.Value.meshAvatarInfos.FirstOrDefault(x => x.key == key);
            if (avatarInfo != null) {
                _currentMeshAvatars[key] = index >= 0 && index < avatarInfo.prefabs.Length ? avatarInfo.prefabs[index] : null;
            }
        }

        /// <summary>
        /// AnimationClipの取得
        /// </summary>
        private AnimationClip GetAnimationClip(int index) {
            if (_setupData.Value == null) {
                return null;
            }

            var clips = _setupData.Value.animationClips;

            if (index < 0 || index >= clips.Length) {
                return null;
            }

            return clips[index];
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private PreviewActorModel(int id)
            : base(id) {
        }
    }
}