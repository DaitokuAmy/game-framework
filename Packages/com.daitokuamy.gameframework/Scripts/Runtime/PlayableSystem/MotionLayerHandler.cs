using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// モーション再生のレイヤーをハンドリングするクラス
    /// </summary>
    internal sealed class MotionLayerHandler : IDisposable {
        private readonly List<MotionCrossFader> _extensionCrossFaders = new();
        private readonly Animator _animator;

        private bool _disposed;
        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _playable;
        private MotionCrossFader _baseCrossFader;
        private float _speed;

        /// <summary>ベース用のモーション再生用ハンドル</summary>
        public MotionHandle BaseHandle { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="animator">初期化に使うAnimator</param>
        public MotionLayerHandler(Animator animator) {
            _animator = animator;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;

            BaseHandle.Dispose();
            foreach (var fader in _extensionCrossFaders) {
                fader.Dispose();
            }

            _extensionCrossFaders.Clear();
            _baseCrossFader.Dispose();
            _baseCrossFader = null;

            _playable.Destroy();
        }

        /// <summary>
        /// Playableの生成
        /// </summary>
        public Playable CreatePlayable(PlayableGraph graph) {
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
        public void Update(float deltaTime) {
            if (_disposed) {
                return;
            }

            _baseCrossFader.Update(deltaTime);
            foreach (var fader in _extensionCrossFaders) {
                fader.Update(deltaTime);
            }
        }

        /// <summary>
        /// 速度の変更
        /// </summary>
        public void SetSpeed(float speed) {
            if (_disposed) {
                return;
            }

            _baseCrossFader.SetSpeed(speed);
            foreach (var fader in _extensionCrossFaders) {
                fader.SetSpeed(speed);
            }
        }

        /// <summary>
        /// 拡張レイヤーの生成
        /// </summary>
        /// <param name="additive">加算レイヤーか</param>
        /// <param name="avatarMask">アバターマスク</param>
        /// <param name="weight">初期ウェイト</param>
        public MotionHandle CreateExtensionLayer(bool additive = false, AvatarMask avatarMask = null, float weight = 1.0f) {
            if (_disposed) {
                return default;
            }

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
        /// 生成済みの拡張レイヤー用を取得
        /// </summary>
        public MotionHandle GetExtensionLayer(int index) {
            if (_disposed) {
                return default;
            }

            if (index < 0 || index >= _extensionCrossFaders.Count) {
                return default;
            }

            var crossFader = _extensionCrossFaders[index];
            return new MotionHandle(this, crossFader);
        }

        /// <summary>
        /// 拡張レイヤーの削除
        /// </summary>
        /// <param name="handle">対象のレイヤーを表すHandle</param>
        public void RemoveExtensionLayer(MotionHandle handle) {
            if (_disposed) {
                return;
            }

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
            if (_disposed) {
                return;
            }

            for (var i = _extensionCrossFaders.Count - 1; i >= 0; i--) {
                var fader = _extensionCrossFaders[i];
                _playable.DisconnectInput(i + 1);
                fader.Dispose();
            }

            _extensionCrossFaders.Clear();
        }

        /// <summary>
        /// レイヤーウェイトの変更
        /// </summary>
        /// <param name="handle">対象のレイヤーを表すHandle</param>
        /// <param name="weight">ウェイト</param>
        public void SetLayerWeight(MotionHandle handle, float weight) {
            if (_disposed) {
                return;
            }

            if (!handle.IsValid) {
                return;
            }

            var index = _extensionCrossFaders.IndexOf(handle.CrossFader);

            // 含まれていなければ何もしない
            if (index < 0) {
                return;
            }

            // ウェイトの変更
            _playable.SetInputWeight(index + 1, weight);
        }

        /// <summary>
        /// レイヤーウェイトの取得
        /// </summary>
        /// <param name="handle">対象のレイヤーを表すHandle</param>
        public float GetLayerWeight(MotionHandle handle) {
            if (_disposed) {
                return 0.0f;
            }

            if (!handle.IsValid) {
                return 0.0f;
            }

            var index = _extensionCrossFaders.IndexOf(handle.CrossFader);

            // 含まれていなければ何もしない
            if (index < 0) {
                return 0.0f;
            }

            // ウェイトの取得
            return _playable.GetInputWeight(index + 1);
        }
    }
}
