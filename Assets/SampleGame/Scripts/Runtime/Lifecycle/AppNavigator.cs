using System;
using GameFramework;
using GameFramework;
using GameFramework.NavigationSystem;
using SampleGame.Application;
using SampleGame.Presentation;
using UnityEngine;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// アプリ内遷移制御クラス
    /// </summary>
    public partial class AppNavigator : DisposableUpdatable, IAppNavigator {
        private static ITransition OutInTransition => new OutInTransition();
        private static ITransition CrossTransition => new CrossTransition();
        private static ITransitionEffect[] BlockOnlyEffects => new ITransitionEffect[] { new BlockTransitionEffect() };
        private static ITransitionEffect[] LoadingEffects => new ITransitionEffect[] { new BlockTransitionEffect(), new LoadingTransitionEffect() };
        private static ITransitionEffect[] FadeEffects => new ITransitionEffect[] { new BlockTransitionEffect(), new FaderTransitionEffect(Color.black) };

        private DisposableScope _scope;
        private NavigationEngine _engine;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AppNavigator() {
        }

        /// <inheritdoc/>
        TransitionHandle<INavNode> IAppNavigator.Back(int depth) {
            var (transition, effects) = GetDefaultBackTransitionInfo();
            return _engine.Back(depth, null, transition, effects);
        }

        /// <inheritdoc/>
        TransitionHandle<INavNode> IAppNavigator.TransitionTo(int nodeId, bool refresh, Action<INavNode> setupAction) {
            var (transition, effects) = GetDefaultTransitionInfo(nodeId);
            return _engine.TransitionTo(nodeId, new NavNodeTree.TransitionOption(refresh), setupAction, transition, effects);
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
        public void Initialize(IObjectResolver globalResolver) {
            if (_scope?.Disposed ?? false) {
                Debug.LogError("SituationService is disposed");
                return;
            }

            _scope = new DisposableScope();
            _engine = NavigationEngineBuilder.Create()
                .CreateLifecycle<RootNode>(Id.Root, root => {
                    root
                        .AddSession<IntroductionSessionNode>(Id.Introduction, introduction => {
                            introduction.AddScreen<TitleTopScreenNode>(Id.TitleTop)
                                .AddScreen<TitleOptionScreenNode>(Id.TitleOption);
                        })
                        .AddSession<OutGameSessionNode>(Id.OutGame, outGame => {
                            outGame.AddScreen<SortieScreenNode>(Id.Sortie, sortie => {
                                sortie.AddScreen<SortieTopScreenNode>(Id.SortieTop)
                                    .AddScreen<SortieRoleSelectScreenNode>(Id.SortieRoleSelectTop, sortieRoleSelect => {
                                        sortieRoleSelect.AddScreen<SortieRoleInformationScreenNode>(Id.SortieRoleInformation);
                                    })
                                    .AddScreen<SortieMissionSelectScreenNode>(Id.SortieMissionSelect, sortieMissionSelect => {
                                        sortieMissionSelect.AddScreen<SortieDifficultySelectScreenNode>(Id.SortieDifficultySelect);
                                    });
                            });
                        });
                })
                .CreateRouter(container => {
                    return NavNodeTreeRouterBuilder.Create()
                        .AddRoot(Id.TitleTop, titleTop => {
                            titleTop.Connect(Id.TitleOption)
                                .Connect(Id.SortieTop, sortieTop => {
                                    sortieTop.Connect(Id.SortieRoleSelectTop, sortieRoleSelect => {
                                            sortieRoleSelect.Connect(Id.SortieRoleInformation);
                                        })
                                        .Connect(Id.SortieMissionSelect, sortieMissionSelect => {
                                            sortieMissionSelect.Connect(Id.SortieDifficultySelect, sortieDifficultySelect => {
                                                //sortieDifficultySelect.Connect(Id.BattleHud);
                                            });
                                        })
                                        .SetGlobalShortcut();
                                })
                                .SetGlobalShortcut();
                        })
                        .Build(container);
                })
                .Build(globalResolver)
                .RegisterTo(_scope);

            SetupDebug(_scope);
        }

        /// <summary>
        /// デフォルトの遷移情報取得
        /// </summary>
        private (ITransition ITransition, ITransitionEffect[]) GetDefaultTransitionInfo(int nodeId) {
            var transition = CrossTransition;
            var effects = Array.Empty<ITransitionEffect>();

            // SessionNodeに差があるか
            var currentSessionNode = _engine.GetNodeInParent<SessionNode>();
            var nextSessionNode = _engine.GetNodeInParent<SessionNode>(nodeId);
            if (currentSessionNode != nextSessionNode) {
                // OutInTransition, LoadingEffectsにする
                transition = OutInTransition;
                effects = LoadingEffects;
            }

            return (transition, effects);
        }

        /// <summary>
        /// デフォルトの遷移情報取得
        /// </summary>
        private (ITransition ITransition, ITransitionEffect[]) GetDefaultTransitionInfo<TSessionNodeType>()
            where TSessionNodeType : SceneSessionNode {
            var transition = CrossTransition;
            var effects = Array.Empty<ITransitionEffect>();

            // 行き先のSessionNodeを含んでいるか
            var sessionNode = _engine.GetNodeInParent<TSessionNodeType>();
            if (sessionNode == null) {
                // OutInTransition, LoadingEffectsにする
                transition = OutInTransition;
                effects = LoadingEffects;
            }

            return (transition, effects);
        }

        /// <summary>
        /// デフォルトの戻り遷移情報取得
        /// </summary>
        private (ITransition ITransition, ITransitionEffect[]) GetDefaultBackTransitionInfo() {
            var transition = CrossTransition;
            var effects = Array.Empty<ITransitionEffect>();

            // 現在のSessionNodeと戻り先のSessionNodeを比較
            var currentSessionNode = _engine.GetNodeInParent<SceneSessionNode>();
            var backSessionNode = _engine.GetBackNodeInParent<SceneSessionNode>();
            if (currentSessionNode != backSessionNode) {
                // OutInTransition, LoadingEffectsにする
                transition = OutInTransition;
                effects = LoadingEffects;
            }

            return (transition, effects);
        }
    }
}