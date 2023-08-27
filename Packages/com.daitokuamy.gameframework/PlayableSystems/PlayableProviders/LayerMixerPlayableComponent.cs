using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimatorControllerを再生するPlayable用のProvider
    /// </summary>
    public class LayerMixerPlayableComponent : PlayableComponent<AnimationLayerMixerPlayable> {
        private readonly List<MotionCrossFader> _extensionCrossFaders = new();

        private Animator _animator;
        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _playable;
        private MotionCrossFader _baseCrossFader;
        private float _speed;

        /// <summary>基礎となるPlayable</summary>
        public override AnimationLayerMixerPlayable Playable => _playable;
        /// <summary>ベースレイヤー用ハンドル</summary>
        public MotionHandle BaseHandle { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="autoDispose">自動廃棄するか</param>
        public LayerMixerPlayableComponent(Animator animator, bool autoDispose)
            : base(autoDispose) {
            _animator = animator;
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        protected override Playable CreatePlayable(PlayableGraph graph) {
            _graph = graph;
            _playable = AnimationLayerMixerPlayable.Create(graph);
            _baseCrossFader = new MotionCrossFader(graph, _animator);
            _playable.AddInput(_baseCrossFader.Playable, 0, 1.0f);
            BaseHandle = new MotionHandle(this, _baseCrossFader);
            return _playable;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void UpdateInternal(float deltaTime) {
            _baseCrossFader.Update(deltaTime);
            foreach (var fader in _extensionCrossFaders) {
                fader.Update(deltaTime);
            }
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected override void DisposeInternal() {
            foreach (var fader in _extensionCrossFaders) {
                fader.Dispose();
            }

            _extensionCrossFaders.Clear();
            _baseCrossFader.Dispose();
            _baseCrossFader = null;
        }

        /// <summary>
        /// 速度の変更
        /// </summary>
        protected override void SetSpeedInternal(float speed) {
            _baseCrossFader.SetSpeed(speed);
            foreach (var fader in _extensionCrossFaders) {
                fader.SetSpeed(speed);
            }
        }

        /// <summary>
        /// 拡張レイヤーの追加
        /// </summary>
        /// <param name="additive">加算レイヤーか</param>
        /// <param name="avatarMask">アバターマスク</param>
        /// <param name="weight">初期ウェイト</param>
        public MotionHandle AddExtensionLayer(bool additive = false, AvatarMask avatarMask = null, float weight = 1.0f) {
            var crossFader = new MotionCrossFader(_graph, _animator);
            crossFader.SetSpeed((float)_playable.GetSpeed());

            var index = (uint)_playable.AddInput(crossFader.Playable, 0, weight);
            _playable.SetLayerAdditive(index, additive);
            if (avatarMask != null) {
                _playable.SetLayerMaskFromAvatarMask(index, avatarMask);
            }

            _extensionCrossFaders.Add(crossFader);

            return new MotionHandle(this, crossFader);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        /// <param name="handle">対象のレイヤーを表すHandle</param>
        public void RemoveExtensionLayer(MotionHandle handle) {
            if (!handle.IsValid) {
                return;
            }

            var index = _extensionCrossFaders.IndexOf(handle.CrossFader);

            // 含まれていなければ何もしない
            if (index < 0) {
                return;
            }

            // 除外
            _extensionCrossFaders.RemoveAt(index);

            // 接続の解除
            _playable.DisconnectInput(index + 1);

            // 削除したCrossFaderをDispose
            handle.CrossFader.Dispose();

            handle.Dispose();
        }

        /// <summary>
        /// 拡張レイヤーの全削除
        /// </summary>
        public void RemoveExtensionLayers() {
            for (var i = _extensionCrossFaders.Count - 1; i >= 0; i--) {
                var fader = _extensionCrossFaders[i];
                _playable.DisconnectInput(i + 1);
                fader.Dispose();
            }
        }

        /// <summary>
        /// レイヤーウェイトの変更
        /// </summary>
        /// <param name="handle">対象のレイヤーを表すHandle</param>
        /// <param name="weight">ウェイト</param>
        public void SetLayerWeight(MotionHandle handle, float weight) {
            if (!handle.IsValid) {
                return;
            }

            var index = _extensionCrossFaders.IndexOf(handle.CrossFader);

            // 含まれていなければ何もしない
            if (index < 0) {
                return;
            }

            // ウェイトの変更
            _playable.SetInputWeight(index, weight);
        }
    }
}