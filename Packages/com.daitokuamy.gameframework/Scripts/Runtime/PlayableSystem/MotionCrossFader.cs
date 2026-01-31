using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// Motionをクロスフェード再生させるクラス
    /// </summary>
    internal sealed class MotionCrossFader : IDisposable {
        /// <summary>
        /// 再生中情報
        /// </summary>
        private struct PlayingInfo {
            public Playable? Playable;
            public int InputPort;
            public float Time;
            public float BlendTimer;
            public bool AutoDispose;

            /// <summary>
            /// 終了処理のTry
            /// </summary>
            public bool TryDestroy(bool force = false) {
                if (Playable == null || !Playable.Value.IsValid()) {
                    return false;
                }

                if (!AutoDispose && !force) {
                    return false;
                }

                Playable.Value.Destroy();
                Playable = null;
                return true;
            }
        }

        // 再生に使うミキサー
        private readonly AnimationMixerPlayable _mixer;
        // フェードアウト中の再生中情報リスト
        private readonly List<PlayingInfo> _outPlayingInfos = new();
        // ワーク領域のWeightリスト
        private readonly List<float> _workWeights = new();

        // カレントな再生中情報
        private PlayingInfo _currentPlayingInfo;

        // 再生に使う時間情報
        private float _blendDuration;
        private float _blendTime;
        private float _prevTime;
        private float _currentTime;

        /// <summary>有効か</summary>
        public bool IsValid => Playable.IsValid();
        /// <summary>再生に使うPlayable</summary>
        public Playable Playable => _mixer;
        /// <summary>グラフ情報</summary>
        public PlayableGraph Graph { get; }
        /// <summary>アニメーター</summary>
        public Animator Animator { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MotionCrossFader(PlayableGraph graph, Animator animator) {
            Graph = graph;
            Animator = animator;

            // Mixer作成
            _mixer = AnimationMixerPlayable.Create(graph, 2);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 再生情報の削除
            foreach (var info in _outPlayingInfos) {
                info.TryDestroy();
            }

            _outPlayingInfos.Clear();
            _currentPlayingInfo.TryDestroy();

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
        /// Playableの再生
        /// </summary>
        /// <param name="playable">再生対象のPlayable</param>
        /// <param name="blendDuration">ブレンド時間</param>
        /// <param name="autoDispose">自動廃棄するか</param>
        public void Change(Playable? playable, float blendDuration, bool autoDispose) {
            if (!IsValid) {
                return;
            }

            // 無効なPlayable
            if (playable != null && !playable.Value.IsValid()) {
                Debug.LogWarning($"Playable is invalid. [{playable}]");
                return;
            }

            // 現在再生中のPlayableと同じならスキップ
            if (_currentPlayingInfo.Playable.Equals(playable)) {
                return;
            }

            blendDuration = Mathf.Max(0.0f, blendDuration);

            // フェードアウト中の物に含まれていたら除外
            var prevInputPort = -1;
            if (playable != null) {
                for (var i = _outPlayingInfos.Count - 1; i >= 0; i--) {
                    var outPlayable = _outPlayingInfos[i].Playable;
                    if (outPlayable == null) {
                        continue;
                    }

                    if (outPlayable.Value.Equals(playable.Value)) {
                        prevInputPort = _outPlayingInfos[i].InputPort;
                        _outPlayingInfos.RemoveAt(i);
                        break;
                    }
                }
            }

            // 現在再生中の物をOutリストに移行
            if (_currentPlayingInfo.Playable != null) {
                _currentPlayingInfo.BlendTimer = blendDuration;
                _outPlayingInfos.Add(_currentPlayingInfo);
            }

            // 現在Playable情報を更新
            _currentPlayingInfo.Playable = playable;
            _currentPlayingInfo.Time = 0.0f;
            _currentPlayingInfo.BlendTimer = blendDuration;
            _currentPlayingInfo.AutoDispose = autoDispose;
            _currentPlayingInfo.InputPort = -1;

            // Speedの適用
            if (playable != null) {
                playable.Value.SetSpeed((float)Playable.GetSpeed());
            }

            // Graphの更新
            if (prevInputPort >= 0) {
                _currentPlayingInfo.InputPort = prevInputPort;
            }
            else if (playable != null) {
                var inputIndex = _mixer.AddInput(playable.Value, 0);
                _mixer.SetInputWeight(inputIndex, 0.0f);
                _currentPlayingInfo.InputPort = inputIndex;
            }

            // ブレンド時間更新
            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                var info = _outPlayingInfos[i];
                info.BlendTimer = Mathf.Min(_outPlayingInfos[i].BlendTimer, blendDuration);
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
            if (_currentPlayingInfo.Playable != null) {
                _currentPlayingInfo.Playable.Value.SetSpeed(speed);
            }

            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                if (_outPlayingInfos[i].Playable == null) {
                    continue;
                }

                _outPlayingInfos[i].Playable.Value.SetSpeed(speed);
            }
        }

        /// <summary>
        /// InputPortのリフレッシュ
        /// </summary>
        private void RefreshInputPorts() {
            _workWeights.Clear();

            if (_currentPlayingInfo.Playable != null) {
                _workWeights.Add(_mixer.GetInputWeight(_currentPlayingInfo.InputPort));
                _mixer.DisconnectInput(_currentPlayingInfo.InputPort);
            }

            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                _workWeights.Add(_mixer.GetInputWeight(_outPlayingInfos[i].InputPort));
                _mixer.DisconnectInput(_outPlayingInfos[i].InputPort);
            }

            _mixer.SetInputCount(_workWeights.Count);

            var index = 0;
            if (_currentPlayingInfo.Playable != null) {
                _mixer.ConnectInput(index, _currentPlayingInfo.Playable.Value, 0);
                _mixer.SetInputWeight(index, _workWeights[index]);
                _currentPlayingInfo.InputPort = index;
                index++;
            }

            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                var info = _outPlayingInfos[i];
                if (info.Playable == null) {
                    continue;
                }

                _mixer.ConnectInput(index, info.Playable.Value, 0);
                _mixer.SetInputWeight(index, _workWeights[index]);
                info.InputPort = index;
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
                if (info.BlendTimer >= 0.0f) {
                    // Blend
                    var fadeRate = info.BlendTimer > 0.01f ? Mathf.Min(1.0f, deltaTime / info.BlendTimer) : 1.0f;
                    info.BlendTimer -= deltaTime;
                    var weight = _mixer.GetInputWeight(info.InputPort);
                    weight = Mathf.Lerp(weight, fadeIn ? 1.0f : 0.0f, fadeRate);
                    _mixer.SetInputWeight(info.InputPort, weight);
                }

                // Time
                info.Time += deltaTime;
            }

            // Playableの更新
            void UpdatePlayable(Playable? playable, float time) {
                if (playable == null) {
                    return;
                }

                var playableValue = playable.Value;
                if (!playableValue.IsValid()) {
                    return;
                }

                playableValue.SetTime(time);
            }

            // 再生情報の更新
            for (var i = _outPlayingInfos.Count - 1; i >= 0; i--) {
                var info = _outPlayingInfos[i];

                // 廃棄チェック
                if (!(info.Playable?.IsValid() ?? false)) {
                    _mixer.DisconnectInput(info.InputPort);
                    _outPlayingInfos.RemoveAt(i);
                }
                // 更新
                else {
                    UpdatePlayingInfo(ref info, false);

                    // フェード完了したら除外
                    if (info.BlendTimer <= 0.0f) {
                        info.TryDestroy();
                        _mixer.DisconnectInput(info.InputPort);
                        _outPlayingInfos.RemoveAt(i);
                    }
                    else {
                        _outPlayingInfos[i] = info;
                    }
                }
            }

            if (_currentPlayingInfo.Playable != null) {
                // 廃棄チェック
                if (_currentPlayingInfo.Playable.Value.IsValid()) {
                    _mixer.DisconnectInput(_currentPlayingInfo.InputPort);
                    _currentPlayingInfo.Playable = null;
                }
                // 更新
                else {
                    UpdatePlayingInfo(ref _currentPlayingInfo, true);
                }
            }

            // Playable更新
            for (var i = 0; i < _outPlayingInfos.Count; i++) {
                UpdatePlayable(_outPlayingInfos[i].Playable, _outPlayingInfos[i].Time);
            }

            UpdatePlayable(_currentPlayingInfo.Playable, _currentPlayingInfo.Time);
        }
    }
}