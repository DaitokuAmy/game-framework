using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameFramework {
    /// <summary>
    /// Tree管理用StateRouter
    /// </summary>
    public class StateTreeRouter<TKey, TState, TOption> : IStateRouter<TKey, TState, TOption>
        where TState : class {
        private readonly IStateContainer<TKey, TState, TOption> _stateContainer;
        private readonly Dictionary<TKey, StateTreeNode<TKey>> _globalShortcutNodes = new();
        private readonly Dictionary<StateTreeNode<TKey>, Dictionary<TKey, StateTreeNode<TKey>>> _shortcutNodes = new();
        private readonly string _label;

        private bool _disposed;
        private StateTreeNode<TKey> _rootNode;

        /// <inheritdoc/>
        string IMonitoredStateRouter.Label => _label;
        /// <summary>戻り先の情報</summary>
        string IMonitoredStateRouter.BackStateInfo {
            get {
                var previous = CurrentNode?.GetPrevious();
                if (previous == null) {
                    return "None";
                }

                return previous.IsRoot ? "Root" : previous.Key.ToString();
            }
        }

        /// <inheritdoc/>
        public TState Current => _stateContainer.Current;
        /// <inheritdoc/>
        public TKey CurrentKey => CurrentNode != null ? CurrentNode.Key : default;
        /// <inheritdoc/>
        public bool IsTransitioning => _stateContainer.IsTransitioning;

        /// <summary>現在のNode</summary>
        public StateTreeNode<TKey> CurrentNode { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StateTreeRouter(IStateContainer<TKey, TState, TOption> container, string label = "", [CallerFilePath] string caller = "") {
            _stateContainer = container;
            _label = string.IsNullOrEmpty(label) ? PathUtility.GetRelativePath(caller) : label;
            _rootNode = new StateTreeNode<TKey>(default, null);
            StateMonitor.AddRouter(this);
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }

            _disposed = true;
            StateMonitor.RemoveRouter(this);

            DisposeInternal();

            if (_rootNode != null) {
                _rootNode.Dispose();
                _rootNode = null;
            }

            _shortcutNodes.Clear();
            _globalShortcutNodes.Clear();
        }

        /// <summary>
        /// モニタリング用の詳細情報取得
        /// </summary>
        void IMonitoredStateRouter.GetDetails(List<(string label, string text)> lines) {
            void AddNodeLine(StateTreeNode<TKey> node, StringBuilder indent, string label = "") {
                var key = node.Key != null ? node.Key.ToString() : "Root";
                var current = CurrentNode == node;
                lines.Add((label, $"{indent}{(current ? $"<color=green>{key}</color>" : key)}"));
                for (var i = 0; i < node.NextNodes.Length; i++) {
                    indent.Append("    ");
                    AddNodeLine(node.NextNodes[i], indent);
                    indent.Remove(indent.Length - 4, 4);
                }
            }

            var builder = new StringBuilder();

            // Tree情報
            AddNodeLine(_rootNode, builder, "<Tree>");

            lines.Add(("", ""));

            // Shortcut情報
            var globalShortcutKeys = _globalShortcutNodes.Keys.ToArray();
            lines.Add(("[Base]", "Root"));
            for (var i = 0; i < globalShortcutKeys.Length; i++) {
                lines.Add((i == 0 ? "    <Shortcuts>" : "", globalShortcutKeys[i].ToString()));
            }

            foreach (var pair in _shortcutNodes) {
                void GetPath(StateTreeNode<TKey> node, StringBuilder path) {
                    if (node == null || !node.IsValid || node.IsRoot) {
                        return;
                    }

                    var key = node.Key;
                    path.Insert(0, path.Length > 0 ? $"{key}/" : key);
                    GetPath(node.GetPrevious(), path);
                }

                lines.Add(("", ""));

                builder.Clear();
                GetPath(pair.Key, builder);
                lines.Add(("[Base]", builder.ToString()));

                var shortcutKeys = pair.Value.Keys.ToArray();
                for (var i = 0; i < shortcutKeys.Length; i++) {
                    lines.Add((i == 0 ? "    <Shortcuts>" : "", shortcutKeys[i].ToString()));
                }
            }
        }

        /// <inheritdoc/>
        public TKey[] GetStateKeys() {
            return GetNodes().Select(x => x.Key).Distinct().ToArray();
        }

        /// <inheritdoc/>
        public TKey GetBackStateKey(int depth = 1) {
            if (CurrentNode == null) {
                return default;
            }

            // 戻り先のノードを取得
            var backNode = CurrentNode;
            for (var i = 0; i < depth; i++) {
                var b = backNode.GetPrevious();
                if (b == null || !b.IsValid) {
                    break;
                }

                backNode = b;
            }

            return backNode.Key;
        }

        /// <summary>
        /// 廃棄時処理
        /// </summary>
        protected virtual void DisposeInternal() {
        }

        /// <summary>
        /// 存在するノードの一覧を取得
        /// </summary>
        public StateTreeNode<TKey>[] GetNodes() {
            var nodes = new List<StateTreeNode<TKey>>();

            void AddNodes(StateTreeNode<TKey> node) {
                if (node == null) {
                    return;
                }

                nodes.Add(node);
                foreach (var nextNode in node.NextNodes) {
                    AddNodes(nextNode);
                }
            }

            AddNodes(_rootNode);

            // Rootは除外
            nodes.RemoveAt(0);

            return nodes.ToArray();
        }

        /// <summary>
        /// Rootに接続する
        /// </summary>
        /// <returns>接続したKeyを保持するNode</returns>
        public StateTreeNode<TKey> ConnectRoot(TKey key) {
            var node = _rootNode.Connect(key);
            SetShortcutNode(node);
            return node;
        }

        /// <summary>
        /// Rootから接続を解除する
        /// </summary>
        /// <returns>解除に成功したか</returns>
        public bool DisconnectRoot(TKey key) {
            return _rootNode.Disconnect(key);
        }

        /// <summary>
        /// ShortcutNodeの設定
        /// </summary>
        /// <param name="node">Shortcut指定するノード</param>
        /// <param name="baseNode">Shortcut対象とするNodeの基点(nullだとグローバル)</param>
        public void SetShortcutNode(StateTreeNode<TKey> node, StateTreeNode<TKey> baseNode = null) {
            if (node == null || !node.IsValid || node.IsRoot) {
                return;
            }

            if (baseNode == null) {
                _globalShortcutNodes[node.Key] = node;
            }
            else {
                if (!_shortcutNodes.TryGetValue(baseNode, out var dict)) {
                    dict = new Dictionary<TKey, StateTreeNode<TKey>>();
                    _shortcutNodes[baseNode] = dict;
                }

                dict[node.Key] = node;
            }
        }

        /// <summary>
        /// ShortcutNodeのリセット
        /// </summary>
        public void ResetShortcutNode(TKey key) {
            _globalShortcutNodes.Remove(key);
            foreach (var dict in _shortcutNodes.Values) {
                dict.Remove(key);
            }
        }

        /// <summary>
        /// ShortcutNodeの設定全解除
        /// </summary>
        public void ResetShortcutNodes() {
            _globalShortcutNodes.Clear();
            _shortcutNodes.Clear();
        }

        /// <summary>
        /// 次の接続先に存在するタイプかチェック
        /// </summary>
        /// <param name="key">接続先を表すキー</param>
        /// <param name="includeShortcut">shortcutに設定された物をチェックするか</param>
        public bool CheckTransition(TKey key, bool includeShortcut = true) {
            var nextNode = CurrentNode.TryGetNext(key);
            if (nextNode != null) {
                return true;
            }

            if (includeShortcut) {
                var findRootNode = CurrentNode;
                while (findRootNode != null) {
                    if (_shortcutNodes.TryGetValue(findRootNode, out var dict)) {
                        if (dict.TryGetValue(key, out _)) {
                            return true;
                        }
                    }

                    findRootNode = findRootNode.GetPrevious();
                }

                if (_globalShortcutNodes.TryGetValue(key, out _)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="key">遷移ターゲットを決めるキー</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<TState> TransitionTo(TKey key, TOption option = default, Action<TState> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects) {
            // 遷移先Nodeの取得
            var nextNode = GetNextNode(key);
            return TransitionInternal(nextNode, option, false, setupAction, transition, effects);
        }

        /// <summary>
        /// 遷移処理
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<TState> TransitionTo(StateTreeNode<TKey> nextNode, TOption option = default, Action<TState> setupAction = null, ITransition transition = null,
            params ITransitionEffect[] effects) {
            return TransitionInternal(nextNode, option, false, setupAction, transition, effects);
        }

        /// <summary>
        /// 戻り遷移処理
        /// </summary>
        /// <param name="depth">戻り階層数(1～)</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<TState> Back(int depth = 1, TOption option = default, Action<TState> setupAction = null, ITransition transition = null, params ITransitionEffect[] effects) {
            // 階層が無効なら無視
            if (depth <= 0) {
                return TransitionHandle<TState>.Empty;
            }

            // 戻れる位置じゃなければ無視
            if (CurrentNode == null || CurrentNode == _rootNode || CurrentNode.GetPrevious() == _rootNode) {
                return TransitionHandle<TState>.Empty;
            }

            // 戻り先のノードを取得
            var backNode = CurrentNode;
            for (var i = 0; i < depth; i++) {
                var b = backNode.GetPrevious();
                if (b == null || !b.IsValid) {
                    break;
                }

                backNode = b;
            }

            return TransitionInternal(backNode, option, true, setupAction, transition, effects);
        }

        /// <summary>
        /// 状態リセット
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="effects">遷移時演出</param>
        public TransitionHandle<TState> Reset(Action<TState> setupAction = null, params ITransitionEffect[] effects) {
            return ResetInternal(setupAction, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="option">遷移時に渡すオプション</param>
        /// <param name="back">戻りか</param>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="transition">遷移方法</param>
        /// <param name="effects">遷移時演出</param>
        private TransitionHandle<TState> TransitionInternal(StateTreeNode<TKey> nextNode, TOption option, bool back, Action<TState> setupAction, ITransition transition,
            params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                var ex = new Exception("In transitioning");
                DebugLog.Exception(ex);
                return new TransitionHandle<TState>(ex);
            }

            // NextNodeがない
            if (nextNode == null) {
                var ex = new Exception("Next node is null.");
                DebugLog.Exception(ex);
                return new TransitionHandle<TState>(ex);
            }

            // 同じ場所なら何もしない
            if (nextNode == CurrentNode) {
                return TransitionHandle<TState>.Empty;
            }

            // 現在のNodeを置き換えて遷移する
            CurrentNode = nextNode;

            // 遷移実行
            return _stateContainer.TransitionTo(nextNode.Key, option, back, setupAction, transition, effects);
        }

        /// <summary>
        /// 現在のStateをリセットする
        /// </summary>
        /// <param name="setupAction">遷移先初期化用関数</param>
        /// <param name="effects">遷移時演出</param>
        private TransitionHandle<TState> ResetInternal(Action<TState> setupAction, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                return new TransitionHandle<TState>(new Exception("In transitioning"));
            }

            if (CurrentNode == null) {
                return TransitionHandle<TState>.Empty;
            }

            // リセット実行
            return _stateContainer.Reset(setupAction, effects);
        }

        /// <summary>
        /// 遷移先のNodeを取得
        /// </summary>
        private StateTreeNode<TKey> GetNextNode(TKey key) {
            var nextNode = default(StateTreeNode<TKey>);

            // 現在のNodeの接続先にあればそこに遷移
            if (CurrentNode != null) {
                nextNode = CurrentNode.TryGetNext(key);
            }

            // 接続先がなければ、Shortcut用のNodeを探す
            if (nextNode == null) {
                var findRootNode = CurrentNode;
                while (findRootNode != null) {
                    if (_shortcutNodes.TryGetValue(findRootNode, out var dict)) {
                        if (dict.TryGetValue(key, out nextNode)) {
                            break;
                        }
                    }

                    findRootNode = findRootNode.GetPrevious();
                }

                if (nextNode == null) {
                    _globalShortcutNodes.TryGetValue(key, out nextNode);
                }
            }

            if (nextNode == null || !nextNode.IsValid) {
                return null;
            }

            return nextNode;
        }
    }
}