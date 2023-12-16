using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報
    /// </summary>
    public class SituationFlow : IDisposable {
        /// <summary>
        /// 開始用のTransitionEffect
        /// </summary>
        private class EnterTransitionEffect : ITransitionEffect {
            private ITransitionEffect[] _effects;

            public EnterTransitionEffect(ITransitionEffect[] effects) {
                _effects = effects;
            }

            IEnumerator ITransitionEffect.EnterRoutine() {
                yield return new MergedCoroutine(_effects.Select(x => x.EnterRoutine()));
            }

            void ITransitionEffect.Update() {
            }

            IEnumerator ITransitionEffect.ExitRoutine() {
                yield break;
            }
        }

        /// <summary>
        /// 終了用のTransitionEffect
        /// </summary>
        private class ExitTransitionEffect : ITransitionEffect {
            private ITransitionEffect[] _effects;
            private Action _onFinished;

            public ExitTransitionEffect(ITransitionEffect[] effects, Action onFinished) {
                _effects = effects;
                _onFinished = onFinished;
            }

            IEnumerator ITransitionEffect.EnterRoutine() {
                yield break;
            }

            void ITransitionEffect.Update() {
            }

            IEnumerator ITransitionEffect.ExitRoutine() {
                yield return new MergedCoroutine(_effects.Select(x => x.ExitRoutine()));
                _onFinished?.Invoke();
            }
        }

        private readonly Dictionary<Type, SituationFlowNode> _fallbackNodes = new();
        private CoroutineRunner _coroutineRunner;
        private SituationFlowNode _rootNode;
        private List<ITransitionEffect> _activeTransitionEffects = new();

        /// <summary>現在のNode</summary>
        public SituationFlowNode CurrentNode { get; private set; }
        /// <summary>トランジション中か</summary>
        public bool IsTransitioning { get; private set; }

        /// <summary>遷移用コンテナ</summary>
        private SituationContainer RootContainer => _rootNode.Container;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationFlow() {
            _rootNode = new SituationFlowNode(this, null, null);
            _coroutineRunner = new CoroutineRunner();
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (_rootNode == null) {
                return;
            }

            // コルーチンの削除
            _coroutineRunner.Dispose();

            // 各種Nodeの開放
            _rootNode.Dispose();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// Rootに接続する
        /// </summary>
        /// <param name="situation">接続するSituation</param>
        /// <returns>接続したSituationを保持するNode</returns>
        public SituationFlowNode ConnectRoot(Situation situation) {
            var node = _rootNode.Connect(situation);
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
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public IProcess Transition<T>(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
            where T : Situation {
            var type = typeof(T);
            return Transition(type, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="type">遷移先を表すSituatonのType</param>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public IProcess Transition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 同じ型なら何もしない
            if (CurrentNode != null && CurrentNode.Situation.GetType() == type) {
                return AsyncOperationHandle.CompletedHandle;
            }

            // 遷移先Nodeの取得
            var nextNode = GetNextNode(type);

            if (nextNode == null) {
                var exception = new KeyNotFoundException($"Not found situation tree node. [{type.Name}]");
                Debug.LogException(exception);
                return AsyncOperator.CreateAbortedOperator(exception).GetHandle();
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
        public IProcess Transition(SituationFlowNode nextNode, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            // 既に遷移中なら失敗
            if (IsTransitioning) {
                return AsyncOperationHandle.CanceledHandle;
            }

            // 同じ場所なら何もしない
            if (nextNode == CurrentNode) {
                return AsyncOperationHandle.CompletedHandle;
            }

            // NextNodeがない
            if (nextNode == null) {
                var exception = new KeyNotFoundException($"Next node is null.");
                Debug.LogException(exception);
                return AsyncOperator.CreateAbortedOperator(exception).GetHandle();
            }

            // 遷移中フラグをON
            IsTransitioning = true;

            // 現在のNodeを置き換えて遷移する
            var prevNode = CurrentNode;
            CurrentNode = nextNode;

            // 遷移実行
            var situation = CurrentNode.Situation;
            onSetup?.Invoke(situation);
            return _coroutineRunner.StartCoroutine(TransitionNodeRoutine(prevNode, CurrentNode, false, overrideTransition, effects),
                () => IsTransitioning = false,
                () => IsTransitioning = false,
                ex => IsTransitioning = false);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public IProcess Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            if (CurrentNode == null) {
                Debug.LogWarning("Current situation is null.");
                return AsyncOperator.CreateCompletedOperator().GetHandle();
            }

            if (CurrentNode == _rootNode || CurrentNode.GetParent() == _rootNode) {
                Debug.LogWarning("Current situation is not back.");
                return AsyncOperator.CreateCompletedOperator().GetHandle();
            }

            var parentNode = CurrentNode.GetParent();
            if (parentNode == null || !parentNode.IsValid) {
                return RootContainer.Back(new SituationContainer.TransitionOption { clearStack = true, forceBack = true }, overrideTransition, effects);
            }

            // 遷移中フラグをON
            IsTransitioning = true;

            // 親Nodeがあればそこへ遷移
            var prevNode = CurrentNode;
            CurrentNode = parentNode;

            // 遷移実行
            return _coroutineRunner.StartCoroutine(TransitionNodeRoutine(prevNode, CurrentNode, true, overrideTransition, effects),
                () => IsTransitioning = false,
                () => IsTransitioning = false,
                ex => IsTransitioning = false);
        }

        /// <summary>
        /// FallbackNodeの設定
        /// </summary>
        public void SetFallbackNode(SituationFlowNode node) {
            if (node == null) {
                return;
            }

            var situation = node.Situation;
            if (situation == null) {
                return;
            }

            _fallbackNodes[situation.GetType()] = node;
        }

        /// <summary>
        /// FallbackNodeの設定
        /// </summary>
        public void ResetFallbackNode<T>()
            where T : Situation {
            var type = typeof(T);
            _fallbackNodes.Remove(type);
        }

        /// <summary>
        /// FallbackNodeの設定全解除
        /// </summary>
        public void ResetFallbackNodes() {
            _fallbackNodes.Clear();
        }

        /// <summary>
        /// 次の接続先に存在するタイプかチェック
        /// </summary>
        /// <param name="type">接続先の型</param>
        /// <param name="includeFallback">fallbackに設定された物をチェックするか</param>
        public bool CheckTransition(Type type, bool includeFallback = true) {
            var nextNode = CurrentNode.TryGetChild(type);
            if (nextNode != null) {
                return true;
            }

            if (includeFallback) {
                if (_fallbackNodes.TryGetValue(type, out var fallbackNode)) {
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
        /// 遷移先のNodeを取得
        /// </summary>
        private SituationFlowNode GetNextNode(Type type) {
            var nextNode = default(SituationFlowNode);

            // 現在のNodeの接続先にあればそこに遷移
            if (CurrentNode != null) {
                nextNode = CurrentNode.TryGetChild(type);
            }

            // 接続先がなければ、Fallback用のNodeを探す
            if (nextNode == null) {
                if (_fallbackNodes.TryGetValue(type, out var fallbackNode)) {
                    nextNode = fallbackNode;
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
        /// <param name="prevNode"></param>
        /// <param name="nextNode"></param>
        /// <param name="back">戻り遷移か</param>
        /// <param name="overrideTransition">遷移方法の指定</param>
        /// <param name="effects">遷移時の効果</param>
        private IEnumerator TransitionNodeRoutine(SituationFlowNode prevNode, SituationFlowNode nextNode, bool back, ITransition overrideTransition, ITransitionEffect[] effects) {
            // 該当Situationのコンテナ階層にターゲットのコンテナがあるかを探す
            bool FindContainer(Situation situation, SituationContainer container, List<Situation> transitionSituations) {
                if (situation == null || situation.ParentContainer == null) {
                    return false;
                }

                if (situation.ParentContainer == container) {
                    transitionSituations.Add(situation);
                    return true;
                }

                var result = FindContainer(situation.ParentContainer.Owner, container, transitionSituations);
                transitionSituations.Add(situation);
                return result;
            }

            var effectEntered = false;
            var enterEffect = new EnterTransitionEffect(effects);
            var exitEffect = new ExitTransitionEffect(effects, () => _activeTransitionEffects.Clear());

            var targetSituation = nextNode.Situation;
            var baseContainer = prevNode?.Container;
            var situations = new List<Situation>();
            // 遷移元がなければ、開く必要のあるシチュエーションをリスト化
            if (baseContainer == null) {
                var target = targetSituation;
                while (target?.ParentContainer != null && target.ParentContainer.Current != target) {
                    situations.Insert(0, target);
                    target = target.ParentContainer?.Owner;
                }
            }
            // 遷移元があれば、遷移元と遷移先の共通Containerまでさかのぼる
            else {
                while (!FindContainer(targetSituation, baseContainer, situations)) {
                    var first = !effectEntered;
                    effectEntered = true;
                    if (first) {
                        // 独自のTransitionEffect用に更新を回すためのインスタンスキャッシュ
                        _activeTransitionEffects.AddRange(effects);
                    }

                    // 現階層を閉じる
                    yield return baseContainer.Transition(null,
                        new SituationContainer.TransitionOption {
                            forceBack = true,
                            clearStack = true
                        }, overrideTransition: null, first ? new ITransitionEffect[] { enterEffect } : Array.Empty<ITransitionEffect>());

                    // 親のContainerを遷移対象してリトライ
                    baseContainer = baseContainer.Owner.ParentContainer;
                    situations.Clear();
                }
            }

            // 遷移を行う
            for (var i = 0; i < situations.Count; i++) {
                var situation = situations[i];
                var last = i == situations.Count - 1;
                var transitionEffects = Array.Empty<ITransitionEffect>();
                if (effectEntered) {
                    if (last) {
                        transitionEffects = new ITransitionEffect[] { exitEffect };
                    }
                }
                else {
                    transitionEffects = effects;
                }

                yield return situation.ParentContainer.Transition(situation,
                    new SituationContainer.TransitionOption {
                        forceBack = back,
                        clearStack = true
                    }, last ? overrideTransition : null, transitionEffects);
            }
        }
    }
}