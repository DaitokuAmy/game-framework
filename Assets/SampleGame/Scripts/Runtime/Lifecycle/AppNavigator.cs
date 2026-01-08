using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using SampleGame.Application;
using UnityEngine;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// アプリ内遷移制御クラス
    /// </summary>
    public partial class AppNavigator : DisposableTask, IAppNavigator {
        private readonly DisposableScope _scope;
        private readonly NavigationEngine _engine;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AppNavigator() {
            _scope = new DisposableScope();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize(IObjectResolver globalResolver) {
            // NavigationEngineBuilder.Create()
            //     .CreateLifecycle()
        }

        /// <inheritdoc/>
        IProcess IAppNavigator.Back(int depth, bool cross) {
            // 戻り先がSceneを跨ぐ場合、強制でTransitionTypeをScene用に差し替える
            var currentNode = _situationTreeRouter.CurrentNode;
            var backNode = default(StateTreeNode<Type>);
            for (var i = 0; i < depth; i++) {
                backNode = currentNode.GetPrevious();
            }

            var differentScene = CheckDifferentSceneSituation(currentNode, backNode);
            var transitionType = differentScene ? TransitionType.SceneDefault : (cross ? TransitionType.ScreenCross : TransitionType.ScreenDefault);
            
            // 戻り遷移
            return Back(depth, null, transitionType);
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            _scope.Dispose();
        }

        /// <inheritdoc/>
        protected override void UpdateInternal() {
            _engine?.Update();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize() {
            if (_scope.Disposed) {
                Debug.LogError("SituationService is disposed");
                return;
            }

            if (_situationContainer == null || _situationTreeRouter == null) {
                Debug.LogError("SituationContainer or SituationFlow is null");
                return;
            }

            SetupContainer(_situationContainer, _scope);
            SetupTree(_situationTreeRouter, _scope);
            SetupDebug(_scope);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public TransitionHandle<Situation> Transition(Type type, Action<Situation> setupAction = null, TransitionType transitionType = TransitionType.ScreenDefault, bool refresh = false) {
            var (transition, effects) = GetTransitionInfo(transitionType);
            if (refresh) {
                var option = new SituationContainer.TransitionOption { Refresh = true };
                return _situationTreeRouter.Transition(type, option, TransitionStep.Complete, setupAction, transition, effects);
            }

            return _situationTreeRouter.Transition(type, null, TransitionStep.Complete, setupAction, transition, effects);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public TransitionHandle<Situation> Transition<T>(Action<T> setupAction = null, TransitionType transitionType = TransitionType.ScreenDefault, bool refresh = false) where T : Situation {
            return Transition(typeof(T), s => setupAction?.Invoke((T)s), transitionType, refresh);
        }

        /// <summary>
        /// 戻り遷移
        /// </summary>
        public TransitionHandle<Situation> Back(int depth, Action<Situation> setupAction = null, TransitionType transitionType = TransitionType.ScreenDefault) {
            var (transition, effects) = GetTransitionInfo(transitionType);
            return _situationTreeRouter.Back(depth, null, setupAction, transition, effects);
        }

        /// <summary>
        /// 戻り遷移
        /// </summary>
        public TransitionHandle<Situation> Back(Action<Situation> setupAction = null, TransitionType transitionType = TransitionType.ScreenDefault) {
            return Back(1, setupAction, transitionType);
        }

        /// <summary>
        /// 現在のSituationNodeをリセット
        /// </summary>
        public TransitionHandle<Situation> Reset(Action<Situation> setupAction = null) {
            var (_, effects) = GetTransitionInfo(TransitionType.SceneDefault);
            return _situationTreeRouter.Reset(setupAction, effects);
        }

        /// <summary>
        /// ノードの接続
        /// </summary>
        private StateTreeNode<Type> ConnectNode<T>(StateTreeNode<Type> parentNode)
            where T : Situation {
            if (parentNode == null) {
                return _situationTreeRouter.ConnectRoot(typeof(T));
            }

            return parentNode.Connect(typeof(T));
        }

        /// <summary>
        /// Nodeに含まれているSituationが違うSceneSituationに含まれているかチェック
        /// </summary>
        private bool CheckDifferentSceneSituation(StateTreeNode<Type> nodeA, StateTreeNode<Type> nodeB) {
            // SceneSituationを再起的に探す
            Situation FindSceneSituation(Situation s) {
                if (s == null) {
                    return null;
                }

                if (s is SceneSessionNode) {
                    return s;
                }

                return FindSceneSituation(s.Parent);
            }

            var situationA = _situationContainer.FindSituation(nodeA.Key);
            var situationB = _situationContainer.FindSituation(nodeB.Key);
            var sceneSituationA = FindSceneSituation(situationA);
            var sceneSituationB = FindSceneSituation(situationB);
            return sceneSituationA != sceneSituationB;
        }

        /// <summary>
        /// 現在のSituationが特定のSituation以下にぶら下がっているかチェック
        /// </summary>
        private bool CheckParentSituation<TSituation>()
            where TSituation : Situation {
            var current = _situationTreeRouter.Current;
            if (current == null) {
                return false;
            }

            var s = current;
            while (s.Parent != null) {
                if (s.Parent is TSituation) {
                    return true;
                }

                s = s.Parent;
            }

            return false;
        }
    }
}