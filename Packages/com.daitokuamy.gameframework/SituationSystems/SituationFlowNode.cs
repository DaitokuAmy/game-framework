using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報格納用ツリー用ノード
    /// </summary>
    public class SituationFlowNode : IDisposable {
        /// <summary>
        /// 遷移情報
        /// </summary>
        public struct TransitionInfo {
            public SituationFlowNode prevNode;
            public SituationFlowNode nextNode;
            public bool back;
        }
        
        /// <summary>
        /// 接続情報
        /// </summary>
        private class ConnectInfo {
            public Action<TransitionInfo> onTransition;
            public SituationFlowNode childNode;
        }

        private readonly Dictionary<Type, ConnectInfo> _connectInfos = new();

        private SituationFlow _flow;
        private Situation _situation;
        private SituationFlowNode _parent;

        /// <summary>有効か</summary>
        public bool IsValid => _flow != null && _situation != null;
        /// <summary>実行対象のSituation</summary>
        internal Situation Situation => _situation;
        /// <summary>Situationが含まれているContainer</summary>
        internal SituationContainer Container => _situation?.ParentContainer;

        /// <summary>フォールバック経由で遷移された時の通知</summary>
        public event Action<TransitionInfo> OnTransitionByFallbackEvent;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="flow">SituationNode管理用ツリー</param>
        /// <param name="situation">遷移時に使うSituation</param>
        /// <param name="parent">接続親のNode</param>
        internal SituationFlowNode(SituationFlow flow, Situation situation, SituationFlowNode parent) {
            _flow = flow;
            _situation = situation;
            _parent = parent;

            if (_situation is INodeSituation nodeSituation) {
                nodeSituation.OnRegisterFlow(flow);
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (!IsValid) {
                return;
            }

            // 親要素から参照を削除
            if (_parent != null) {
                _parent._connectInfos.Remove(GetType());
            }

            // 子要素を削除
            var infos = _connectInfos.Values.ToArray();
            foreach (var info in infos) {
                if (info == null) {
                    continue;
                }

                if (info.childNode != null) {
                    info.childNode.Dispose();
                }
            }

            // 登録解除通知
            if (_situation is INodeSituation nodeSituation) {
                nodeSituation.OnUnregisterFlow(_flow);
            }

            _parent = null;
            _connectInfos.Clear();
            _situation = null;
            _flow = null;
        }

        /// <summary>
        /// シチュエーションノードの接続
        /// </summary>
        /// <param name="situation">追加するSituationのインスタンス</param>
        /// <param name="onTransition">遷移時の通知</param>
        public SituationFlowNode Connect(Situation situation, Action<TransitionInfo> onTransition = null) {
            var type = situation.GetType();

            // 既に同じTypeがあった場合は何もしない
            if (_connectInfos.TryGetValue(type, out var connectInfo)) {
                return connectInfo.childNode;
            }

            // ノードの追加
            var childNode = new SituationFlowNode(_flow, situation, this);
            connectInfo = new ConnectInfo {
                onTransition = onTransition,
                childNode = childNode
            };
            _connectInfos.Add(type, connectInfo);
            return childNode;
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
            connectInfo.childNode.Dispose();
            _connectInfos.Remove(type);
            return true;
        }

        /// <summary>
        /// 親のNodeを取得
        /// </summary>
        internal SituationFlowNode GetParent() {
            return _parent;
        }

        /// <summary>
        /// 該当の型の子Nodeを取得
        /// </summary>
        internal SituationFlowNode TryGetChild(Type type) {
            if (!_connectInfos.TryGetValue(type, out var connectInfo)) {
                return null;
            }

            return connectInfo.childNode;
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
                prevNode = prevNode,
                nextNode = this,
                back = back
            };
            
            // 戻り遷移の場合はConnect情報からイベントを探す
            if (back) {
                if (_connectInfos.TryGetValue(prevNode.Situation.GetType(), out var connectInfo)) {
                    connectInfo.onTransition?.Invoke(transitionInfo);
                }
            }
            // 進む遷移の場合は、前のノードのConnect情報からイベントを探す
            else {
                if (prevNode._connectInfos.TryGetValue(Situation.GetType(), out var connectInfo)) {
                    connectInfo.onTransition?.Invoke(transitionInfo);
                }
                // 接続情報がなく遷移した場合は、Fallback遷移とみなして通知する
                else {
                    OnTransitionByFallbackEvent?.Invoke(transitionInfo);
                }
            }
        }
    }
}