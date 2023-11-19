using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報
    /// </summary>
    public class SituationFlow : IDisposable {
        private readonly Dictionary<Type, SituationFlowNode> _fallbackNodes = new();
        private CoroutineRunner _coroutineRunner;

        /// <summary>ルートとなるNode</summary>
        public SituationFlowNode RootNode { get; }
        /// <summary>現在のNode</summary>
        public SituationFlowNode CurrentNode { get; private set; }
        
        /// <summary>遷移用コンテナ</summary>
        private SituationContainer RootContainer => RootNode.Container;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootSituation">RootとなるSituation</param>
        public SituationFlow(Situation rootSituation) {
            RootNode = new SituationFlowNode(this, rootSituation, null);
            SetFallbackNode(RootNode);
            _coroutineRunner = new CoroutineRunner();
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (RootNode == null) {
                return;
            }
            
            // コルーチンの削除
            _coroutineRunner.Dispose();

            // 各種Nodeの開放
            RootNode.Dispose();
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update() {
            _coroutineRunner.Update();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="transitionEffects">遷移効果</param>
        public IProcess SetupAsync(Action<Situation> onSetup = null, params ITransitionEffect[] transitionEffects) {
            CurrentNode = RootNode;
            var situation = RootNode.Situation;
            onSetup?.Invoke(situation);
            return RootContainer.Transition(situation, transitionEffects);
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

            // 現在のNodeを置き換えて遷移する
            var prevNode = CurrentNode;
            CurrentNode = nextNode;

            // 遷移実行
            var situation = CurrentNode.Situation;
            onSetup?.Invoke(situation);
            return _coroutineRunner.StartCoroutine(TransitionNodeRoutine(prevNode, CurrentNode, false, overrideTransition, effects));
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

            if (CurrentNode == RootNode) {
                Debug.LogWarning("Root node is not back.");
                return AsyncOperator.CreateCompletedOperator().GetHandle();
            }

            var parentNode = CurrentNode.GetParent();
            if (parentNode == null || !parentNode.IsValid) {
                return RootContainer.Back(new SituationContainer.TransitionOption { clearStack = true, forceBack = true }, overrideTransition, effects);
            }

            // 親Nodeがあればそこへ遷移
            var prevNode = CurrentNode;
            CurrentNode = parentNode;

            // 遷移実行
            return _coroutineRunner.StartCoroutine(TransitionNodeRoutine(prevNode, CurrentNode, true, overrideTransition, effects));
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
        private IEnumerator TransitionNodeRoutine(SituationFlowNode prevNode, SituationFlowNode nextNode, bool back, ITransition overrideTransition, ITransitionEffect[] effects) {
            if (prevNode == null) {
                Debug.LogError("Failed prevNode. prevNode is null.");
                yield break;
            }
            
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

            // 遷移先NodeとContainerと同じContainerになるまで親をさかのぼる
            var targetSituation = nextNode.Situation;
            var baseContainer = prevNode.Container;
            var situations = new List<Situation>();
            while (!FindContainer(targetSituation, baseContainer, situations)) {
                // 現階層を閉じる
                yield return baseContainer.Transition(null,
                    new SituationContainer.TransitionOption {
                        forceBack = true,
                        clearStack = true
                    });
                
                // 親のContainerを遷移対象してリトライ
                baseContainer = baseContainer.Owner.ParentContainer;
                situations.Clear();
            }
            
            // 遷移を行う
            foreach (var situation in situations) {
                yield return situation.ParentContainer.Transition(situation,
                    new SituationContainer.TransitionOption {
                        forceBack = back,
                        clearStack = true
                    }, overrideTransition, effects);
            }
        }
    }
}