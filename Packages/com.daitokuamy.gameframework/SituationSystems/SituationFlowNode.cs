using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報格納用ツリー用ノード
    /// </summary>
    public class SituationFlowNode : IDisposable {
        private readonly Dictionary<Type, SituationFlowNode> _children = new();

        private SituationFlow _flow;
        private Situation _situation;
        private SituationFlowNode _parent;

        /// <summary>有効か</summary>
        public bool IsValid => _flow != null && _situation != null;
        /// <summary>実行対象のSituation</summary>
        internal Situation Situation => _situation;
        /// <summary>Situationが含まれているContainer</summary>
        internal SituationContainer Container => _situation.ParentContainer;

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
                nodeSituation.OnRegisterTree(flow);
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
                _parent._children.Remove(GetType());
            }
            
            // 子要素を削除
            var children = _children.Values.ToArray();
            foreach (var child in children) {
                child.Dispose();
            }

            // 登録解除通知
            if (_situation is INodeSituation nodeSituation) {
                nodeSituation.OnUnregisterTree(_flow);
            }

            _parent = null;
            _children.Clear();
            _situation = null;
            _flow = null;
        }

        /// <summary>
        /// シチュエーションノードの接続
        /// </summary>
        /// <param name="situation">追加するSituationのインスタンス</param>
        public SituationFlowNode Connect(Situation situation) {
            var type = situation.GetType();
            
            // 既に同じTypeがあった場合は何もしない
            if (_children.TryGetValue(type, out var node)) {
                return node;
            }

            // ノードの追加
            node = new SituationFlowNode(_flow, situation, this);
            _children.Add(type, node);
            return node;
        }

        /// <summary>
        /// シチュエーションノードの接続解除
        /// </summary>
        public bool Disconnect<T>()
            where T : Situation {
            var type = typeof(T);
            
            // 該当のTypeがなければ何もしない
            if (!_children.TryGetValue(type, out var node)) {
                return false;
            }

            // ノードの削除
            node.Dispose();
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
            if (!_children.TryGetValue(type, out var child)) {
                return null;
            }

            return child;
        }
    }
}