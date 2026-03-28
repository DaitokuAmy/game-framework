using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystem {
    /// <summary>
    /// AnimationJobを再生させるためのクラス
    /// </summary>
    public sealed class AnimationJobConnector : IDisposable {
        /// <summary>
        /// 再生中情報
        /// </summary>
        private class PlayingInfo {
            public int Order;
            public IAnimationJobComponent Component;
        }

        // Animator
        private readonly Animator _animator;
        // Playable情報
        private readonly PlayableGraph _graph;
        // Output
        private readonly AnimationPlayableOutput _output;
        // NextPlayable
        private readonly Playable _nextPlayable;

        // 再生中のComponent情報
        private readonly List<PlayingInfo> _sortedPlayingInfos = new();
        private readonly HashSet<IAnimationJobComponent> _components = new();

        // Graph更新フラグ
        private bool _dirtyGraph;

        // 再生速度
        private float _speed = 1.0f;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="animator">Outputを反映させるAnimator</param>
        /// <param name="graph">構築に使うGraph</param>
        /// <param name="outputIndex">接続に使うOutputのIndex</param>
        public AnimationJobConnector(Animator animator, PlayableGraph graph, int outputIndex = 0) {
            _animator = animator;
            _graph = graph;
            _output = (AnimationPlayableOutput)graph.GetOutput(outputIndex);
            _nextPlayable = _output.GetSourcePlayable();
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            // 登録されているComponentをDisposeする
            foreach (var info in _sortedPlayingInfos) {
                info.Component.Dispose();
            }

            _sortedPlayingInfos.Clear();
            _components.Clear();

            // 接続を戻す
            _output.SetSourcePlayable(_nextPlayable);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            // 無効なComponentがいたら削除
            for (var i = _sortedPlayingInfos.Count - 1; i >= 0; i--) {
                var info = _sortedPlayingInfos[i];
                if (info.Component != null && !info.Component.IsDisposed) {
                    continue;
                }

                _components.Remove(info.Component);
                _sortedPlayingInfos.RemoveAt(i);
                _dirtyGraph = true;
            }

            // Graphの更新
            if (_dirtyGraph) {
                RefreshGraph();
            }

            // Componentの更新
            for (var i = 0; i < _sortedPlayingInfos.Count; i++) {
                _sortedPlayingInfos[i].Component.Update(deltaTime);
            }
        }

        /// <summary>
        /// Componentの追加
        /// </summary>
        public void AddComponent(IAnimationJobComponent component, int order = 0) {
            if (component == null || component.IsDisposed) {
                Debug.LogError($"Failed component. {component}");
                return;
            }

            // 既に設定済み
            if (_components.Contains(component)) {
                return;
            }

            // Component初期化
            component.Initialize(_animator, _graph);

            // 要素の追加
            _components.Add(component);
            _sortedPlayingInfos.Add(new PlayingInfo {
                Component = component,
                Order = order
            });

            _dirtyGraph = true;
        }

        /// <summary>
        /// 再生速度の設定
        /// </summary>
        public void SetSpeed(float speed) {
            if (Math.Abs(speed - _speed) <= float.Epsilon) {
                return;
            }

            _speed = Mathf.Max(0.0f, speed);
        }

        /// <summary>
        /// グラフの更新
        /// </summary>
        private void RefreshGraph() {
            // Listを整理
            _sortedPlayingInfos.Sort((a, b) => a.Order.CompareTo(b.Order));

            if (_sortedPlayingInfos.Count > 0) {
                // Output～NextPlayableの間に直列に並べなおす
                var outputPlayable = _sortedPlayingInfos[_sortedPlayingInfos.Count - 1].Component.GetPlayable();
                _output.SetSourcePlayable(outputPlayable);

                for (var i = _sortedPlayingInfos.Count - 2; i >= 0; i--) {
                    var inputPlayable = _sortedPlayingInfos[i].Component.GetPlayable();
                    outputPlayable.DisconnectInput(0);
                    outputPlayable.ConnectInput(0, inputPlayable, 0);
                    outputPlayable = inputPlayable;
                }

                outputPlayable.DisconnectInput(0);
                outputPlayable.ConnectInput(0, _nextPlayable, 0);
            }
            else {
                // 何もなければ何もつなげない
                _output.SetSourcePlayable(_nextPlayable);
            }

            _dirtyGraph = false;
        }
    }
}
