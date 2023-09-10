using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.CoroutineSystems;
using UnityEngine;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報格納用ツリー
    /// </summary>
    public class SituationTree : IDisposable {
        private readonly Dictionary<Type, SituationTreeNode> _fallbackNodes = new();
        private CoroutineRunner _coroutineRunner;

        /// <summary>ルートとなるNode</summary>
        public SituationTreeNode RootNode { get; }
        /// <summary>遷移用コンテナ</summary>
        public SituationContainer RootContainer { get; }
        /// <summary>現在のNode</summary>
        public SituationTreeNode CurrentNode { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootSituation">RootとなるSituation</param>
        /// <param name="rootContainer">RootSituationを管理するためのContainer</param>
        public SituationTree(Situation rootSituation, SituationContainer rootContainer) {
            RootContainer = rootContainer;
            RootNode = new SituationTreeNode(this, rootSituation, RootContainer, null);
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
        public IProcess SetupAsync(Action<Situation> onSetup = null) {
            CurrentNode = RootNode;
            
            var situation = RootNode.GetSituation();
            onSetup?.Invoke(situation);
            return RootContainer.Transition(situation,
                new SituationContainer.TransitionOption {
                    resetStack = true
                });
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
            var situation = CurrentNode.GetSituation();
            onSetup?.Invoke(situation);
            return _coroutineRunner.StartCoroutine(TransitionNodeRoutine(prevNode, CurrentNode, false));
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
                return RootContainer.Back(new SituationContainer.TransitionOption { resetStack = true, forceBack = true }, overrideTransition, effects);
            }

            // 親Nodeがあればそこへ遷移
            var prevNode = CurrentNode;
            CurrentNode = parentNode;

            // 遷移実行
            return _coroutineRunner.StartCoroutine(TransitionNodeRoutine(prevNode, CurrentNode, true));
        }

        /// <summary>
        /// FallbackNodeの設定
        /// </summary>
        public void SetFallbackNode(SituationTreeNode node) {
            if (node == null) {
                return;
            }

            var situation = node.GetSituation();
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
        /// 遷移先のNodeを取得
        /// </summary>
        private SituationTreeNode GetNextNode(Type type) {
            var nextNode = default(SituationTreeNode);

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
        private IEnumerator TransitionNodeRoutine(SituationTreeNode prevNode, SituationTreeNode nextNode, bool back) {
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
            var targetSituation = nextNode.GetSituation();
            var baseContainer = prevNode.Container;
            var situations = new List<Situation>();
            while (!FindContainer(targetSituation, baseContainer, situations)) {
                // 現階層を閉じる
                yield return baseContainer.Transition(null,
                    new SituationContainer.TransitionOption {
                        forceBack = true,
                        resetStack = true
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
                        resetStack = true
                    });
            }
        }
    }
}