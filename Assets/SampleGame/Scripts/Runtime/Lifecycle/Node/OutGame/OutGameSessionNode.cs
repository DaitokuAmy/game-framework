using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameFramework;
using GameFramework.ActorSystem;
using GameFramework;
using GameFramework.NavigationSystem;
using GameFramework.UISystem;
using SampleGame.Domain;
using SampleGame.Infrastructure;
using SampleGame.Presentation.OutGame;
using ThirdPersonEngine;
using VContainer;

namespace SampleGame.Lifecycle {
    /// <summary>
    /// OutGame用のSessionNode
    /// </summary>
    public class OutGameSessionNode : SceneSessionNode {
        [Inject]
        private UIManager _uiManager;
        
        /// <inheritdoc/>
        protected override string ScenePath => "Assets/SampleGame/Scenes/out_game.unity";

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
            //yield return _outGameAppService.SetupAsync(_battleId, _playerId, scope.Token).ToCoroutine();
            
            SetupPresentations(scope);
        }

        /// <summary>
        /// UIの読み込み
        /// </summary>
        private UniTask LoadUIAsync(IScope unloadScope, CancellationToken ct) {
            UniTask LoadAsync(string assetKey) {
                return _uiManager.LoadSceneAsync(assetKey).RegisterTo(unloadScope).ToUniTask(cancellationToken: ct);
            }

            return UniTask.WhenAll(LoadAsync("out_game"));
        }

        /// <summary>
        /// Infrastructure初期化
        /// </summary>
        private void SetupInfrastructures(IContainerBuilder builder) {
            builder.Register<IModelRepository, ModelRepository>(Lifetime.Singleton);
            builder.Register<BodyPrefabRepository>(Lifetime.Singleton);
            builder.Register<EnvironmentSceneRepository>(Lifetime.Singleton);
        }

        /// <summary>
        /// Manager初期化
        /// </summary>
        private void SetupManagers(IContainerBuilder builder) {
            //builder.Register<ActorEntityManager>(Lifetime.Singleton);
            //var cameraManager = ServiceResolver.Resolve<CameraManager>();
            //cameraManager.RegisterTask(TaskOrder.Camera);
        }

        /// <summary>
        /// Domain初期化
        /// </summary>
        private void SetupDomains(IContainerBuilder builder) {
            // builder.Register<OutGameDomainService>(Lifetime.Singleton);
            // builder.Register<CharacterDomainService>(Lifetime.Singleton);
        }

        /// <summary>
        /// Application初期化
        /// </summary>
        private void SetupApplications(IContainerBuilder builder) {
            // builder.Register<OutGameAppService>(Lifetime.Singleton);
        }

        /// <summary>
        /// Factory初期化
        /// </summary>
        private void SetupFactories(IContainerBuilder builder) {
            // builder.Register<ICharacterActorFactory, CharacterActorFactory>(Lifetime.Singleton);
            // builder.Register<IFieldActorFactory, FieldActorFactory>(Lifetime.Singleton);
        }

        /// <summary>
        /// Presentation初期化
        /// </summary>
        private void SetupPresentations(IScope scope) {
            var sortieUIService = _uiManager.GetService<SortieUIService>();
            sortieUIService.BaseScreen.RegisterHandler(LogicUtility.CreateLogic<SortieBasePresenter>(ObjectResolver, false, scope));
            sortieUIService.TopScreen.RegisterHandler(LogicUtility.CreateLogic<SortieTopPresenter>(ObjectResolver, false, scope));
            sortieUIService.RoleSelectScreen.RegisterHandler(LogicUtility.CreateLogic<SortieRoleSelectPresenter>(ObjectResolver, false, scope));
            sortieUIService.RoleInformationScreen.RegisterHandler(LogicUtility.CreateLogic<SortieRoleInformationPresenter>(ObjectResolver, false, scope));
            sortieUIService.MissionSelectScreen.RegisterHandler(LogicUtility.CreateLogic<SortieMissionSelectPresenter>(ObjectResolver, false, scope));
            sortieUIService.DifficultySelectScreen.RegisterHandler(LogicUtility.CreateLogic<SortieDifficultySelectPresenter>(ObjectResolver, false, scope));
        }
    }
}