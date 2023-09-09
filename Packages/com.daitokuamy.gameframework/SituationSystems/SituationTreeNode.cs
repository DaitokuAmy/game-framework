using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報格納用ツリー用ノード
    /// </summary>
    public class SituationTreeNode : IDisposable {
        private readonly Dictionary<Type, SituationTreeNode> _children = new();

        private SituationTree _tree;
        private Situation _situation;
        private SituationTreeNode _parent;

        /// <summary>有効か</summary>
        public bool IsValid => _tree != null && _situation != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="tree">SituationNode管理用ツリー</param>
        /// <param name="situation">遷移時に使うSituation</param>
        /// <param name="parent">接続親のNode</param>
        internal SituationTreeNode(SituationTree tree, Situation situation, SituationTreeNode parent) {
            _tree = tree;
            _situation = situation;
            _parent = parent;

            if (_situation is INodeSituation nodeSituation) {
                nodeSituation.OnRegisterTree(tree);
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (!IsValid) {
                return;
            }
            
            // ContainerのStackにあった場合は除外
            _tree.Container.Remove(_situation);
            
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
                nodeSituation.OnUnregisterTree(_tree);
            }

            _parent = null;
            _children.Clear();
            _situation = null;
            _tree = null;
        }

        /// <summary>
        /// シチュエーションノードの追加
        /// </summary>
        /// <param name="situation">追加するSituationのインスタンス</param>
        public void AddNode(Situation situation) {
            var type = situation.GetType();
            
            // 既に同じTypeがあった場合は何もしない
            if (_children.TryGetValue(type, out var node)) {
                return;
            }

            // ノードの追加
            node = new SituationTreeNode(_tree, situation, this);
            _children.Add(type, node);
        }

        /// <summary>
        /// シチュエーションノードの削除
        /// </summary>
        public bool RemoveNode<T>()
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
        /// 遷移に使用するSituationを取得
        /// </summary>
        /// <returns></returns>
        internal Situation GetSituation() {
            return _situation;
        }

        /// <summary>
        /// 親のNodeを取得
        /// </summary>
        internal SituationTreeNode GetParent() {
            return _parent;
        }

        /// <summary>
        /// 該当の型の子Nodeを取得
        /// </summary>
        internal SituationTreeNode TryGetChild(Type type) {
            if (!_children.TryGetValue(type, out var child)) {
                return null;
            }

            return child;
        }
    }
}