using System;
using GameFramework;
using GameFramework.Core;
using GameFramework.NavigationSystems;
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
        TransitionHandle<INavNode> IAppNavigator.TransitionTo(Type nodeType, bool refresh, Action<INavNode> setupAction) {
            var (transition, effects) = GetDefaultTransitionInfo(nodeType);
            return _engine.TransitionTo(nodeType, new NavNodeTree.TransitionOption { Refresh = refresh, }, setupAction, transition, effects);
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
                .CreateLifecycle(new RootNode(), root => {
                    root
                        .AddSession(new IntroductionSessionNode(), introduction => {
                            introduction.AddScreen(new TitleTopScreenNode())
                                .AddScreen(new TitleOptionScreenNode());
                        })
                        .AddSession(new OutGameSessionNode(), outGame => {
                            outGame.AddScreen(new SortieScreenNode(), sortie => {
                                sortie.AddScreen(new SortieTopScreenNode())
                                    .AddScreen(new SortieRoleSelectScreenNode(), sortieRoleSelect => {
                                        sortieRoleSelect.AddScreen(new SortieRoleInformationScreenNode());
                                    })
                                    .AddScreen(new SortieMissionSelectScreenNode(), sortieMissionSelect => {
                                        sortieMissionSelect.AddScreen(new SortieDifficultySelectScreenNode());
                                    });
                            });
                        })
                        .AddSession(new BattleSessionNode(), battle => {
                            battle.AddScreen(new BattleHudScreenNode(), battleHud => {
                                battleHud.AddScreen(new BattlePauseScreenNode());
                            });
                        });
                })
                .CreateRouter(container => {
                    return NavNodeTreeRouterBuilder.Create()
                        .AddRoot<TitleTopScreenNode>(titleTop => {
                            titleTop.Connect<TitleOptionScreenNode>()
                                .Connect<SortieTopScreenNode>(sortieTop => {
                                    sortieTop.Connect<SortieRoleSelectScreenNode>(sortieRoleSelect => {
                                            sortieRoleSelect.Connect<SortieRoleInformationScreenNode>();
                                        })
                                        .Connect<SortieMissionSelectScreenNode>(sortieMissionSelect => {
                                            sortieMissionSelect.Connect<SortieDifficultySelectScreenNode>(sortieDifficultySelect => {
                                                sortieDifficultySelect.Connect<BattleHudScreenNode>();
                                            });
                                        });
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
        private (ITransition ITransition, ITransitionEffect[]) GetDefaultTransitionInfo(Type nodeType) {
            var transition = CrossTransition;
            var effects = Array.Empty<ITransitionEffect>();

            // SessionNodeに差があるか
            var currentSessionNode = _engine.GetNodeInParent<SessionNode>();
            var nextSessionNode = _engine.GetNodeInParent<SessionNode>(nodeType);
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