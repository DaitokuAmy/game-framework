using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.SituationSystems;
using UnityEngine;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// SituationService
    /// </summary>
    public partial class SituationService : DisposableFixedUpdatableTask {
        private readonly DisposableScope _scope;
        private readonly SituationContainer _situationContainer;
        private readonly SituationTree _situationTree;

        /// <summary>現在のNodeが対象としているSituationを取得する</summary>
        public Situation CurrentNodeSituation => _situationTree.Current;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SituationService() {
            _scope = new DisposableScope();
            _situationContainer = new SituationContainer().RegisterTo(_scope);
            _situationTree = new SituationTree(_situationContainer).RegisterTo(_scope);
        }

        /// <inheritdoc/>
        protected override void DisposeInternal() {
            _scope.Dispose();
        }

        /// <inheritdoc/>
        protected override void UpdateInternal() {
            _situationContainer.Update();
        }

        /// <inheritdoc/>
        protected override void LateUpdateInternal() {
            _situationContainer.LateUpdate();
        }

        /// <inheritdoc/>
        protected override void FixedUpdateInternal() {
            _situationContainer.FixedUpdate();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize() {
            if (_scope.Disposed) {
                Debug.LogError("SituationService is disposed");
                return;
            }

            if (_situationContainer == null || _situationTree == null) {
                Debug.LogError("SituationContainer or SituationFlow is null");
                return;
            }

            SetupContainer(_situationContainer, _scope);
            SetupTree(_situationTree, _scope);
            SetupDebug(_scope);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public TransitionHandle Transition(Type type, Action<Situation> onSetup = null, TransitionType transitionType = TransitionType.ScreenDefault, bool refresh = false) {
            var (transition, effects) = GetTransitionInfo(transitionType);
            if (refresh) {
                return _situationTree.RefreshTransition(type, onSetup, transition, effects);
            }

            return _situationTree.Transition(type, onSetup, transition, effects);
        }

        /// <summary>
        /// 指定したSituationへ遷移する
        /// </summary>
        public TransitionHandle Transition<T>(Action<T> onSetup = null, TransitionType transitionType = TransitionType.ScreenDefault, bool refresh = false) where T : Situation {
            var (transition, effects) = GetTransitionInfo(transitionType);
            if (refresh) {
                return _situationTree.RefreshTransition(onSetup, transition, effects);
            }

            return _situationTree.Transition(onSetup, transition, effects);
        }

        /// <summary>
        /// 戻り遷移
        /// </summary>
        public TransitionHandle Back(Action<Situation> onSetup = null, TransitionType transitionType = TransitionType.ScreenDefault) {
            var (transition, effects) = GetTransitionInfo(transitionType);
            return _situationTree.Back(onSetup, transition, effects);
        }

        /// <summary>
        /// 戻り遷移
        /// </summary>
        public TransitionHandle Back(int depth, Action<Situation> onSetup = null, TransitionType transitionType = TransitionType.ScreenDefault) {
            var (transition, effects) = GetTransitionInfo(transitionType);
            return _situationTree.Back(depth, onSetup, transition, effects);
        }

        /// <summary>
        /// 現在のSituationNodeをリセット
        /// </summary>
        public TransitionHandle Reset(Action<Situation> onSetup = null) {
            var (_, effects) = GetTransitionInfo(TransitionType.SceneDefault);
            return _situationTree.Reset(onSetup, effects);
        }

        /// <summary>
        /// ノードの接続
        /// </summary>
        private SituationTreeNode ConnectNode<T>(SituationTreeNode parentNode)
            where T : Situation {
            if (parentNode == null) {
                return _situationTree.ConnectRoot<T>();
            }

            return parentNode.Connect<T>();
        }
    }
}