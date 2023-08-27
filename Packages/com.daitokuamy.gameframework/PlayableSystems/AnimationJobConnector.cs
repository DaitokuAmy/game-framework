using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GameFramework.PlayableSystems {
    /// <summary>
    /// AnimationJobを再生させるためのクラス
    /// </summary>
    public class AnimationJobConnector : IDisposable {
        // 再生中のProvider情報
        private class PlayingInfo {
            public int order;
            public IAnimationJobProvider provider;
        }

        // Animator
        private Animator _animator;
        // Playable情報
        private PlayableGraph _graph;
        // Output
        private AnimationPlayableOutput _output;
        // NextPlayable
        private Playable _nextPlayable;

        // 再生中のProvider情報
        private List<PlayingInfo> _sortedPlayingInfos = new List<PlayingInfo>();
        private HashSet<IAnimationJobProvider> _providers = new HashSet<IAnimationJobProvider>();

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
            // 登録されているProviderをDisposeする
            foreach (var info in _sortedPlayingInfos) {
                info.provider.Dispose();
            }

            _sortedPlayingInfos.Clear();
            _providers.Clear();

            // 接続を戻す
            _output.SetSourcePlayable(_nextPlayable);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(float deltaTime) {
            // 無効なProviderがいたら削除
            for (var i = _sortedPlayingInfos.Count - 1; i >= 0; i--) {
                var info = _sortedPlayingInfos[i];
                if (info.provider != null && !info.provider.IsDisposed) {
                    continue;
                }

                _providers.Remove(info.provider);
                _sortedPlayingInfos.RemoveAt(i);
                _dirtyGraph = true;
            }

            // Graphの更新
            if (_dirtyGraph) {
                RefreshGraph();
            }

            // Providerの更新
            for (var i = 0; i < _sortedPlayingInfos.Count; i++) {
                _sortedPlayingInfos[i].provider.Update(deltaTime);
            }
        }

        /// <summary>
        /// Providerの設定
        /// </summary>
        public void SetProvider(IAnimationJobProvider provider, int order = 0) {
            if (provider == null || provider.IsDisposed) {
                Debug.LogError($"Failed provider. {provider}");
                return;
            }

            // 既に設定済み
            if (_providers.Contains(provider)) {
                return;
            }

            // Provider初期化
            provider.Initialize(_animator, _graph);

            // 要素の追加
            _providers.Add(provider);
            _sortedPlayingInfos.Add(new PlayingInfo {
                provider = provider,
                order = order
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
            _sortedPlayingInfos.Sort((a, b) => a.order.CompareTo(b.order));

            if (_sortedPlayingInfos.Count > 0) {
                // Output～NextPlayableの間に直列に並べなおす
                var outputPlayable = _sortedPlayingInfos[_sortedPlayingInfos.Count - 1].provider.GetPlayable();
                _output.SetSourcePlayable(outputPlayable);

                for (var i = _sortedPlayingInfos.Count - 2; i >= 0; i--) {
                    var inputPlayable = _sortedPlayingInfos[i].provider.GetPlayable();
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