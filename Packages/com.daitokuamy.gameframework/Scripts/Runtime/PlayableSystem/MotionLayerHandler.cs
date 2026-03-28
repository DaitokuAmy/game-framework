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
        /// <summary>
        /// 拡張レイヤー情報
        /// </summary>
        private sealed class ExtensionLayerInfo {
            public MotionCrossFader CrossFader;
            public int InputPort;
        }

        private readonly List<ExtensionLayerInfo> _extensionLayerInfos = new();
        private readonly Animator _animator;

        private bool _disposed;
        private PlayableGraph _graph;
        private AnimationLayerMixerPlayable _playable;
        private MotionCrossFader _baseCrossFader;
        private float _speed = 1.0f;

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
            foreach (var info in _extensionLayerInfos) {
                info.CrossFader.Dispose();
            }

            _extensionLayerInfos.Clear();
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
            _baseCrossFader.SetSpeed(_speed);
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
            foreach (var info in _extensionLayerInfos) {
                info.CrossFader.Update(deltaTime);
            }
        }

        /// <summary>
        /// 速度の変更
        /// </summary>
        public void SetSpeed(float speed) {
            if (_disposed) {
                return;
            }

            _speed = Mathf.Max(0.0f, speed);
            _baseCrossFader.SetSpeed(_speed);
            foreach (var info in _extensionLayerInfos) {
                info.CrossFader.SetSpeed(_speed);
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
            crossFader.SetSpeed(_speed);

            var index = (int)_playable.AddInput(crossFader.Playable, 0, weight);
            _playable.SetLayerAdditive((uint)index, additive);
            if (avatarMask != null) {
                _playable.SetLayerMaskFromAvatarMask((uint)index, avatarMask);
            }

            _extensionLayerInfos.Add(new ExtensionLayerInfo {
                CrossFader = crossFader,
                InputPort = index
            });

            return new MotionHandle(this, crossFader);
        }

        /// <summary>
        /// 生成済みの拡張レイヤー用を取得
        /// </summary>
        public MotionHandle GetExtensionLayer(int index) {
            if (_disposed) {
                return default;
            }

            if (index < 0 || index >= _extensionLayerInfos.Count) {
                return default;
            }

            var crossFader = _extensionLayerInfos[index].CrossFader;
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

            var index = FindExtensionLayerIndex(handle.CrossFader);

            // 含まれていなければ何もしない
            if (index < 0) {
                return;
            }

            var info = _extensionLayerInfos[index];

            // 除外
            _extensionLayerInfos.RemoveAt(index);

            // 接続の解除
            _playable.DisconnectInput(info.InputPort);

            // 削除したCrossFaderをDispose
            info.CrossFader.Dispose();
        }

        /// <summary>
        /// 拡張レイヤーの全削除
        /// </summary>
        public void RemoveExtensionLayers() {
            if (_disposed) {
                return;
            }

            for (var i = _extensionLayerInfos.Count - 1; i >= 0; i--) {
                var info = _extensionLayerInfos[i];
                _playable.DisconnectInput(info.InputPort);
                info.CrossFader.Dispose();
            }

            _extensionLayerInfos.Clear();
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

            var index = FindExtensionLayerIndex(handle.CrossFader);

            // 含まれていなければ何もしない
            if (index < 0) {
                return;
            }

            // ウェイトの変更
            _playable.SetInputWeight(_extensionLayerInfos[index].InputPort, weight);
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

            var index = FindExtensionLayerIndex(handle.CrossFader);

            // 含まれていなければ何もしない
            if (index < 0) {
                return 0.0f;
            }

            // ウェイトの取得
            return _playable.GetInputWeight(_extensionLayerInfos[index].InputPort);
        }

        /// <summary>
        /// 拡張レイヤーのIndexを取得
        /// </summary>
        private int FindExtensionLayerIndex(MotionCrossFader crossFader) {
            for (var i = 0; i < _extensionLayerInfos.Count; i++) {
                if (_extensionLayerInfos[i].CrossFader == crossFader) {
                    return i;
                }
            }

            return -1;
        }
    }
}
