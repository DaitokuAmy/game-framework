using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報
    /// </summary>
    public class SituationFlow : IDisposable {
        private readonly SituationFlowNode _rootNode;
        private readonly SituationContainer _situationContainer;
        private readonly Dictionary<Type, SituationFlowNode> _globalFallbackNodes = new();
        private readonly Dictionary<SituationFlowNode, Dictionary<Type, SituationFlowNode>> _fallbackNodes = new();

        /// <summary>現在のNode</summary>
        public SituationFlowNode CurrentNode { get; private set; }
        /// <summary>トランジション中か</summary>
        public bool IsTransitioning => _situationContainer.IsTransitioning;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationFlow(SituationContainer container) {
            _rootNode = new SituationFlowNode(this, null, null);
            _situationContainer = container;
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_rootNode == null) {
                return;
            }

            // 各種Nodeの開放
            _rootNode.Dispose();
        }

        /// <summary>
        /// Rootに接続する
        /// </summary>
        /// <returns>接続したSituationを保持するNode</returns>
        public SituationFlowNode ConnectRoot<TSituation>()
            where TSituation : Situation {
            var node = _rootNode.Connect<TSituation>();
            SetFallbackNode(node);
            return node;
        }

        /// <summary>
        /// Rootから接続を解除する
        /// </summary>
        /// <returns>解除に成功したか</returns>
        public bool DisconnectRoot<T>()
            where T : Situation {
            return _rootNode.Disconnect<T>();
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, null, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return Transition<TSituation>(null, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<TSituation>(Action<TSituation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            var type = typeof(TSituation);
            return Transition(type, situation => onSetup?.Invoke((TSituation)situation), overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="type">遷移先を表すSituatonのType</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 同じ型なら何もしない
            if (CurrentNode != null && CurrentNode.Situation.GetType() == type) {
                return TransitionHandle.Empty;
            }

            // 遷移先Nodeの取得
            var nextNode = GetNextNode(type);

            if (nextNode == null) {
                Debug.LogError($"Not found situation tree node. [{type.Name}]");
                return TransitionHandle.Empty;
            }

            return Transition(nextNode, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition(SituationFlowNode nextNode, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return TransitionInternal(nextNode, false, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle RefreshTransition<TSituation>(params ITransitionEffect[] effects)
            where TSituation : Situation {
            return RefreshTransition<TSituation>(null, null, effects);
        }

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle RefreshTransition<TSituation>(ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            return RefreshTransition<TSituation>(null, overrideTransition, effects);
        }

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle RefreshTransition<TSituation>(Action<TSituation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where TSituation : Situation {
            var type = typeof(TSituation);
            return RefreshTransition(type, situation => onSetup?.Invoke((TSituation)situation), overrideTransition, effects);
        }

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="type">遷移先を表すSituatonのType</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle RefreshTransition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 同じ型なら何もしない
            if (CurrentNode != null && CurrentNode.Situation.GetType() == type) {
                return TransitionHandle.Empty;
            }

            // 遷移先Nodeの取得
            var nextNode = GetNextNode(type);

            if (nextNode == null) {
                Debug.LogError($"Not found situation tree node. [{type.Name}]");
                return TransitionHandle.Empty;
            }

            return RefreshTransition(nextNode, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// リフレッシュ遷移実行
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle RefreshTransition(SituationFlowNode nextNode, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return TransitionInternal(nextNode, true, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(int depth, params ITransitionEffect[] effects) {
            return Back(depth, null, null, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(params ITransitionEffect[] effects) {
            return Back(null, null, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(int depth, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return Back(depth, null, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return Back(null, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return Back(1, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(int depth, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            return BackInternal(depth, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 現在のSituationをリセットする
        /// </summary>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Reset(params ITransitionEffect[] effects) {
            return Reset(null, effects);
        }

        /// <summary>
        /// 現在のSituationをリセットする
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Reset(Action<Situation> onSetup = null, params ITransitionEffect[] effects) {
            return ResetInternal(onSetup, effects);
        }

        /// <summary>
        /// FallbackNodeの設定
        /// </summary>
        /// <param name="node">Fallback指定するノード</param>
        /// <param name="rootNode">Fallback対象とするNodeの基点(nullだとグローバル)</param>
        public void SetFallbackNode(SituationFlowNode node, SituationFlowNode rootNode = null) {
            if (node == null) {
                return;
            }

            var situation = node.Situation;
            if (situation == null) {
                return;
            }

            if (rootNode == null) {
                _globalFallbackNodes[situation.GetType()] = node;
            }
            else {
                if (!_fallbackNodes.TryGetValue(rootNode, out var dict)) {
                    dict = new Dictionary<Type, SituationFlowNode>();
                    _fallbackNodes[rootNode] = dict;
                }

                dict[situation.GetType()] = node;
            }
        }

        /// <summary>
        /// FallbackNodeの設定
        /// </summary>
        public void ResetFallbackNode<T>()
            where T : Situation {
            var type = typeof(T);
            _globalFallbackNodes.Remove(type);
            foreach (var dict in _fallbackNodes.Values) {
                dict.Remove(type);
            }
        }

        /// <summary>
        /// FallbackNodeの設定全解除
        /// </summary>
        public void ResetFallbackNodes() {
            _globalFallbackNodes.Clear();
            _fallbackNodes.Clear();
        }

        /// <summary>
        /// 存在するノードの一覧を取得
        /// </summary>
        public SituationFlowNode[] GetNodes() {
            var nodes = new List<SituationFlowNode>();

            void AddNodes(SituationFlowNode node) {
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
        /// 次の接続先に存在するタイプかチェック
        /// </summary>
        /// <param name="type">接続先の型</param>
        /// <param name="includeFallback">fallbackに設定された物をチェックするか</param>
        public bool CheckTransition(Type type, bool includeFallback = true) {
            var nextNode = CurrentNode.TryGetNext(type);
            if (nextNode != null) {
                return true;
            }

            if (includeFallback) {
                var findRootNode = CurrentNode;
                while (findRootNode != null) {
                    if (_fallbackNodes.TryGetValue(findRootNode, out var dict)) {
                        if (dict.TryGetValue(type, out _)) {
                            return true;
                        }
                    }

                    findRootNode = findRootNode.GetPrevious();
                }

                if (_globalFallbackNodes.TryGetValue(type, out _)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 次の接続先に存在するタイプかチェック
        /// </summary>
        /// <param name="includeFallback">fallbackに設定された物をチェックするか</param>
        public bool CheckTransition<T>(bool includeFallback = true)
            where T : Situation {
            return CheckTransition(typeof(T));
        }

        /// <summary>
        /// 接続可能なSituationを探す
        /// </summary>
        internal Situation FindSituation(Type type) {
            return _situationContainer.FindSituation(type);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="refresh">RootSituationから再構築するか</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        private TransitionHandle TransitionInternal(SituationFlowNode nextNode, bool refresh = false, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                Debug.LogError("In transitioning");
                return TransitionHandle.Empty;
            }

            // 同じ場所なら何もしない
            if (nextNode == CurrentNode) {
                return TransitionHandle.Empty;
            }

            // NextNodeがない
            if (nextNode == null) {
                Debug.LogError("Next node is null.");
                return TransitionHandle.Empty;
            }

            // 現在のNodeを置き換えて遷移する
            CurrentNode = nextNode;

            // 遷移実行
            var option = new SituationContainer.TransitionOption();
            option.Refresh = refresh;
            return _situationContainer.Transition(CurrentNode.Situation.GetType(), onSetup, option, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="depth">何階層戻るか</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        private TransitionHandle BackInternal(int depth, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 階層が無効なら無視
            if (depth <= 0) {
                return TransitionHandle.Empty;
            }
            
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                Debug.LogError("In transitioning");
                return TransitionHandle.Empty;
            }

            if (CurrentNode == null) {
                Debug.LogWarning("Current situation is null.");
                return TransitionHandle.Empty;
            }

            if (CurrentNode == _rootNode || CurrentNode.GetPrevious() == _rootNode) {
                Debug.LogWarning("Current situation is not back.");
                return TransitionHandle.Empty;
            }

            // 戻り先の取得
            var backNode = CurrentNode;
            for (var i = 0; i < depth; i++) {
                var b = backNode.GetPrevious();
                if (b == null || !b.IsValid) {
                    break;
                }

                backNode = b;
            }

            // 遷移先がなければ終わり
            if (CurrentNode == backNode) {
                return TransitionHandle.Empty;
            }

            // 親Nodeがあればそこへ遷移
            CurrentNode = backNode;

            // 遷移実行
            return _situationContainer.Transition(CurrentNode.Situation.GetType(), onSetup, new SituationContainer.TransitionOption { Back = true }, overrideTransition, effects);
        }

        /// <summary>
        /// 現在のSituationをリセットする
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="effects">遷移演出</param>
        private TransitionHandle ResetInternal(Action<Situation> onSetup = null, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                Debug.LogError("In transitioning");
                return TransitionHandle.Empty;
            }

            if (CurrentNode == null) {
                Debug.LogWarning("Current situation is null.");
                return TransitionHandle.Empty;
            }

            // リセット実行
            return _situationContainer.Reset(onSetup, effects);
        }

        /// <summary>
        /// 遷移先のNodeを取得
        /// </summary>
        private SituationFlowNode GetNextNode(Type type) {
            var nextNode = default(SituationFlowNode);

            // 現在のNodeの接続先にあればそこに遷移
            if (CurrentNode != null) {
                nextNode = CurrentNode.TryGetNext(type);
            }

            // 接続先がなければ、Fallback用のNodeを探す
            if (nextNode == null) {
                var findRootNode = CurrentNode;
                while (findRootNode != null) {
                    if (_fallbackNodes.TryGetValue(findRootNode, out var dict)) {
                        if (dict.TryGetValue(type, out nextNode)) {
                            break;
                        }
                    }

                    findRootNode = findRootNode.GetPrevious();
                }

                if (nextNode == null) {
                    _globalFallbackNodes.TryGetValue(type, out nextNode);
                }
            }

            if (nextNode == null || !nextNode.IsValid) {
                return null;
            }

            return nextNode;
        }

        /// <summary>
        /// Node遷移用コルーチン
        /// </summary>
        /// <param name="nextNode">遷移先のNode</param>
        /// <param name="onSetup">初期化アクション</param>
        /// <param name="back">戻り遷移か</param>
        /// <param name="overrideTransition">遷移方法の指定</param>
        /// <param name="effects">遷移時の効果</param>
        private IEnumerator TransitionNodeRoutine(SituationFlowNode nextNode, Action<Situation> onSetup, bool back, ITransition overrideTransition, ITransitionEffect[] effects) {
            var situation = nextNode.Situation;
            yield return _situationContainer.Transition(situation.GetType(), onSetup, new SituationContainer.TransitionOption { Back = back }, overrideTransition, effects);
        }

        /// <summary>
        /// Nodeリセット用コルーチン
        /// </summary>
        /// <param name="onSetup">初期化アクション</param>
        /// <param name="effects">遷移時の効果</param>
        private IEnumerator ResetNodeRoutine(Action<Situation> onSetup, ITransitionEffect[] effects) {
            yield return _situationContainer.Reset(onSetup, effects);
        }
    }
}