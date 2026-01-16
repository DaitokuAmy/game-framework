using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.ActorSystems;
using GameFramework.Core;
using GameFramework.NavigationSystems;
using GameFramework.UISystems;
using SampleGame.Infrastructure;
using SampleGame.Infrastructure.Battle;
using SampleGame.Presentation.Battle;
using SampleGame.Application.Battle;
using SampleGame.Domain;
using SampleGame.Domain.Battle;
using SampleGame.Presentation.UITest;
using ThirdPersonEngine;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// Battle用のSceneINavNode
    /// </summary>
    public class BattleSessionNode : SceneSessionNode {
        [Inject]
        private UIManager _uiManager;

        private BattleAppService _battleAppService;

        private int _battleId = 1;
        private int _playerId = 1;

        /// <inheritdoc/>
        protected override string ScenePath => "Assets/SampleGame/Scenes/battle.unity";

        /// <inheritdoc/>
        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);

            SetupInfrastructures(builder);
            SetupManagers(builder);
            SetupDomains(builder);
            SetupApplications(builder);
            SetupFactories(builder);
        }

        /// <inheritdoc/>
        protected override IEnumerator LoadRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.LoadRoutine(handle, scope);

            // UI読み込み
            var tasks = new List<UniTask>();
            tasks.Add(LoadUIAsync(scope, scope.Token));

            yield return UniTask.WhenAll(tasks).ToCoroutine();
        }

        /// <inheritdoc/>
        protected override IEnumerator InitializeRoutine(TransitionHandle<INavNode> handle, IScope scope) {
            yield return base.InitializeRoutine(handle, scope);

            // 初期化処理
            yield return _battleAppService.SetupAsync(_battleId, _playerId, scope.Token).ToCoroutine();

            SetupPresentations(scope);
        }

        /// <inheritdoc/>
        protected override void Terminate(TransitionHandle<INavNode> handle) {
            _battleAppService.Cleanup();

            base.Terminate(handle);
        }

        /// <inheritdoc/>
        protected override void Update() {
            base.Update();

            _battleAppService.UpdateFrame();
        }

        /// <summary>
        /// セットアップ
        /// </summary>
        public void Setup(int battleId, int playerId) {
            _battleId = battleId;
            _playerId = playerId;
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            UniTask LoadAsync(string assetKey) {
                return _uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("battle"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private void SetupInfrastructures(IContainerBuilder builder) {
            builder.Register<IBattleTableRepository, BattleTableRepository>(Lifetime.Singleton);
            builder.Register<IModelRepository, ModelRepository>(Lifetime.Singleton);
            builder.Register<BattleCharacterAssetRepository>(Lifetime.Singleton);
            builder.Register<BodyPrefabRepository>(Lifetime.Singleton);
            builder.Register<EnvironmentSceneRepository>(Lifetime.Singleton);
        }

        /// <summary>
        /// Manager初期化
        /// </summary>
        private void SetupManagers(IContainerBuilder builder) {
            builder.Register<ActorEntityManager>(Lifetime.Singleton);
            // var cameraManager = ServiceResolver.Resolve<CameraManager>();
            // cameraManager.RegisterTask(TaskOrder.Camera);
        }

        /// <summary>
        /// Domain初期化
        /// </summary>
        private void SetupDomains(IContainerBuilder builder) {
            builder.Register<BattleDomainService>(Lifetime.Singleton);
            builder.Register<CharacterDomainService>(Lifetime.Singleton);
        }

        /// <summary>
        /// Application初期化
        /// </summary>
        private void SetupApplications(IContainerBuilder builder) {
            builder.Register(resolver => {
                _battleAppService = resolver.Resolve<BattleAppService>();
                return _battleAppService;
            }, Lifetime.Singleton);
            builder.Register<PlayerAppService>(Lifetime.Singleton);
        }

        /// <summary>
        /// Factory初期化
        /// </summary>
        private void SetupFactories(IContainerBuilder builder) {
            builder.Register<ICharacterActorFactory, CharacterActorFactory>(Lifetime.Singleton);
            builder.Register<IFieldActorFactory, FieldActorFactory>(Lifetime.Singleton);
        }

        /// <summary>
        /// Presentation初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            var overlayUIService = _uiManager.GetService<BattleOverlayUIService>();
            overlayUIService.OverlayScreenContainer.RegisterHandler(LogicUtility.CreateLogic<OverlayUIScreenPresenter>(UpdateOrder.Presenter, ObjectResolver, false, scope));
            LogicUtility.CreateLogic<CameraPresenter>(UpdateOrder.Presenter, ObjectResolver, true, scope);
        }
    }
}