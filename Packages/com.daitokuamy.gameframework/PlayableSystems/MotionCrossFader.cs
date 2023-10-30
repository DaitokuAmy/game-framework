using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// Motionをクロスフェード再生させるクラス
    /// </summary>
    internal class MotionCrossFader : IDisposable {
        /// <summary>
        /// 再生中情報
        /// </summary>
        private struct PlayingInfo {
            public IPlayableComponent component;
            public int inputPort;
            public float time;
            public float blendTimer;

            /// <summary>
            /// 廃棄のTry
            /// </summary>
            public bool TryDispose(bool force = false) {
                if (component == null || component.IsDisposed) {
                    return false;
                }

                if (!component.AutoDispose && !force) {
                    return false;
                }

                component.Dispose();
                component = null;
                return true;
            }
        }

        // 登録されているGraph
        private PlayableGraph _graph;
        // 再生に使うミキサー
        private AnimationMixerPlayable _mixer;

        // カレントな再生中情報
        private PlayingInfo _currentPlayingInfo;
        // フェードアウト中の再生中情報リスト
        private List<PlayingInfo> _outPlayingInfos = new();

        // 再生に使う時間情報
        private float _blendDuration;
        private float _blendTime;
        private float _prevTime;
        private float _currentTime;
        
        // ワーク領域のWeightリスト
        private List<float> _workWeights = new();

        /// <summary>有効か</summary>
        public bool IsValid => Playable.IsValid();
        /// <summary>再生に使うPlayable</summary>
        public Playable Playable => _mixer;
        /// <summary>アニメーター</summary>
        public Animator Animator { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MotionCrossFader(PlayableGraph graph, Animator animator) {
            _graph = graph;
            Animator = animator;

            // 基本Layerの作成
            _mixer = AnimationMixerPlayable.Create(graph, 2);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 再生情報の削除
            foreach (var info in _outPlayingInfos) {
                info.TryDispose();
            }

            _outPlayingInfos.Clear();
            _currentPlayingInfo.TryDispose();

            // Mixerの削除
            _mixer.Destroy();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            if (!IsValid) {
                return;
            }

            // レイヤーの更新
            UpdateInternal(deltaTime);
        }

        /// <summary>
        /// 再生対象のPlayableProviderを変更
        /// </summary>
        /// <param name="component">変更対象のPlayableを返すProvider</param>
        /// <param name="blendDuration">ブレンド時間</param>
        public void Change(IPlayableComponent component, float blendDuration) {
            if (!IsValid) {
                return;
            }

            // 無効なProvider
            if (component != null && component.IsDisposed) {
                Debug.LogWarning($"Provider is invalid. [{component}]");
                return;
            }

            // 現在再生なComponentと同じならスキップ
            if (_currentPlayingInfo.component == component) {
                return;
            }

            blendDuration = Mathf.Max(0.0f, blendDuration);

            // フェードアウト中の物に含まれていたら除外
            var prevInputPort = -1;
            if (component != null) {
                for (var i = _outPlayingInfos.Count - 1; i >= 0; i--) {
                    if (_outPlayingInfos[i].component == component) {
                        prevInputPort = _outPlayingInfos[i].inputPort;
                        _outPlayingInfos.RemoveAt(i);
                        break;
                    }
                }
            }

            // 現在のComponentをOutリストに移行
            if (_currentPlayingInfo.component != null) {
                _currentPlayingInfo.blendTimer = blendDuration;
                _outPlayingInfos.Add(_currentPlayingInfo);
            }

            // 現在のComponentを更新
            _currentPlayingInfo.component = component;
            _currentPlayingInfo.time = 0.0f;
            _currentPlayingInfo.blendTimer = blendDuration;
            
            if (component != null) {
                // 未初期化だった場合は、初期化
                if (!component.IsInitialized) {
                    component.Initialize(_graph);
                }
                
                // Speedの適用
                component.SetSpeed((float)Playable.GetSpeed());
            }

            // Graphの更新
            if (component != null) {
                // 既に繋がれていたなら、再利用
                if (prevInputPort >= 0) {
                    _currentPlayingInfo.inputPort = prevInputPort;
                }
                else {
                    var inputIndex = _mixer.AddInput(component.GetPlayable(), 0);
                    _mixer.SetInputWeight(inputIndex, 0.0f);
                    _currentPlayingInfo.inputPort = inputIndex;
                }
            }

            // ブレンド時間更新
            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                var info = _outPlayingInfos[i];
                info.blendTimer = Mathf.Min(_outPlayingInfos[i].blendTimer, blendDuration);
                _outPlayingInfos[i] = info;
            }
            
            // InputPortのリフレッシュ
            RefreshInputPorts();
        }

        /// <summary>
        /// 速度の変更
        /// </summary>
        /// <param name="speed"></param>
        public void SetSpeed(float speed) {
            // Mixerの速度変更
            Playable.SetSpeed(speed);
            
            // コンポーネント更新
            if (_currentPlayingInfo.component != null) {
                _currentPlayingInfo.component.SetSpeed(speed);
            }
            
            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                _outPlayingInfos[i].component.SetSpeed(speed);
            }
        }

        /// <summary>
        /// InputPortのリフレッシュ
        /// </summary>
        private void RefreshInputPorts() {
            _workWeights.Clear();
            
            if (_currentPlayingInfo.component != null) {
                _workWeights.Add(_mixer.GetInputWeight(_currentPlayingInfo.inputPort));
                _mixer.DisconnectInput(_currentPlayingInfo.inputPort);
            }

            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                _workWeights.Add(_mixer.GetInputWeight(_outPlayingInfos[i].inputPort));
                _mixer.DisconnectInput(_outPlayingInfos[i].inputPort);
            }
            
            _mixer.SetInputCount(_workWeights.Count);

            var index = 0;
            if (_currentPlayingInfo.component != null) {
                _mixer.ConnectInput(index, _currentPlayingInfo.component.GetPlayable(), 0);
                _mixer.SetInputWeight(index, _workWeights[index]);
                _currentPlayingInfo.inputPort = index;
                index++;
            }

            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                var info = _outPlayingInfos[i];
                _mixer.ConnectInput(index, info.component.GetPlayable(), 0);
                _mixer.SetInputWeight(index, _workWeights[index]);
                info.inputPort = index;
                _outPlayingInfos[i] = info;
                index++;
            }
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        private void UpdateInternal(float deltaTime) {
            // PlayingInfoの更新
            void UpdatePlayingInfo(ref PlayingInfo info, bool fadeIn) {
                if (info.blendTimer >= 0.0f) {
                    // Blend
                    var fadeRate = info.blendTimer > 0.01f ? Mathf.Min(1.0f, deltaTime / info.blendTimer) : 1.0f;
                    info.blendTimer -= deltaTime;
                    var weight = _mixer.GetInputWeight(info.inputPort);
                    weight = Mathf.Lerp(weight, fadeIn ? 1.0f : 0.0f, fadeRate);
                    _mixer.SetInputWeight(info.inputPort, weight);
                }

                // Time
                info.time += deltaTime;
            }

            // PlayableComponentの更新
            void UpdateComponent(IPlayableComponent provider, float time) {
                if (provider == null) {
                    return;
                }

                provider.SetTime(time);
                provider.Update(deltaTime);
            }

            // 再生情報の更新
            for (var i = _outPlayingInfos.Count - 1; i >= 0; i--) {
                var info = _outPlayingInfos[i];

                // 廃棄チェック
                if (info.component.IsDisposed) {
                    _mixer.DisconnectInput(info.inputPort);
                    _outPlayingInfos.RemoveAt(i);
                }
                // 更新
                else {
                    UpdatePlayingInfo(ref info, false);

                    // フェード完了したら除外
                    if (info.blendTimer <= 0.0f) {
                        info.TryDispose();
                        _mixer.DisconnectInput(info.inputPort);
                        _outPlayingInfos.RemoveAt(i);
                    }
                    else {
                        _outPlayingInfos[i] = info;
                    }
                }
            }

            if (_currentPlayingInfo.component != null) {
                // 廃棄チェック
                if (_currentPlayingInfo.component.IsDisposed) {
                    _mixer.DisconnectInput(_currentPlayingInfo.inputPort);
                    _currentPlayingInfo.component = null;
                }
                // 更新
                else {
                    UpdatePlayingInfo(ref _currentPlayingInfo, true);
                }
            }

            // コンポーネント更新
            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                UpdateComponent(_outPlayingInfos[i].component, _outPlayingInfos[i].time);
            }

            UpdateComponent(_currentPlayingInfo.component, _currentPlayingInfo.time);
        }
    }
}