using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報格納用ツリー用ノード
    /// </summary>
    public class SituationFlowNode : IDisposable {
        /// <summary>
        /// 遷移情報
        /// </summary>
        public struct TransitionInfo {
            public SituationFlowNode PrevNode;
            public SituationFlowNode NextNode;
            public bool Back;
        }
        
        /// <summary>
        /// 接続情報
        /// </summary>
        private class ConnectInfo {
            public Action<TransitionInfo> TransitionEvent;
            public SituationFlowNode NextNode;
        }

        private readonly Dictionary<Type, ConnectInfo> _connectInfos = new();

        private SituationFlow _flow;
        private Situation _situation;
        private SituationFlowNode _previous;

        /// <summary>有効か</summary>
        public bool IsValid => _flow != null && _situation != null;
        /// <summary>実行対象のSituation</summary>
        public Situation Situation => _situation;

        /// <summary>遷移先のノードリスト</summary>
        internal SituationFlowNode[] NextNodes => _connectInfos.Select(x => x.Value.NextNode).ToArray();

        /// <summary>フォールバック経由で遷移された時の通知</summary>
        public event Action<TransitionInfo> TransitionByFallbackEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="flow">SituationNode管理用ツリー</param>
        /// <param name="situation">遷移時に使うSituation</param>
        /// <param name="prev">接続元のNode</param>
        internal SituationFlowNode(SituationFlow flow, Situation situation, SituationFlowNode prev) {
            _flow = flow;
            _situation = situation;
            _previous = prev;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (!IsValid) {
                return;
            }

            // 前要素から参照を削除
            if (_previous != null) {
                _previous._connectInfos.Remove(GetType());
            }

            // 次要素を削除
            var infos = _connectInfos.Values.ToArray();
            foreach (var info in infos) {
                if (info == null) {
                    continue;
                }

                if (info.NextNode != null) {
                    info.NextNode.Dispose();
                }
            }

            _previous = null;
            _connectInfos.Clear();
            _situation = null;
            _flow = null;
        }

        /// <summary>
        /// シチュエーションノードの接続
        /// </summary>
        /// <param name="overridePrevNode">戻り先ノードの上書き指定</param>
        /// <param name="onTransition">遷移時の通知</param>
        public SituationFlowNode Connect<TSituation>(SituationFlowNode overridePrevNode, Action<TransitionInfo> onTransition = null)
            where TSituation : Situation {
            var type = typeof(TSituation);
            
            // Situationが無ければnullを返す
            var situation = _flow.FindSituation(type);
            if (situation == null) {
                Debug.LogError($"Not found situation. [{type.Name}]");
                return null;
            }

            // 既に同じTypeがあった場合は何もしない
            if (_connectInfos.TryGetValue(type, out var connectInfo)) {
                return connectInfo.NextNode;
            }

            // ノードの追加
            var nextNode = new SituationFlowNode(_flow, situation, overridePrevNode ?? this);
            connectInfo = new ConnectInfo {
                TransitionEvent = onTransition,
                NextNode = nextNode
            };
            _connectInfos.Add(type, connectInfo);
            return nextNode;
        }

        /// <summary>
        /// シチュエーションノードの接続
        /// </summary>
        /// <param name="onTransition">遷移時の通知</param>
        public SituationFlowNode Connect<TSituation>(Action<TransitionInfo> onTransition = null)
            where TSituation : Situation {
            return Connect<TSituation>(null, onTransition);
        }

        /// <summary>
        /// シチュエーションノードの接続解除
        /// </summary>
        public bool Disconnect<T>()
            where T : Situation {
            var type = typeof(T);

            // 該当のTypeがなければ何もしない
            if (!_connectInfos.TryGetValue(type, out var connectInfo)) {
                return false;
            }

            // ノードの削除
            connectInfo.NextNode.Dispose();
            _connectInfos.Remove(type);
            return true;
        }

        /// <summary>
        /// 前のNodeを取得
        /// </summary>
        internal SituationFlowNode GetPrevious() {
            return _previous;
        }

        /// <summary>
        /// 型をもとに接続先のNodeを取得
        /// </summary>
        internal SituationFlowNode TryGetNext(Type type) {
            if (!_connectInfos.TryGetValue(type, out var connectInfo)) {
                return null;
            }

            return connectInfo.NextNode;
        }

        /// <summary>
        /// 遷移通知
        /// </summary>
        /// <param name="prevNode">遷移前のノード</param>
        /// <param name="back">戻り遷移か</param>
        internal void OnTransition(SituationFlowNode prevNode, bool back) {
            // 遷移元がなければ何もしない
            if (prevNode == null) {
                return;
            }

            // Situation間の遷移以外は何もしない
            if (prevNode.Situation == null || Situation == null) {
                return;
            }

            var transitionInfo = new TransitionInfo {
                PrevNode = prevNode,
                NextNode = this,
                Back = back
            };
            
            // 戻り遷移の場合はConnect情報からイベントを探す
            if (back) {
                if (_connectInfos.TryGetValue(prevNode.Situation.GetType(), out var connectInfo)) {
                    connectInfo.TransitionEvent?.Invoke(transitionInfo);
                }
            }
            // 進む遷移の場合は、前のノードのConnect情報からイベントを探す
            else {
                if (prevNode._connectInfos.TryGetValue(Situation.GetType(), out var connectInfo)) {
                    connectInfo.TransitionEvent?.Invoke(transitionInfo);
                }
                // 接続情報がなく遷移した場合は、Fallback遷移とみなして通知する
                else {
                    TransitionByFallbackEvent?.Invoke(transitionInfo);
                }
            }
        }
    }
}