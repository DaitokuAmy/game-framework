using System;
using System.Collections.Generic;

namespace GameFramework.SituationSystems {
    /// <summary>
    /// シチュエーション遷移情報格納用ツリー
    /// </summary>
    public class SituationTree : IDisposable {
        private readonly Dictionary<Type, SituationTreeNode> _fallbackNodes = new();
        
        /// <summary>ルートとなるNode</summary>
        public SituationTreeNode RootNode { get; }
        /// <summary>遷移用コンテナ</summary>
        public SituationContainer Container { get; }
        /// <summary>現在のNode</summary>
        public SituationTreeNode CurrentNode { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="container">Situation管理用Container</param>
        /// <param name="rootSituation">RootとなるSituation</param>
        public SituationTree(SituationContainer container, Situation rootSituation) {
            Container = container;
            RootNode = new SituationTreeNode(this, rootSituation, null);
            SetFallbackNode(RootNode);
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        public void Dispose() {
            if (RootNode == null) {
                return;
            }
            
            // 各種Nodeの開放
            RootNode.Dispose();
        }

        /// <summary>
        /// ルートへの遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle TransitionRoot(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            var type = RootNode.GetSituation().GetType();
            return Transition(type, onSetup, overrideTransition, effects);
        }

        /// <summary>
        /// 遷移実行
        /// </summary>
        /// <param name="onSetup">初期化処理</param>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Transition<T>(Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects)
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
        public TransitionHandle Transition(Type type, Action<Situation> onSetup = null, ITransition overrideTransition = null, params ITransitionEffect[] effects) {
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
            
            // Fallbackもなければ遷移失敗
            if (nextNode == null || !nextNode.IsValid) {
                return new TransitionHandle(new KeyNotFoundException($"Situation type is not found. [{type.Name}]"));
            }
            
            // 現在のNodeを置き換えて遷移する
            CurrentNode = nextNode;
            
            // 遷移実行
            var situation = nextNode.GetSituation();
            onSetup?.Invoke(situation);
            return Container.Transition(situation, new SituationContainer.TransitionOption { resetStack = true, forceBack = true}, overrideTransition, effects);
        }

        /// <summary>
        /// 戻り遷移実行
        /// </summary>
        /// <param name="overrideTransition">上書き用の遷移処理</param>
        /// <param name="effects">遷移演出</param>
        public TransitionHandle Back(ITransition overrideTransition = null, params ITransitionEffect[] effects) {
            if (CurrentNode == null) {
                return new TransitionHandle(new Exception("Current situation is null."));
            }
            
            var parentNode = CurrentNode.GetParent();
            if (parentNode == null || !parentNode.IsValid) {
                return Container.Back(new SituationContainer.TransitionOption { resetStack = true, forceBack = true}, overrideTransition, effects);
            }
            
            // 親Nodeがあればそこへ遷移
            var situation = parentNode.GetSituation();
            return Container.Transition(situation, new SituationContainer.TransitionOption { resetStack = true, forceBack = true}, overrideTransition, effects);
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
    }
}